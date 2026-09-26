using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using RadishDeck.Core.Models;
using RadishDeck.Core.Models.V2;
using RadishDeck.Core.Rendering;
using RadishDeck.Desktop.Services;
using RadishDeck.Infrastructure;

namespace RadishDeck.Desktop;
public partial class MainWindow : Window
{
    private readonly DeckJsonStore _store = new();
    private readonly ServerLauncherService _server = new();
    private readonly ServerSettingsStore _settingsStore = new();
    private ServerSettings _settings = ServerSettings.Defaults;
    private Deck _deck = new();
    private Page? _page;
    private Guid? _selected;
    private bool _ready, _closing, _canClose;
    private string? _error;
    private readonly CanvasProfile _canvas = new() { Name = "Desktop", Width = 1920, Height = 1080, Resolution = "1920x1080" };
    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await InitializeAsync();
        Closing += OnClosing;
        _server.StateChanged += ServerChanged;
    }
    private async Task InitializeAsync()
    {
        try { _settings = await _settingsStore.LoadAsync(); }
        catch (Exception ex) { _error = ex.Message; }
        try
        {
            _deck = await _store.LoadAsync();
            _page = _deck.Pages[0];
            foreach (var element in _deck.Pages.SelectMany(p => p.Elements))
            {
                element.Layout ??= new ElementLayout { X = element.GridX * 100 + 20, Y = element.GridY * 80 + 20, Width = element.GridWidth * 100, Height = element.GridHeight * 60 };
                element.Content ??= new ElementContent { Text = element.Name };
            }
        }
        catch (Exception ex) { _error = ex.Message; }
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Web", "index.html");
            if (!File.Exists(path)) throw new FileNotFoundException("Local Web UI not found", path);
            var environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RadishDeck", "WebView2"));
            await WebView.EnsureCoreWebView2Async(environment);
            // RadishDeck is a desktop host; do not expose the embedded browser's
            // Back/Reload/Save/Inspect context menu to end users.
            WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("radishdeck.local", Path.GetDirectoryName(path)!, CoreWebView2HostResourceAccessKind.DenyCors);
            WebView.CoreWebView2.NavigationStarting += (_, e) => { if (e.Uri != "https://radishdeck.local/index.html") e.Cancel = true; };
            WebView.CoreWebView2.NewWindowRequested += (_, e) => e.Handled = true;
            WebView.WebMessageReceived += OnMessage;
            WebView.CoreWebView2.Navigate("https://radishdeck.local/index.html");
        }
        catch (Exception ex) { MessageBox.Show(this, "WebView2 initialization failed: " + ex.Message, "RadishDeck"); }
    }
    private async void OnMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (_closing || e.Source != "https://radishdeck.local/index.html") return;
        try
        {
            var message = JsonSerializer.Deserialize<BridgeMessage>(e.WebMessageAsJson, WebBridge.Json) ?? throw new ArgumentException("Empty message");
            switch (message.Type)
            {
                case "ready": _ready = true; SendServer(); SendElements(); if (_error != null) SendError(_error); break;
                case "navigation": break;

                case "window.maximize":
                    WindowState = WindowState.Maximized;
                    break;

                case "window.restore":
                     WindowState = WindowState.Normal;
                     break;

                case "server.getState": SendServer(); break;
                case "server.start":
                    if (!_server.Snapshot.CanEditConfiguration) throw new InvalidOperationException("Stop the server before changing configuration");
                    var request = message.Payload.Deserialize<ServerStartRequest>(WebBridge.Json) ?? throw new ArgumentException("Missing configuration");
                    var configuration = ServerConfiguration.Create(request.IpAddress, request.Port);
                    await _server.StartAsync(configuration);
                    if (_server.Snapshot.Status == ServerLifecycle.Running)
                    {
                        _settings.IpAddress = configuration.IpAddress; _settings.Port = configuration.Port;
                        await _settingsStore.SaveAsync(_settings);
                    }
                    SendServer(); break;
                case "server.stop": await _server.StopAsync(); SendServer(); break;
                case "server.restart": await _server.RestartAsync(); SendServer(); break;
                case "server.openWebRuntime": Process.Start(new ProcessStartInfo(AccessUrl()) { UseShellExecute = true }); break;
                case "server.copyUrl": Clipboard.SetText(AccessUrl()); break;
                case "designer.render": SendElements(); break;
                case "designer.selectElement":
                    var id = message.Payload.GetProperty("id").GetGuid();
                    _selected = _page?.Elements.FirstOrDefault(x => x.Id == id)?.Id; break;
                case "designer.addElement":
                    if (_page is null) throw new InvalidOperationException("Deck unavailable");
                    var added = DeckEditor.AddButton(_page);
                    added.Layout = new ElementLayout { X = 80 + added.GridX * 200, Y = 80 + added.GridY * 100, Width = 180, Height = 64 };
                    added.Content = new ElementContent { Text = added.Name }; _selected = added.Id;
                    await _store.SaveAsync(_deck); SendElements(); break;
                case "designer.elementChanged":
                    var move = message.Payload.Deserialize<ElementMove>(WebBridge.Json) ?? throw new ArgumentException("Missing coordinates");
                    var element = _page?.Elements.FirstOrDefault(x => x.Id == move.Id) ?? throw new ArgumentException("Unknown element");
                    var layout = element.Layout!;
                    (layout.X, layout.Y) = WebBridge.Clamp(move.X, move.Y, layout, _canvas);
                    _selected = element.Id;
                    await _store.SaveAsync(_deck); SendElements();
                    Send("designer.saved", new { id = element.Id, x = layout.X, y = layout.Y }); break;
                default: SendError("Unknown message: " + message.Type); break;
            }
        }
        catch (Exception ex) { SendError(ex.Message); }
    }
    private string AccessUrl()
    {
        var state = _server.Snapshot;
        if (state.Status != ServerLifecycle.Running || state.Configuration is null) throw new InvalidOperationException("Server is not running");
        return state.Configuration.Address.AbsoluteUri;
    }
    private void SendElements() => Send("renderElements", new { canvas = _canvas, elements = _page?.Elements.Select(RenderElement.From).ToArray() ?? Array.Empty<RenderElement>(), selectedId = _selected, pageName = _page?.Name, available = _page != null });
    private void ServerChanged() { if (!Dispatcher.HasShutdownStarted) Dispatcher.BeginInvoke(new System.Action(SendServer)); }
    private void SendServer()
    {
        var state = _server.Snapshot;
        var addresses = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork && !a.Address.ToString().StartsWith("169.254."))
            .Select(a => a.Address.ToString()).Append("127.0.0.1").Append(_settings.IpAddress).Distinct().ToArray();
        Send("server.state", new { status = state.Status.ToString(), state.Message, diagnostics = _server.Diagnostics,
            canEdit = state.CanEditConfiguration, canStop = state.CanStop,
            canRestart = state.Configuration != null && state.Status is ServerLifecycle.Running or ServerLifecycle.Error,
            ipAddress = state.Configuration?.IpAddress ?? _settings.IpAddress, port = state.Configuration?.Port ?? _settings.Port,
            accessUrl = state.Status == ServerLifecycle.Running ? state.Configuration?.Address.AbsoluteUri : null, addresses });
    }
    private void Send(string type, object payload) { if (_ready) WebView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { type, payload }, WebBridge.Json)); }
    private void SendError(string message) => Send("error", new { message });
    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_canClose) return;
        e.Cancel = true; if (_closing) return; _closing = true;
        try
        {
            await _server.StopAsync();
            if (_server.Snapshot.OwnsProcess) throw new InvalidOperationException(_server.Snapshot.Message);
            if (_page != null) await _store.SaveAsync(_deck);
            _server.StateChanged -= ServerChanged; await _server.DisposeAsync();
            _canClose = true; _ready = false; WebView.Dispose(); Close();
        }
        catch (Exception ex) { _closing = false; MessageBox.Show(this, ex.Message, "RadishDeck"); }
    }
}
