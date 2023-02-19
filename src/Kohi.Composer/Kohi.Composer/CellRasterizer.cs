// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class CellRasterizer
{
    public static int SortCalls;
    public static int MaxUsed = int.MinValue;
    public static int MaxSortedYSize = int.MinValue;

    internal static void ResetCells(CellData cellData)
    {
        cellData.Used = 0;
        cellData.Style = new Cell();
        cellData.Current = new Cell();
        cellData.Sorted = false;

        cellData.MinX = 0x7FFFFFFF;
        cellData.MinY = 0x7FFFFFFF;
        cellData.MaxX = -0x7FFFFFFF;
        cellData.MaxY = -0x7FFFFFFF;
    }

    internal static void Line(LineMethodArgs f, CellData cellData, SubpixelScale ss)
    {
        LineArgs a;

        a.Dx = f.X2 - f.X1;

        if (a.Dx >= ss.DxLimit || a.Dx <= -ss.DxLimit)
        {
            var cx = (f.X1 + f.X2) >> 1;
            var cy = (f.Y1 + f.Y2) >> 1;
            Line(new LineMethodArgs(f.X1, f.Y1, cx, cy), cellData, ss);
            Line(new LineMethodArgs(cx, cy, f.X2, f.Y2), cellData, ss);
        }

        a.Dy = f.Y2 - f.Y1;
        a.Ex1 = f.X1 >> ss.Value;
        a.Ex2 = f.X2 >> ss.Value;
        a.Ey1 = f.Y1 >> ss.Value;
        a.Ey2 = f.Y2 >> ss.Value;
        a.Fy1 = f.Y1 & ss.Mask;
        a.Fy2 = f.Y2 & ss.Mask;

        if (a.Ex1 < cellData.MinX) cellData.MinX = a.Ex1;
        if (a.Ex1 > cellData.MaxX) cellData.MaxX = a.Ex1;
        if (a.Ey1 < cellData.MinY) cellData.MinY = a.Ey1;
        if (a.Ey1 > cellData.MaxY) cellData.MaxY = a.Ey1;
        if (a.Ex2 < cellData.MinX) cellData.MinX = a.Ex2;
        if (a.Ex2 > cellData.MaxX) cellData.MaxX = a.Ex2;
        if (a.Ey2 < cellData.MinY) cellData.MinY = a.Ey2;
        if (a.Ey2 > cellData.MaxY) cellData.MaxY = a.Ey2;

        SetCurrentCell(a.Ex1, a.Ey1, cellData);

        if (a.Ey1 == a.Ey2)
        {
            RenderHorizontalLine(new RenderHorizontalLineMethodArgs(a.Ey1, f.X1, a.Fy1, f.X2, a.Fy2), cellData, ss);
            return;
        }

        a.Incr = 1;

        if (a.Dx == 0)
        {
            var ex = f.X1 >> ss.Value;
            var twoFx = (f.X1 - (ex << ss.Value)) << 1;

            a.First = ss.Scale;
            if (a.Dy < 0)
            {
                a.First = 0;
                a.Incr = -1;
            }

            a.Delta = a.First - a.Fy1;

            {
                var current = cellData.Current;
                current.Cover += a.Delta;
                current.Area += twoFx * a.Delta;
                cellData.Current = current;
            }

            a.Ey1 += a.Incr;
            SetCurrentCell(ex, a.Ey1, cellData);

            a.Delta = a.First + a.First - ss.Scale;
            var area = twoFx * a.Delta;
            while (a.Ey1 != a.Ey2)
            {
                {
                    var current = cellData.Current;
                    current.Cover = a.Delta;
                    current.Area = area;
                    cellData.Current = current;
                }

                a.Ey1 += a.Incr;
                SetCurrentCell(ex, a.Ey1, cellData);
            }

            a.Delta = a.Fy2 - ss.Scale + a.First;

            {
                var current = cellData.Current;
                current.Cover += a.Delta;
                current.Area += twoFx * a.Delta;
                cellData.Current = current;
            }

            return;
        }

        var p = (ss.Scale - a.Fy1) * a.Dx;
        a.First = ss.Scale;

        if (a.Dy < 0)
        {
            p = a.Fy1 * a.Dx;
            a.First = 0;
            a.Incr = -1;
            a.Dy = -a.Dy;
        }

        a.Delta = p / a.Dy;
        var mod = p % a.Dy;

        if (mod < 0)
        {
            a.Delta--;
            mod += a.Dy;
        }

        var xFrom = f.X1 + a.Delta;
        RenderHorizontalLine(new RenderHorizontalLineMethodArgs(a.Ey1, f.X1, a.Fy1, xFrom, a.First), cellData, ss);

        a.Ey1 += a.Incr;
        SetCurrentCell(xFrom >> ss.Value, a.Ey1, cellData);

        if (a.Ey1 != a.Ey2)
        {
            p = ss.Scale * a.Dx;
            var lift = p / a.Dy;
            var rem = p % a.Dy;

            if (rem < 0)
            {
                lift--;
                rem += a.Dy;
            }

            mod -= a.Dy;

            while (a.Ey1 != a.Ey2)
            {
                a.Delta = lift;
                mod += rem;
                if (mod >= 0)
                {
                    mod -= a.Dy;
                    a.Delta++;
                }

                var xTo = xFrom + a.Delta;
                RenderHorizontalLine(new RenderHorizontalLineMethodArgs(a.Ey1, xFrom, ss.Scale - a.First, xTo, a.First),
                    cellData,
                    ss);
                xFrom = xTo;

                a.Ey1 += a.Incr;
                SetCurrentCell(xFrom >> ss.Value, a.Ey1, cellData);
            }
        }

        RenderHorizontalLine(new RenderHorizontalLineMethodArgs(a.Ey1, xFrom, ss.Scale - a.First, f.X2, a.Fy2),
            cellData, ss);
    }

    internal static void SortCells(CellData cellData)
    {
        if (cellData.Sorted)
            return;

        SortCalls++;

        AddCurrentCell(cellData);

        {
            var current = cellData.Current;
            current.X = 0x7FFFFFFF;
            current.Y = 0x7FFFFFFF;
            current.Cover = 0;
            current.Area = 0;
            cellData.Current = current;
        }

        if (cellData.Used == 0) return;

        if (MaxUsed < cellData.Used) MaxUsed = cellData.Used;

        var sortedYSize = cellData.MaxY - cellData.MinY + 1;

        for (var i = 0; i < sortedYSize; i++)
        {
            cellData.SortedY[i].Start = 0;
            cellData.SortedY[i].Count = 0;
        }

        if (MaxSortedYSize < sortedYSize)
            MaxSortedYSize = sortedYSize;

        for (var i = 0; i < cellData.Used; i++)
        {
            var index = cellData.Cells[i].Y - cellData.MinY;
            cellData.SortedY[index].Start++;
        }

        var start = 0;
        for (var i = 0; i < sortedYSize; i++)
        {
            var v = cellData.SortedY[i].Start;
            cellData.SortedY[i].Start = start;
            start += v;
        }

        for (var i = 0; i < cellData.Used; i++)
        {
            var index = cellData.Cells[i].Y - cellData.MinY;
            var currentYStart = cellData.SortedY[index].Start;
            var currentYCount = cellData.SortedY[index].Count;
            cellData.SortedCells[currentYStart + currentYCount] = cellData.Cells[i];
            ++cellData.SortedY[index].Count;
        }

        for (var i = 0; i < sortedYSize; i++)
            if (cellData.SortedY[i].Count != 0)
                Sort(cellData.SortedCells, cellData.SortedY[i].Start,
                    cellData.SortedY[i].Start + cellData.SortedY[i].Count - 1);

        cellData.Sorted = true;
    }

    private static void RenderHorizontalLine(RenderHorizontalLineMethodArgs f, CellData cellData, SubpixelScale ss)
    {
        RenderHorizontalLineArgs a;

        a.Ex1 = f.X1 >> ss.Value;
        a.Ex2 = f.X2 >> ss.Value;
        a.Fx1 = f.X1 & ss.Mask;
        a.Fx2 = f.X2 & ss.Mask;
        a.Delta = 0;

        if (f.Y1 == f.Y2)
        {
            SetCurrentCell(a.Ex2, f.Ey, cellData);
            return;
        }

        if (a.Ex1 == a.Ex2)
        {
            a.Delta = f.Y2 - f.Y1;

            {
                var current = cellData.Current;
                current.Cover += a.Delta;
                current.Area += (a.Fx1 + a.Fx2) * a.Delta;
                cellData.Current = current;
            }

            return;
        }

        var p = (ss.Scale - a.Fx1) * (f.Y2 - f.Y1);
        var first = ss.Scale;
        var incr = 1;
        var dx = f.X2 - f.X1;

        if (dx < 0)
        {
            p = a.Fx1 * (f.Y2 - f.Y1);
            first = 0;
            incr = -1;
            dx = -dx;
        }

        a.Delta = p / dx;
        var mod = p % dx;

        if (mod < 0)
        {
            a.Delta--;
            mod += dx;
        }

        {
            var current = cellData.Current;
            current.Cover += a.Delta;
            current.Area += (a.Fx1 + first) * a.Delta;
            cellData.Current = current;
        }


        a.Ex1 += incr;
        SetCurrentCell(a.Ex1, f.Ey, cellData);
        f.Y1 += a.Delta;

        if (a.Ex1 != a.Ex2)
        {
            p = ss.Scale * (f.Y2 - f.Y1 + a.Delta);
            var lift = p / dx;
            var rem = p % dx;

            if (rem < 0)
            {
                lift--;
                rem += dx;
            }

            mod -= dx;

            while (a.Ex1 != a.Ex2)
            {
                a.Delta = lift;
                mod += rem;
                if (mod >= 0)
                {
                    mod -= dx;
                    a.Delta++;
                }

                {
                    var current = cellData.Current;
                    current.Cover += a.Delta;
                    current.Area += ss.Scale * a.Delta;
                    cellData.Current = current;
                }

                f.Y1 += a.Delta;
                a.Ex1 += incr;
                SetCurrentCell(a.Ex1, f.Ey, cellData);
            }
        }

        a.Delta = f.Y2 - f.Y1;

        {
            var current = cellData.Current;
            current.Cover += a.Delta;
            current.Area += (a.Fx2 + ss.Scale - first) * a.Delta;
            cellData.Current = current;
        }
    }

    private static void SetCurrentCell(int x, int y, CellData cellData)
    {
        if (cellData.Current.NotEqual(x, y, cellData.Style))
        {
            AddCurrentCell(cellData);
            cellData.Current.Style(cellData.Style);


            {
                var current = cellData.Current;
                current.X = x;
                current.Y = y;
                current.Cover = 0;
                current.Area = 0;
                cellData.Current = current;
            }
        }
    }

    private static void AddCurrentCell(CellData cellData)
    {
        if ((cellData.Current.Area | cellData.Current.Cover) != 0)
        {
            if (cellData.Used >= cellData.Cb.Limit) return;
            cellData.Cells[cellData.Used].Set(cellData.Current);
            cellData.Used++;
        }
    }

    private static void Sort(IList<Cell> cells, int start, int stop)
    {
        while (true)
        {
            if (stop == start) return;

            int pivot;
            {
                var m = start + 1;
                var n = stop;
                while (m < stop
                       && cells[start].X >= cells[m].X)
                    m++;

                while (n > start && cells[start].X <= cells[n].X) n--;
                while (m < n)
                {
                    (cells[m], cells[n]) = (cells[n], cells[m]);
                    while (m < stop && cells[start].X >= cells[m].X) m++;
                    while (n > start && cells[start].X <= cells[n].X) n--;
                }

                if (start != n) (cells[n], cells[start]) = (cells[start], cells[n]);

                pivot = n;
            }
            if (pivot > start) Sort(cells, start, pivot - 1);

            if (pivot < stop)
            {
                start = pivot + 1;
                continue;
            }

            break;
        }
    }

    internal readonly struct LineMethodArgs
    {
        public readonly int X1;
        public readonly int Y1;
        public readonly int X2;
        public readonly int Y2;

        public LineMethodArgs(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }
    }

    private struct LineArgs
    {
        public int Dx;
        public int Dy;
        public int Ex1;
        public int Ex2;
        public int Ey1;
        public int Ey2;
        public int Fy1;
        public int Fy2;
        public int Delta;
        public int First;
        public int Incr;
    }

    internal struct RenderHorizontalLineMethodArgs
    {
        public readonly int Ey;
        public readonly int X1;
        public int Y1;
        public readonly int X2;
        public readonly int Y2;

        public RenderHorizontalLineMethodArgs(int ey, int x1, int y1, int x2, int y2)
        {
            Ey = ey;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }
    }

    private struct RenderHorizontalLineArgs
    {
        public int Ex1;
        public int Ex2;
        public int Fx1;
        public int Fx2;
        public int Delta;
    }
}