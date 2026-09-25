using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RadishDeck.Core;
using RadishDeck.Core.Actions;
using RadishDeck.Core.Models;
using RadishDeck.Desktop.Services;
using RadishDeck.Infrastructure;
using CoreAction = RadishDeck.Core.Models.Action;
using DeckPage = RadishDeck.Core.Models.Page;

namespace RadishDeck.Desktop;

public partial class MainWindow : Window
{
    private readonly ServerLauncherService _server = new();
    private readonly DeckJsonStore _store = new();
    private readonly ServerSettingsStore _settingsStore = new();
    private readonly ActionRegistry _registry;
    private readonly ActionDispatcher _dispatcher;
    private Deck _deck = new();
    private ServerSettings _serverSettings = ServerSettings.Defaults;
    private DeckPage? _page;
    private Element? _selected;
    private bool _loaded;
    private bool _closing;
    private bool _canClose;
    private long _actionSequence;
    private long _saveSequence;
    private readonly HashSet<Guid> _executing = new();

    public MainWindow()
    {
        InitializeComponent();
        _registry = DesktopActions.Create(_server, () =>
            ServerConfiguration.Create(IpAddressTextBox.Text, PortTextBox.Text));
        _dispatcher = new ActionDispatcher(_registry);
        ActionComboBox.ItemsSource = _registry.Definitions;
        _server.StateChanged += OnServerStateChanged;
        Closing += OnClosing;
        Loaded += OnLoaded;
        RenderServer();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        string? settingsError = null;
        try
        {
            var settings = await _settingsStore.LoadAsync();
            _serverSettings = settings;
            IpAddressTextBox.Text = settings.IpAddress;
            PortTextBox.Text = settings.Port.ToString();
        }
        catch (Exception exception)
        {
            // Server settings are optional for Deck editing. Keep defaults and continue loading the Deck.
            settingsError = "Settings Error: " + exception.Message;
            SaveStatusText.Text = settingsError;
        }

        try
        {
            _deck = await _store.LoadAsync();
            _loaded = true;
            BindPages();
            SelectPage(_deck.Pages[0]);
            EditorPanel.IsEnabled = !_closing;
            SaveStatusText.Text = settingsError is null
                ? "Deck loaded: " + _store.FilePath
                : settingsError + " Deck loaded: " + _store.FilePath;
        }
        catch (Exception exception)
        {
            // Keep the editor disabled and never save over an unreadable file, including on exit.
            SaveStatusText.Text = exception.Message;
        }
    }

    private void BindPages()
    {
        PagesList.ItemsSource = null;
        PagesList.ItemsSource = _deck.Pages;
        DeletePageButton.IsEnabled = _deck.Pages.Count > 1;
    }

    private void SelectPage(DeckPage page)
    {
        _page = page;
        _selected = null;
        CurrentPageText.Text = PageNameTextBox.Text = page.Name;
        PagesList.SelectedItem = page;
        RenderDeck();
        RenderProperties();
    }

