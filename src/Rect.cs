using System.Management.Automation.Host;
using System.Security.AccessControl;

namespace InteractiveSelect;

internal readonly record struct Rect(int X, int Y, int Width, int Height)
{
    public int Bottom => Y + Height;
    public int Right => X + Width;
    public Coordinates TopLeft => new Coordinates(X, Y);
}
