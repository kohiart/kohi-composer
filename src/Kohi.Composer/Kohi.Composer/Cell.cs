// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public struct Cell
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Cover { get; set; }
    public int Area { get; set; }
    public int Left { get; set; }
    public int Right { get; set; }

    public Cell()
    {
        X = 0x7FFFFFFF;
        Y = 0x7FFFFFFF;
        Cover = 0;
        Area = 0;
        Left = -1;
        Right = -1;
    }

    public void Set(Cell other)
    {
        X = other.X;
        Y = other.Y;
        Cover = other.Cover;
        Area = other.Area;
        Left = other.Left;
        Right = other.Right;
    }

    public void Style(Cell other)
    {
        Left = other.Left;
        Right = other.Right;
    }

    public bool NotEqual(int ex, int ey, Cell other)
    {
        unchecked
        {
            return ((ex - X) | (ey - Y) | (Left - other.Left) | (Right - other.Right)) != 0;
        }
    }
}