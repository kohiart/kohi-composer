// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public class Clipping
{
    internal static void MoveToClip(int x1, int y1, ClippingData clippingData)
    {
        clippingData.X1 = x1;
        clippingData.Y1 = y1;
        if (clippingData.Clipping) clippingData.F1 = ClippingFlags(x1, y1, clippingData.ClipBox);
    }

    internal static void LineToClip(Graphics2D g, int x2, int y2)
    {
        if (g.clippingData.Clipping)
        {
            var f2 = ClippingFlags(x2, y2, g.clippingData.ClipBox);

            if ((g.clippingData.F1 & 10) == (f2 & 10) && (g.clippingData.F1 & 10) != 0)
            {
                g.clippingData.X1 = x2;
                g.clippingData.Y1 = y2;
                g.clippingData.F1 = f2;
                return;
            }

            var x1 = g.clippingData.X1;
            var y1 = g.clippingData.Y1;
            var f1 = g.clippingData.F1;
            int y3;
            int y4;
            int f3;
            int f4;

            if ((((f1 & 5) << 1) | (f2 & 5)) == 0)
            {
                LineClipY(g, x1, y1, x2, y2, f1, f2);
            }
            else if ((((f1 & 5) << 1) | (f2 & 5)) == 1)
            {
                y3 = y1 +
                     MulDiv(
                         (g.clippingData.ClipBox.Right - x1) * Fix64.One,
                         (y2 - y1) * Fix64.One,
                         (x2 - x1) * Fix64.One
                     );
                f3 = ClippingFlagsY(y3, g.clippingData.ClipBox);
                LineClipY(g, x1, y1, g.clippingData.ClipBox.Right, y3, f1, f3);
                LineClipY(g, g.clippingData.ClipBox.Right, y3, g.clippingData.ClipBox.Right, y2, f3, f2);
            }
            else if ((((f1 & 5) << 1) | (f2 & 5)) == 2)
            {
                y3 = y1 +
                     MulDiv(
                         (g.clippingData.ClipBox.Right - x1) * Fix64.One,
                         (y2 - y1) * Fix64.One,
                         (x2 - x1) * Fix64.One
                     );
                f3 = ClippingFlagsY(y3, g.clippingData.ClipBox);
                LineClipY(g, g.clippingData.ClipBox.Right, y1, g.clippingData.ClipBox.Right, y3, f1, f3);
                LineClipY(g, g.clippingData.ClipBox.Right, y3, x2, y2, f3, f2);
            }
            else if ((((f1 & 5) << 1) | (f2 & 5)) == 3)
            {
                LineClipY(g, g.clippingData.ClipBox.Right, y1, g.clippingData.ClipBox.Right, y2, f1, f2);
            }
            else if ((((f1 & 5) << 1) | (f2 & 5)) == 4)
            {
                y3 = y1 +
                     MulDiv(
                         (g.clippingData.ClipBox.Left - x1) * Fix64.One,
                         (y2 - y1) * Fix64.One,
                         (x2 - x1) * Fix64.One
                     );
                f3 = ClippingFlagsY(y3, g.clippingData.ClipBox);
                LineClipY(g, x1, y1, g.clippingData.ClipBox.Left, y3, f1, f3);
                LineClipY(g, g.clippingData.ClipBox.Left, y3, g.clippingData.ClipBox.Left, y2, f3, f2);
            }
            else if ((((f1 & 5) << 1) | (f2 & 5)) == 6)
            {
                y3 = y1 +
                     MulDiv(
                         (g.clippingData.ClipBox.Right - x1) * Fix64.One,
                         (y2 - y1) * Fix64.One,
                         (x2 - x1) * Fix64.One
                     );

                y4 = y1 +
                     MulDiv(
                         (g.clippingData.ClipBox.Left - x1) * Fix64.One,
                         (y2 - y1) * Fix64.One,
                         (x2 - x1) * Fix64.One
                     );

                f3 = ClippingFlagsY(y3, g.clippingData.ClipBox);
                f4 = ClippingFlagsY(y4, g.clippingData.ClipBox);
                LineClipY(g, g.clippingData.ClipBox.Right, y1, g.clippingData.ClipBox.Right, y3, f1, f3);
                LineClipY(g, g.clippingData.ClipBox.Right, y3, g.clippingData.ClipBox.Left, y4, f3, f4);
                LineClipY(g, g.clippingData.ClipBox.Left, y4, g.clippingData.ClipBox.Left, y2, f4, f2);
            }
            else if ((((f1 & 5) << 1) | (f2 & 5)) == 8)
            {
                y3 = y1 +
                     MulDiv(
                         (g.clippingData.ClipBox.Left - x1) * Fix64.One,
                         (y2 - y1) * Fix64.One,
                         (x2 - x1) * Fix64.One
                     );

                f3 = ClippingFlagsY(y3, g.clippingData.ClipBox);
                LineClipY(g, g.clippingData.ClipBox.Left, y1, g.clippingData.ClipBox.Left, y3, f1, f3);
                LineClipY(g, g.clippingData.ClipBox.Left, y3, x2, y2, f3, f2);
            }
            else if ((((f1 & 5) << 1) | (f2 & 5)) == 9)
            {
                y3 = y1 +
                     MulDiv(
                         (g.clippingData.ClipBox.Left - x1) * Fix64.One,
                         (y2 - y1) * Fix64.One,
                         (x2 - x1) * Fix64.One
                     );

                y4 = y1 +
                     MulDiv(
                         (g.clippingData.ClipBox.Right - x1) * Fix64.One,
                         (y2 - y1) * Fix64.One,
                         (x2 - x1) * Fix64.One
                     );
                f3 = ClippingFlagsY(y3, g.clippingData.ClipBox);
                f4 = ClippingFlagsY(y4, g.clippingData.ClipBox);
                LineClipY(g, g.clippingData.ClipBox.Left, y1, g.clippingData.ClipBox.Left, y3, f1, f3);
                LineClipY(g, g.clippingData.ClipBox.Left, y3, g.clippingData.ClipBox.Right, y4, f3, f4);
                LineClipY(g, g.clippingData.ClipBox.Right, y4, g.clippingData.ClipBox.Right, y2, f4, f2);
            }
            else if ((((f1 & 5) << 1) | (f2 & 5)) == 12)
            {
                LineClipY(g, g.clippingData.ClipBox.Left, y1, g.clippingData.ClipBox.Left, y2, f1, f2);
            }

            g.clippingData.F1 = f2;
        }
        else
        {
            CellRasterizer.Line(new CellRasterizer.LineMethodArgs(g.clippingData.X1, g.clippingData.Y1, x2, y2),
                g.cellData,
                g.ss);
        }

        g.clippingData.X1 = x2;
        g.clippingData.Y1 = y2;
    }

    private static void LineClipY(Graphics2D g, int x1, int y1, int x2, int y2, int f1, int f2)
    {
        f1 &= 10;
        f2 &= 10;
        if ((f1 | f2) == 0)
        {
            CellRasterizer.Line(new CellRasterizer.LineMethodArgs(x1, y1, x2, y2), g.cellData, g.ss);
        }
        else
        {
            if (f1 == f2)
                // Invisible by Y
                return;

            var tx1 = x1;
            var ty1 = y1;
            var tx2 = x2;
            var ty2 = y2;

            if ((f1 & 8) != 0) // y1 < clip.y1
            {
                tx1 = x1 +
                      MulDiv(
                          (g.clippingData.ClipBox.Bottom - y1) * Fix64.One,
                          (x2 - x1) * Fix64.One,
                          (y2 - y1) * Fix64.One
                      );

                ty1 = g.clippingData.ClipBox.Bottom;
            }

            if ((f1 & 2) != 0) // y1 > clip.y2
            {
                tx1 = x1 +
                      MulDiv(
                          (g.clippingData.ClipBox.Top - y1) * Fix64.One,
                          (x2 - x1) * Fix64.One,
                          (y2 - y1) * Fix64.One
                      );

                ty1 = g.clippingData.ClipBox.Top;
            }

            if ((f2 & 8) != 0) // y2 < clip.y1
            {
                tx2 = x1 +
                      MulDiv(
                          (g.clippingData.ClipBox.Bottom - y1) * Fix64.One,
                          (x2 - x1) * Fix64.One,
                          (y2 - y1) * Fix64.One
                      );

                ty2 = g.clippingData.ClipBox.Bottom;
            }

            if ((f2 & 2) != 0) // y2 > clip.y2
            {
                tx2 = x1 +
                      MulDiv(
                          (g.clippingData.ClipBox.Top - y1) * Fix64.One,
                          (x2 - x1) * Fix64.One,
                          (y2 - y1) * Fix64.One
                      );

                ty2 = g.clippingData.ClipBox.Top;
            }

            CellRasterizer.Line(new CellRasterizer.LineMethodArgs(tx1, ty1, tx2, ty2), g.cellData, g.ss);
        }
    }

    private static int ClippingFlags(int x, int y, RectangleInt clipBox)
    {
        return (x > clipBox.Right ? 1 : 0)
               | (y > clipBox.Top ? 1 << 1 : 0)
               | (x < clipBox.Left ? 1 << 2 : 0)
               | (y < clipBox.Bottom ? 1 << 3 : 0);
    }

    private static int ClippingFlagsY(int y, RectangleInt clipBox)
    {
        return ((y > clipBox.Top ? 1 : 0) << 1) | ((y < clipBox.Bottom ? 1 : 0) << 3);
    }

    private static int MulDiv(long a, long b, long c)
    {
        var div = Fix64.Div(b, c);
        var mulDiv = Fix64.Mul(a, div);
        return (int) (Fix64.Round(mulDiv) / Fix64.One);
    }
}