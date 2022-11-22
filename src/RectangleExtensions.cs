using System.Management.Automation.Host;

namespace InteractiveSelect;

internal static class RectangleExtensions
{
    public static int GetWidth(this Rectangle rectangle)
        => rectangle.Right - rectangle.Left;

    public static int GetHeight(this Rectangle rectangle)
        => rectangle.Bottom - rectangle.Top;

    public static Coordinates GetTopLeft(this Rectangle rectangle)
        => new Coordinates(rectangle.Left, rectangle.Top);
}
