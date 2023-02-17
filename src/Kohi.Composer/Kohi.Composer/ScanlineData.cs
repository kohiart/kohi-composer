// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public sealed class ScanlineData
{
    public int ScanY { get; set; }
    public int StartX { get; set; }
    public int StartY { get; set; }
    public ScanlineStatus Status { get; set; }
    public int CoverIndex { get; set; }
    public byte[] Covers { get; set; } = null!;
    public int SpanIndex { get; set; }
    public ScanlineSpan[] Spans { get; set; } = null!;
    public int Current { get; set; }
    public int LastX { get; set; }
    public int Y { get; set; }
}