    private void PageSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PagesList.SelectedItem is DeckPage page && page != _page) SelectPage(page);
    }

    private void RenderDeck()
    {
        DeckGrid.Children.Clear();
        DeckGrid.RowDefinitions.Clear();
        DeckGrid.ColumnDefinitions.Clear();
        for (var i = 0; i < DeckEditor.GridSize; i++)
        {
            DeckGrid.RowDefinitions.Add(new RowDefinition());
            DeckGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        if (_page is null) return;
        foreach (var element in _page.Elements)
        {
            var button = new Button
            {
                Content = element.Name, Tag = element, FontSize = 17,
                BorderBrush = Brushes.Crimson, BorderThickness = new Thickness(element == _selected ? 3 : 1)
            };
            button.Click += ElementClick;
            Grid.SetColumn(button, element.GridX);
            Grid.SetRow(button, element.GridY);
            Grid.SetColumnSpan(button, element.GridWidth);
            Grid.SetRowSpan(button, element.GridHeight);
            DeckGrid.Children.Add(button);
        }
    }

    private void ElementClick(object sender, RoutedEventArgs e)
    {
        _selected = ((FrameworkElement)sender).Tag as Element;
        RenderDeck();
        RenderProperties();
    }

    private void RenderProperties()
    {
        PropertiesPanel.IsEnabled = _selected is not null && !_closing;
        if (_selected is null) return;
        ElementNameTextBox.Text = _selected.Name;
        ActionComboBox.SelectedValue = _selected.ActionId;
        UrlTextBox.Text = _selected.Url;
        ProcessPathTextBox.Text = _selected.Url;
        XTextBox.Text = _selected.GridX.ToString();
        YTextBox.Text = _selected.GridY.ToString();
        WidthTextBox.Text = _selected.GridWidth.ToString();
        HeightTextBox.Text = _selected.GridHeight.ToString();
        ExecuteElementButton.IsEnabled = !_executing.Contains(_selected.Id);
    }

    private void ActionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var actionId = ActionComboBox.SelectedValue as string;
        UrlPanel.Visibility = actionId == "url.open" ? Visibility.Visible : Visibility.Collapsed;
        ProcessPathPanel.Visibility = actionId == "process.start" ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool ApplyProperties()
    {
        if (_page is null || _selected is null) return true;
        try
        {
            if (ActionComboBox.SelectedValue is not string id || !_registry.TryGet(id, out _, out _))
                throw new ArgumentException("Select a registered action");
            if (!int.TryParse(XTextBox.Text, out var x) || !int.TryParse(YTextBox.Text, out var y) ||
                !int.TryParse(WidthTextBox.Text, out var width) || !int.TryParse(HeightTextBox.Text, out var height))
                throw new ArgumentException("Position and size must be integers");
            var value = id == "process.start" ? ProcessPathTextBox.Text.Trim() : UrlTextBox.Text.Trim();
            DeckEditor.UpdateElement(_page, _selected, ElementNameTextBox.Text, id, value, x, y, width, height);
            RenderDeck();
            return true;
        }
        catch (Exception exception)
        {
            ActionResultText.Text = "Edit Error: " + exception.Message;
            return false;
        }
    }

    private async void ApplyElement_Click(object sender, RoutedEventArgs e)
    {
        if (ApplyProperties()) await SaveAsync();
    }

    private async void AddPage_Click(object sender, RoutedEventArgs e)
    {
        var page = DeckEditor.AddPage(_deck);
        BindPages();
        SelectPage(page);
        await SaveAsync();
    }

    private async void RenamePage_Click(object sender, RoutedEventArgs e)
    {
        if (_page is null) return;
        try
        {
            DeckEditor.RenamePage(_page, PageNameTextBox.Text);
            BindPages();
            PagesList.SelectedItem = _page;
            CurrentPageText.Text = _page.Name;
            await SaveAsync();
        }
        catch (Exception exception) { ActionResultText.Text = "Edit Error: " + exception.Message; }
    }

    private async void DeletePage_Click(object sender, RoutedEventArgs e)
    {
        if (_page is null) return;
        try
        {
            DeckEditor.DeletePage(_deck, _page);
            BindPages();
            SelectPage(_deck.Pages[0]);
            await SaveAsync();
        }
        catch (Exception exception) { ActionResultText.Text = "Edit Error: " + exception.Message; }
    }

    private async void AddElement_Click(object sender, RoutedEventArgs e)
    {
        if (_page is null) return;
        try
        {
            _selected = DeckEditor.AddButton(_page);
            RenderDeck();
            RenderProperties();
            await SaveAsync();
        }
        catch (Exception exception) { ActionResultText.Text = "Edit Error: " + exception.Message; }
    }

    private async void DeleteElement_Click(object sender, RoutedEventArgs e)
    {
        if (_page is null || _selected is null) return;
        _page.Elements.Remove(_selected);
        _selected = null;
        RenderDeck();
        RenderProperties();
        await SaveAsync();
    }

    private async Task<bool> SaveAsync()
    {
        if (!_loaded) return true;
        var sequence = ++_saveSequence;
        try
        {
            await _store.SaveAsync(_deck);
            if (sequence == _saveSequence) SaveStatusText.Text = "Saved: " + _store.FilePath;
            return true;
        }
        catch (Exception exception)
        {
            if (sequence == _saveSequence) SaveStatusText.Text = "Save Error: " + exception.Message;
            return false;
        }
    }

    private async Task ExecuteAsync(Element element, string actionId)
    {
        if (_closing || !_executing.Add(element.Id)) return;
        var sequence = ++_actionSequence;
        ActionResultText.Text = $"Action {actionId}: executing";
        try
        {
            // Both bottom controls and Deck use the same dispatcher and the same server service.
            var execution = _dispatcher.ExecuteAsync(element, new CoreAction { Type = actionId });
            RenderServer();
            var result = await execution;
            if (sequence == _actionSequence)
                ActionResultText.Text = $"Action {actionId}: {result.Status} — {result.Message}";
        }
        finally
        {
            _executing.Remove(element.Id);
            RenderServer();
            ExecuteElementButton.IsEnabled = _selected is not null && !_executing.Contains(_selected.Id) && !_closing;
        }
    }

    private async void ExecuteElement_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || !ApplyProperties()) return;
        var element = _selected;
        ExecuteElementButton.IsEnabled = false;
        await SaveAsync();
        await ExecuteAsync(element, element.ActionId);
    }

    private void OnServerStateChanged()
    {
        if (!Dispatcher.HasShutdownStarted) Dispatcher.BeginInvoke(new System.Action(RenderServer));
    }

    private void RenderServer()
    {
        var state = _server.Snapshot;
        ServerStatusText.Text = state.Status.ToString();
        ServerMessageText.Text = state.Message;
        ServerVersionText.Text = state.Version ?? AppVersion.Current;
        DiagnosticsText.Text = _server.Diagnostics;
        var editable = state.CanEditConfiguration && !_closing;
        IpAddressTextBox.IsEnabled = PortTextBox.IsEnabled = editable;
        StartServerButton.IsEnabled = editable;
        RestartServerButton.IsEnabled = !_closing && state.Status is ServerLifecycle.Running or ServerLifecycle.Error && state.Configuration is not null;
        StopServerButton.IsEnabled = !_closing && state.CanStop;
    }

    private async void StartServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        await ExecuteAsync(new Element(), "server.start");
        if (_server.Snapshot.Status == ServerLifecycle.Running)
            await SaveServerSettingsAsync();
    }

    private async void RestartServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_closing) return;
        ActionResultText.Text = "Action server.restart: executing";
        var result = await _server.RestartAsync();
        ActionResultText.Text = $"Action server.restart: {result.Status} — {result.Message}";
        if (result.Status == ServerLifecycle.Running.ToString()) await SaveServerSettingsAsync();
    }

    private async void StopServerButton_OnClick(object sender, RoutedEventArgs e) =>
        await ExecuteAsync(new Element(), "server.stop");

    private async Task SaveServerSettingsAsync()
    {
        try
        {
            var configuration = _server.Snapshot.Configuration;
            if (configuration is null) return;
            await _settingsStore.SaveAsync(new ServerSettings
            {
                IpAddress = configuration.IpAddress,
                Port = configuration.Port,
                Name = _serverSettings.Name
            });
        }
        catch (Exception exception) { SaveStatusText.Text = "Settings Error: " + exception.Message; }
    }

    private void ActionsTab_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show(this, string.Join(Environment.NewLine, _registry.Definitions.Select(a => $"{a.Id} — {a.Name}")), "Action Registry");

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_canClose) return;
        e.Cancel = true;
        if (_closing) return;
        _closing = true;
        EditorPanel.IsEnabled = false;
        RenderServer();
        try
        {
            await _server.StopAsync();
            if (_server.Snapshot.OwnsProcess) return;
            if (_loaded && (!ApplyProperties() || !await SaveAsync())) return;
            _server.StateChanged -= OnServerStateChanged;
            await _server.DisposeAsync();
            _canClose = true;
            Close();
        }
        catch (Exception exception) { SaveStatusText.Text = "Close Error: " + exception.Message; }
        finally
        {
            if (!_canClose)
            {
                _closing = false;
                EditorPanel.IsEnabled = _loaded;
                RenderServer();
            }
        }
    }
}
