using System.Net.Http;
using System.Windows;
using RadishDeck.Core;
using RadishDeck.Core.Models;
using RadishDeck.Desktop.Services;
using CoreAction = RadishDeck.Core.Models.Action;

namespace RadishDeck.Desktop;

public partial class MainWindow : Window
{
    private readonly ServerLauncherService _serverLauncherService = new();
    private readonly Element _serverElement = new() { Name = "Start Server", Type = "Button" };
    private readonly CoreAction _startServerAction = new() { Name = "Start Server", Type = "server.start" };
    private ServerStartActionExecutor? _serverStartActionExecutor;
    private ServerStatusService? _serverStatusService;

    public MainWindow()
    {
        InitializeComponent();
        ServerVersionText.Text = AppVersion.Current;
        _serverStartActionExecutor = new ServerStartActionExecutor(_serverLauncherService, ReadServerConfiguration);
        Closed += (_, _) =>
        {
            _serverStatusService?.Dispose();
            _serverLauncherService.Dispose();
        };
    }

    private async void StartServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(PortTextBox.Text, out var port) || port is < 1 or > 65535 || string.IsNullOrWhiteSpace(IpAddressTextBox.Text))
        {
            ServerStatusText.Text = "Unavailable";
            return;
        }

        StartServerButton.IsEnabled = false;
        try
        {
            var state = await _serverStartActionExecutor!.ExecuteAsync(_serverElement, _startServerAction);
            ServerStatusText.Text = state.Status;
            if (state.Status == "Running")
            {
                var (ipAddress, configuredPort) = ReadServerConfiguration();
                _serverStatusService?.Dispose();
                _serverStatusService = new ServerStatusService(new Uri($"http://{ipAddress}:{configuredPort}/"));
                var serverStatus = await _serverStatusService.GetStatusAsync();
                ServerVersionText.Text = serverStatus?.Version ?? AppVersion.Current;
            }
            else
            {
                ServerVersionText.Text = AppVersion.Current;
            }
        }
        catch (Exception)
        {
            ServerStatusText.Text = "Unavailable";
            ServerVersionText.Text = "Unavailable";
        }
        finally
        {
            StartServerButton.IsEnabled = true;
        }
    }

    private (string IpAddress, int Port) ReadServerConfiguration() =>
        (IpAddressTextBox.Text.Trim(), int.Parse(PortTextBox.Text));

    private void StopServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        _serverLauncherService.Stop();
        _serverStatusService?.Dispose();
        _serverStatusService = null;
        ServerStatusText.Text = "Stopped";
        ServerVersionText.Text = "—";
    }
}
