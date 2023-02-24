// Copyright (c) Kohi Art Community, Inc.

using System.Drawing;

namespace Kohi.Composer;

public class Graphics2D : CastingShim
{
    public const int OrderB = 0;
    public const int OrderG = 1;
    public const int OrderR = 2;
    public const int OrderA = 3;

    public Graphics2D(int width, int height, int scale = 1)
    {
        Width = width * scale;
        Height = height * scale;

        Aa = AntiAliasMethods.Create(8);
        Ss = SubpixelScaleMethods.Create(8);
        ScanlineData = ScanlineDataMethods.Create(width, scale);
        ClippingData = ClippingDataMethods.Create(width, height, Ss, scale);
        CellData = CellDataMethods.Create(scale);
        Buffer = new byte[Width * 4 * Height];
    }

    public AntiAlias Aa { get; set; }
    public SubpixelScale Ss { get; set; }
    public ScanlineData ScanlineData { get; set; }
    public ClippingData ClippingData { get; set; }
    public CellData CellData { get; set; }

    public byte[] Buffer { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public static void Clear(Graphics2D g, uint color)
    {
        var clippingRect = new RectangleInt(
            g.ClippingData.ClipBox.Left / g.Ss.Scale,
            g.ClippingData.ClipBox.Bottom / g.Ss.Scale,
            g.ClippingData.ClipBox.Right / g.Ss.Scale,
            g.ClippingData.ClipBox.Top / g.Ss.Scale);

        for (var y = clippingRect.Bottom; y < clippingRect.Top; y++)
        {
            var bufferOffset = GetBufferOffsetXy(g, clippingRect.Left, y);
            var clippingWidth = clippingRect.Right - clippingRect.Left;

            for (var x = 0; x < clippingWidth; x++)
            {
                g.Buffer[bufferOffset + OrderB] = (byte) (color >> 0);
                g.Buffer[bufferOffset + OrderG] = (byte) (color >> 8);
                g.Buffer[bufferOffset + OrderR] = (byte) (color >> 16);
                g.Buffer[bufferOffset + OrderA] = (byte) (color >> 24);

                bufferOffset += 4;
            }
        }
    }

    public static void RenderWithTransform(Graphics2D g, IList<VertexData> vertices, uint color, Matrix transform,
        bool blend = true)
    {
        if (!transform.IsIdentity()) vertices = ApplyTransformMethods.ApplyTransform(vertices, transform);

        Render(g, vertices, color, blend);
    }

    public static void Render(Graphics2D g, IList<VertexData> vertices, uint color, bool blend = true)
    {
        AddPath(g, vertices);

        ScanlineRasterizer.RenderSolid(g, color, blend);
    }

    public static void RenderWithYShift(Graphics2D g, IList<VertexData> vertices, Color color, Matrix transform,
        int yShift, bool blend = true)
    {
        if (!transform.IsIdentity())
            vertices = ApplyTransformMethods.ApplyTransform(vertices, transform, yShift);

        AddPath(g, vertices);

        ScanlineRasterizer.RenderSolid(g, color.ToUInt32(), blend);
    }

    internal static int GetBufferOffsetY(Graphics2D g, int y)
    {
        return y * g.Width * 4;
    }

    internal static int GetBufferOffsetXy(Graphics2D g, int x, int y)
    {
        if (x < 0 || x >= g.Width || y < 0 || y >= g.Height)
            return -1;
        return y * g.Width * 4 + x * 4;
    }

    public static void CopyPixels(byte[] buffer, int bufferOffset, uint sourceColor, int count,
        PixelClipping? clipping = null)
    {
        var i = 0;
        do
        {
            if (clipping.HasValue && !clipping.Value.IsPointInPolygon(clipping.Value.X + i, clipping.Value.Y))
            {
                i++;
                bufferOffset += 4;
                continue;
            }

            buffer[bufferOffset + OrderR] = uint8(sourceColor >> 16);
            buffer[bufferOffset + OrderG] = uint8(sourceColor >> 8);
            buffer[bufferOffset + OrderB] = uint8(sourceColor >> 0);
            buffer[bufferOffset + OrderA] = uint8(sourceColor >> 24);
            bufferOffset += 4;
            i++;
        } while (--count != 0);
    }

    public static void BlendPixel(byte[] buffer, int bufferOffset, uint sourceColor, PixelClipping? clipping = null)
    {
        if (bufferOffset == -1)
            return;

        if (clipping.HasValue && !clipping.Value.IsPointInPolygon(clipping.Value.X, clipping.Value.Y))
            return;

        {
            var sr = uint8(sourceColor >> 16);
            var sg = uint8(sourceColor >> 8);
            var sb = uint8(sourceColor >> 0);
            var sa = uint8(sourceColor >> 24);

            unchecked
            {
                if (sourceColor >> 24 == 255)
                {
                    buffer[bufferOffset + OrderR] = sr;
                    buffer[bufferOffset + OrderG] = sg;
                    buffer[bufferOffset] = sb;
                    buffer[bufferOffset + OrderA] = sa;
                }
                else
                {
                    var r = buffer[bufferOffset + OrderR];
                    var g = buffer[bufferOffset + OrderG];
                    var b = buffer[bufferOffset];
                    var a = buffer[bufferOffset + OrderA];

                    buffer[bufferOffset + OrderR] = uint8(((sr - r) * sa + (r << 8)) >> 8);
                    buffer[bufferOffset + OrderG] = uint8(((sg - g) * sa + (g << 8)) >> 8);
                    buffer[bufferOffset] = uint8(((sb - b) * sa + (b << 8)) >> 8);
                    buffer[bufferOffset + OrderA] = uint8(sa + a - ((sa * a + 255) >> 8));
                }
            }
        }
    }

    private static void AddPath(Graphics2D g, IList<VertexData> vertices)
    {
        if (g.CellData.Sorted)
        {
            CellRasterizer.ResetCells(g.CellData);
            g.ScanlineData.Status = ScanlineStatus.Initial;
        }

        foreach (var vertex in vertices)
        {
            if (vertex.Command == Command.Stop)
                break;

            var command = vertex.Command;

            if (command == Command.MoveTo)
            {
                ClosePolygon(g);
                g.ScanlineData.StartX = ClippingDataMethods.Upscale(vertex.Position.X, g.Ss);
                g.ScanlineData.StartY = ClippingDataMethods.Upscale(vertex.Position.Y, g.Ss);
                Clipping.MoveToClip(g.ScanlineData.StartX, g.ScanlineData.StartY, g.ClippingData);
                g.ScanlineData.Status = ScanlineStatus.MoveTo;
            }
            else
            {
                if (command != Command.Stop && command != Command.EndPoly)
                {
                    Clipping.LineToClip(g, ClippingDataMethods.Upscale(vertex.Position.X, g.Ss),
                        ClippingDataMethods.Upscale(vertex.Position.Y, g.Ss));
                    g.ScanlineData.Status = ScanlineStatus.LineTo;
                }
                else
                {
                    if (command == Command.EndPoly) ClosePolygon(g);
                }
            }
        }
    }

    internal static void ClosePolygon(Graphics2D g)
    {
        if (g.ScanlineData.Status != ScanlineStatus.LineTo) return;
        Clipping.LineToClip(g, g.ScanlineData.StartX, g.ScanlineData.StartY);
        g.ScanlineData.Status = ScanlineStatus.Closed;
    }
}