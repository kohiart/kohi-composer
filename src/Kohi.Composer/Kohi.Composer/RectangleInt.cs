// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public struct RectangleInt
{
    public int Left { get; set; }
    public int Bottom { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }

    public RectangleInt(int left, int bottom, int right, int top)
    {
        Left = left;
        Bottom = bottom;
        Right = right;
        Top = top;
    }

    public RectangleInt Normalize()
    {
        int t;
        if (Left > Right)
        {
            t = Left;
            Left = Right;
            Right = t;
        }

        if (Bottom > Top)
        {
            t = Bottom;
            Bottom = Top;
            Top = t;
        }

        return this;
    }

    public override int GetHashCode()
    {
        return new {x1 = Left, x2 = Right, y1 = Bottom, y2 = Top}.GetHashCode();
    }
}