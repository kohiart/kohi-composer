// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public readonly struct Rectangle
{
    public readonly long Left;
    public readonly long Bottom;
    public readonly long Right;
    public readonly long Top;

    public Rectangle(long left, long bottom, long right, long top)
    {
        Left = left;
        Bottom = bottom;
        Right = right;
        Top = top;
    }

    public static bool operator ==(Rectangle a, Rectangle b)
    {
        if (a.Left == b.Left && a.Bottom == b.Bottom && a.Right == b.Right && a.Top == b.Top) return true;

        return false;
    }

    public static bool operator !=(Rectangle a, Rectangle b)
    {
        if (a.Left != b.Left || a.Bottom != b.Bottom || a.Right != b.Right || a.Top != b.Top) return true;

        return false;
    }

    public override int GetHashCode()
    {
        return new {x1 = Left, x2 = Right, y1 = Bottom, y2 = Top}.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj?.GetType() == typeof(Rectangle)) return this == (Rectangle) obj;

        return false;
    }
}