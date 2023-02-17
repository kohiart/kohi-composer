// Copyright (c) Kohi Art Community, Inc.

// ReSharper disable InconsistentNaming

namespace Kohi.Composer;

public sealed class ScanlineRasterizer : CastingShim
{
    public static int MaxMaxLength = int.MinValue;

    internal static void RenderSolid(Graphics2D g, uint color, bool blend)
    {
        Graphics2D.ClosePolygon(g);
        CellRasterizer.SortCells(g.cellData);
        if (g.cellData.Used == 0) return;
        g.scanlineData.ScanY = g.cellData.MinY;

        Reset(g.cellData.MinX, g.cellData.MaxX, g.scanlineData);
        while (SweepScanline(g))
        {
            var y = g.scanlineData.Y;
            var spanCount = g.scanlineData.SpanIndex;

            g.scanlineData.Current = 1;
            var scanlineSpan = getNextScanlineSpan(g.scanlineData);

            var covers = g.scanlineData.Covers;
            for (;;)
            {
                var x = scanlineSpan.X;
                if (scanlineSpan.Length > 0)
                {
                    BlendSolidHorizontalSpan(g,
                        new BlendSolidHorizontalSpanArgs(x, y, scanlineSpan.Length, color, covers, scanlineSpan.CoverIndex,
                            blend));
                }
                else
                {
                    var x2 = x - scanlineSpan.Length - 1;
                    blendHorizontalLine(g,
                        new BlendHorizontalLine(x, y, x2, color, covers[scanlineSpan.CoverIndex], blend));
                }

                if (--spanCount == 0) break;
                scanlineSpan = getNextScanlineSpan(g.scanlineData);
            }
        }
    }

    private static void BlendSolidHorizontalSpan(Graphics2D g, BlendSolidHorizontalSpanArgs f)
    {
        var colorAlpha = (int) (f.sourceColor >> 24);

        if (colorAlpha != 0)
            unchecked
            {
                var bufferOffset = Graphics2D.GetBufferOffsetXy(g, f.x, f.y);
                if (bufferOffset == -1)
                    return;

                var i = 0;
                do
                {
                    var alpha = !f.blend ? colorAlpha : (colorAlpha * (f.covers[f.coversIndex] + 1)) >> 8;

                    if (alpha == 255)
                    {
                        Graphics2D.CopyPixels(g.buffer, bufferOffset, f.sourceColor, 1,
                            g.clippingData.ClipPoly == null
                                ? default(PixelClipping?)
                                : new PixelClipping {Area = g.clippingData.ClipPoly, X = f.x + i, Y = f.y});
                    }
                    else
                    {
                        var targetColor = ColorMath.ToColor((byte) alpha, (byte) (f.sourceColor >> 16),
                            (byte) (f.sourceColor >> 8),
                            (byte) (f.sourceColor >> 0));

                        Graphics2D.BlendPixel(g.buffer, bufferOffset,
                            targetColor,
                            g.clippingData.ClipPoly == null
                                ? default(PixelClipping?)
                                : new PixelClipping
                                {
                                    Area = g.clippingData.ClipPoly,
                                    X = f.x + i,
                                    Y = f.y
                                });
                    }

                    bufferOffset += 4;
                    f.coversIndex++;
                    i++;
                } while (--f.len != 0);
            }
    }

    private static void blendHorizontalLine(Graphics2D g, BlendHorizontalLine f)
    {
        var colorAlpha = (int) (f.sourceColor >> 24);
        if (colorAlpha != 0)
        {
            var len = f.x2 - f.x1 + 1;
            var bufferOffset = Graphics2D.GetBufferOffsetXy(g, f.x1, f.y);
            var alpha = !f.blend ? colorAlpha : (colorAlpha * (f.cover + 1)) >> 8;

            if (alpha == 255)
            {
                Graphics2D.CopyPixels(g.buffer, bufferOffset, f.sourceColor, len,
                    g.clippingData.ClipPoly == null
                        ? default(PixelClipping?)
                        : new PixelClipping {Area = g.clippingData.ClipPoly, X = f.x1, Y = f.y});
            }
            else
            {
                var i = 0;
                var targetColor = ColorMath.ToColor((byte) alpha, (byte) (f.sourceColor >> 16),
                    (byte) (f.sourceColor >> 8),
                    (byte) (f.sourceColor >> 0));

                do
                {
                    Graphics2D.BlendPixel(g.buffer, bufferOffset,
                        targetColor,
                        g.clippingData.ClipPoly == null
                            ? default(PixelClipping?)
                            : new PixelClipping {Area = g.clippingData.ClipPoly, X = f.x1 + i, Y = f.y});

                    bufferOffset += 4;
                    i++;
                } while (--len != 0);
            }
        }
    }

