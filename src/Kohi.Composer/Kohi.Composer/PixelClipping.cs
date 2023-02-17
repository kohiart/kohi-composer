// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public struct PixelClipping
{
    public int X;
    public int Y;
    public Vector2[] Area;

    public bool IsPointInPolygon(int px, int py)
    {
        if (Area == null) throw new NullReferenceException();

        if (Area.Length < 3) return false;

        var oldPoint = Area[^1];

        var inside = false;

        foreach (var newPoint in Area)
        {
            Vector2 p2;
            Vector2 p1;

            if (newPoint.X > oldPoint.X)
            {
                p1 = oldPoint;
                p2 = newPoint;
            }
            else
            {
                p1 = newPoint;
                p2 = oldPoint;
            }

            var pxF = px * Fix64.One;
            var pyF = py * Fix64.One;

            var t1 = Fix64.Sub(pyF, p1.Y);
            var t2 = Fix64.Sub(p2.X, p1.X);
            var t3 = Fix64.Sub(p2.Y, p1.Y);
            var t4 = Fix64.Sub(pxF, p1.X);

            if (newPoint.X < pxF == pxF <= oldPoint.X
                && Fix64.Mul(t1, t2) < Fix64.Mul(t3, t4))
                inside = !inside;

            oldPoint = newPoint;
        }

        return inside;
    }
}