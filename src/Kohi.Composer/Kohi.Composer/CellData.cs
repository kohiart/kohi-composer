// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public sealed class CellData
{
    public CellBlock Cb { get; set; }
    public Cell[] Cells { get; set; } = null!;
    public Cell Current { get; set; }
    public int Used { get; set; }
    public SortedY[] SortedY { get; set; } = null!;
    public Cell[] SortedCells { get; set; } = null!;
    public bool Sorted { get; set; }
    public Cell Style { get; set; }
    public int MaxX { get; set; }
    public int MaxY { get; set; }
    public int MinX { get; set; }
    public int MinY { get; set; }
}