using System.Net.Http;
using System.Windows;
using RadishDeck.Desktop.Services;

namespace RadishDeck.Desktop;

public partial class MainWindow : Window
{
    private readonly ServerLauncherService _serverLauncherService = new();
    private ServerStatusService? _serverStatusService;

    public MainWindow()
    {
        InitializeComponent();
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
            var ipAddress = IpAddressTextBox.Text.Trim();
            var started = await _serverLauncherService.StartAsync(ipAddress, port);
            if (!started)
            {
                ServerStatusText.Text = "Unavailable";
                return;
            }

            _serverStatusService?.Dispose();
            _serverStatusService = new ServerStatusService(new Uri($"http://{ipAddress}:{port}/"));
            var serverStatus = await _serverStatusService.GetStatusAsync();
            ServerStatusText.Text = serverStatus?.Status == "running" ? "Running" : "Unavailable";
            ServerVersionText.Text = serverStatus?.Version ?? "Unavailable";
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

    private void StopServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        _serverLauncherService.Stop();
        _serverStatusService?.Dispose();
        _serverStatusService = null;
        ServerStatusText.Text = "Stopped";
        ServerVersionText.Text = "—";
    }
}
