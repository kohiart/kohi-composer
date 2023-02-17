// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public sealed class ClippingData
{
    public int F1 { get; set; }
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public Vector2[]? ClipPoly { get; set; }
    public RectangleInt ClipBox { get; set; }
    public bool Clipping { get; set; }
}