using System.Net.Http;
using System.Windows;
using RadishDeck.Desktop.Services;

namespace RadishDeck.Desktop;

public partial class MainWindow : Window
{
    private readonly ServerStatusService _serverStatusService =
        new(new Uri("http://127.0.0.1:5187/"));

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += (_, _) => _serverStatusService.Dispose();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var serverStatus = await _serverStatusService.GetStatusAsync();
            ServerStatusText.Text = serverStatus?.Status switch
            {
                "running" => "Running",
                null => "Unavailable",
                var status => status
            };
            ServerVersionText.Text = serverStatus?.Version ?? "Unavailable";
        }
        catch (HttpRequestException)
        {
            ServerStatusText.Text = "Unavailable";
            ServerVersionText.Text = "Unavailable";
        }
        catch (TaskCanceledException)
        {
            ServerStatusText.Text = "Unavailable";
            ServerVersionText.Text = "Unavailable";
        }
    }
}
