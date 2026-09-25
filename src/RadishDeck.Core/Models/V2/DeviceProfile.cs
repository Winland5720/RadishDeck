namespace RadishDeck.Core.Models.V2;

/// <summary>Describes the target device profile used for rendering.</summary>
public sealed class DeviceProfile
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DeviceProfileType Type { get; set; } = DeviceProfileType.Custom;
}

/// <summary>Supported target device categories.</summary>
public enum DeviceProfileType { Desktop, Tablet, Mobile, Custom }