    private static bool SweepScanline(Graphics2D g)
    {
        for (;;)
        {
            if (g.scanlineData.ScanY > g.cellData.MaxY) return false;

            ResetSpans(g.scanlineData);
            var cellCount = g.cellData.SortedY[g.scanlineData.ScanY - g.cellData.MinY].Count;

            var cells = g.cellData.SortedCells;
            var offset = g.cellData.SortedY[g.scanlineData.ScanY - g.cellData.MinY].Start;

            var cover = 0;

            while (cellCount != 0)
            {
                var current = cells[offset];
                var x = current.X;
                var area = current.Area;
                int alpha;

                cover += current.Cover;

                while (--cellCount != 0)
                {
                    offset++;
                    current = cells[offset];
                    if (current.X != x) break;

                    area += current.Area;
                    cover += current.Cover;
                }

                if (area != 0)
                {
                    alpha = CalculateAlpha(g, (cover << (g.ss.Value + 1)) - area);
                    if (alpha != 0) AddCell(g.scanlineData, x, alpha);
                    x++;
                }

                if (cellCount != 0 && current.X > x)
                {
                    alpha = CalculateAlpha(g, cover << (g.ss.Value + 1));
                    if (alpha != 0) AddSpan(g.scanlineData, x, current.X - x, alpha);
                }
            }

            if (g.scanlineData.SpanIndex != 0) break;
            ++g.scanlineData.ScanY;
        }

        g.scanlineData.Y = g.scanlineData.ScanY;
        ++g.scanlineData.ScanY;
        return true;
    }

    private static int CalculateAlpha(Graphics2D g, int area)
    {
        var cover = area >> (g.ss.Value * 2 + 1 - g.aa.Value);
        if (cover < 0) cover = -cover;
        if (cover > g.aa.Mask) cover = g.aa.Mask;
        return cover;
    }

    private static void AddSpan(ScanlineData scanlineData, int x, int len, int cover)
    {
        if (x == scanlineData.LastX + 1
            && scanlineData.Spans[scanlineData.SpanIndex].Length < 0
            && cover == scanlineData.Spans[scanlineData.SpanIndex].CoverIndex)
        {
            scanlineData.Spans[scanlineData.SpanIndex].Length -= (short) len;
        }
        else
        {
            scanlineData.Covers[scanlineData.CoverIndex] = (byte) cover;
            scanlineData.SpanIndex++;
            scanlineData.Spans[scanlineData.SpanIndex].CoverIndex = scanlineData.CoverIndex++;
            scanlineData.Spans[scanlineData.SpanIndex].X = (short) x;
            scanlineData.Spans[scanlineData.SpanIndex].Length = (short) -len;
        }

        scanlineData.LastX = x + len - 1;
    }

    private static void AddCell(ScanlineData scanlineData, int x, int cover)
    {
        scanlineData.Covers[scanlineData.CoverIndex] = (byte) cover;
        if (x == scanlineData.LastX + 1 && scanlineData.Spans[scanlineData.SpanIndex].Length > 0)
        {
            scanlineData.Spans[scanlineData.SpanIndex].Length++;
        }
        else
        {
            scanlineData.SpanIndex++;
            scanlineData.Spans[scanlineData.SpanIndex].CoverIndex = scanlineData.CoverIndex;
            scanlineData.Spans[scanlineData.SpanIndex].X = (short) x;
            scanlineData.Spans[scanlineData.SpanIndex].Length = 1;
        }

        scanlineData.LastX = x;
        scanlineData.CoverIndex++;
    }

    private static void ResetSpans(ScanlineData scanlineData)
    {
        scanlineData.LastX = 0x7FFFFFF0;
        scanlineData.CoverIndex = 0;
        scanlineData.SpanIndex = 0;
        scanlineData.Spans[scanlineData.SpanIndex].Length = 0;
    }

    private static void Reset(int minX, int maxX, ScanlineData scanlineData)
    {
        var maxLength = maxX - minX + 3;
        scanlineData.LastX = 0x7FFFFFF0;
        scanlineData.CoverIndex = 0;
        scanlineData.SpanIndex = 0;
        scanlineData.Spans[scanlineData.SpanIndex].Length = 0;
    }

    private static ScanlineSpan getNextScanlineSpan(ScanlineData scanlineData)
    {
        scanlineData.Current++;
        return scanlineData.Spans[scanlineData.Current - 1];
    }

    private struct BlendSolidHorizontalSpanArgs
    {
        public readonly int x;
        public readonly int y;
        public readonly uint sourceColor;
        public readonly byte[] covers;
        public readonly bool blend;

        public int coversIndex;
        public int len;

        public BlendSolidHorizontalSpanArgs(int x, int y, int len, uint sourceColor, byte[] covers, int coversIndex,
            bool blend)
        {
            this.x = x;
            this.y = y;
            this.len = len;
            this.sourceColor = sourceColor;
            this.covers = covers;
            this.coversIndex = coversIndex;
            this.blend = blend;
        }
    }

    private readonly struct BlendHorizontalLine
    {
        public readonly int x1;
        public readonly int y;
        public readonly int x2;
        public readonly uint sourceColor;
        public readonly byte cover;
        public readonly bool blend;

        public BlendHorizontalLine(int x1, int y, int x2, uint sourceColor, byte cover, bool blend)
        {
            this.x1 = x1;
            this.y = y;
            this.x2 = x2;
            this.sourceColor = sourceColor;
            this.cover = cover;
            this.blend = blend;
        }
    }
}