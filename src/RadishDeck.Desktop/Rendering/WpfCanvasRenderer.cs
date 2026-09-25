using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RadishDeck.Core.Rendering;

namespace RadishDeck.Desktop.Rendering;

/// <summary>WPF adapter that renders the platform-independent RenderElement contract on a Canvas.</summary>
public sealed class WpfCanvasRenderer : IRenderer
{
    public void Render(IReadOnlyCollection<RenderElement> elements, IRenderContext context)
    {
        context.Clear();
        foreach (var element in elements) context.Draw(element);
    }
}

/// <summary>WPF-specific render context used by the Desktop Preview.</summary>
public sealed class WpfCanvasRenderContext : IRenderContext
{
    private readonly Canvas _canvas;
    private readonly Action<Guid> _selected;
    private readonly Action<Guid, double, double> _moved;
    private readonly Dictionary<UIElement, (Guid Id, Point Start, double X, double Y)> _drag = new();

    public WpfCanvasRenderContext(Canvas canvas, Action<Guid> selected, Action<Guid, double, double> moved)
        => (_canvas, _selected, _moved) = (canvas, selected, moved);

    public void Clear() => _canvas.Children.Clear();

    public void Draw(RenderElement element)
    {
        if (!string.Equals(element.Type, "Button", StringComparison.OrdinalIgnoreCase)) return;
        var button = new Button
        {
            Content = element.Content?.Text ?? element.Type,
            Width = element.Layout.Width,
            Height = element.Layout.Height,
            Background = Brush(element.Appearance?.Background) ?? Brushes.Crimson,
            BorderBrush = Brush(element.Appearance?.Border) ?? Brushes.White,
            BorderThickness = new Thickness(1),
            Tag = element.Id
        };
        Canvas.SetLeft(button, element.Layout.X);
        Canvas.SetTop(button, element.Layout.Y);
        button.PreviewMouseLeftButtonDown += (_, e) =>
        {
            _selected(element.Id);
            _drag[button] = (element.Id, e.GetPosition(_canvas), Canvas.GetLeft(button), Canvas.GetTop(button));
            button.CaptureMouse();
        };
        button.PreviewMouseMove += (_, e) =>
        {
            if (!_drag.TryGetValue(button, out var drag) || e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) return;
            var point = e.GetPosition(_canvas);
            var x = Math.Max(0, drag.X + point.X - drag.Start.X);
            var y = Math.Max(0, drag.Y + point.Y - drag.Start.Y);
            Canvas.SetLeft(button, x);
            Canvas.SetTop(button, y);
            _moved(drag.Id, x, y);
        };
        button.PreviewMouseLeftButtonUp += (_, _) =>
        {
            _drag.Remove(button);
            button.ReleaseMouseCapture();
        };
        _canvas.Children.Add(button);
    }

    private static Brush? Brush(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try { return (Brush)new BrushConverter().ConvertFromString(value)!; }
        catch (FormatException) { return null; }
    }
}
