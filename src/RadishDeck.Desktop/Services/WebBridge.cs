using System.Text.Json;
using RadishDeck.Core.Models.V2;
namespace RadishDeck.Desktop.Services;

public sealed record BridgeMessage(string Type, JsonElement Payload);
public sealed record ElementMove(Guid Id, double X, double Y);
public sealed record ServerStartRequest(string IpAddress, string Port);
public static class WebBridge
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public static (double X, double Y) Clamp(double x, double y, ElementLayout layout, CanvasProfile canvas)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y)) throw new ArgumentException("Coordinates must be finite");
        return (Math.Clamp(x, 0, Math.Max(0, canvas.Width - layout.Width)), Math.Clamp(y, 0, Math.Max(0, canvas.Height - layout.Height)));
    }
}
