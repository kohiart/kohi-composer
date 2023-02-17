// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class MathUtils
{
    public const int RecursionLimit = 32;
    public const long AngleTolerance = 42949672L; /* 0.01 */
    public const long Epsilon = 4L; /* 0.000000001 */

    public static long CrossProduct(long x1, long y1,
        long x2, long y2,
        long x, long y)
    {
        var value = Fix64.Sub(
            Fix64.Mul(Fix64.Sub(x, x2), Fix64.Sub(y2, y1)),
            Fix64.Mul(Fix64.Sub(y, y2), Fix64.Sub(x2, x1))
        );

        return value;
    }

    public static (long x, long y, bool intersects) CalcIntersection(long aX1, long aY1, long aX2, long aY2,
        long bX1, long bY1, long bX2, long bY2)
    {
        var num = Fix64.Mul(Fix64.Sub(aY1, bY1), Fix64.Sub(bX2, bX1)) -
                  Fix64.Mul(Fix64.Sub(aX1, bX1), Fix64.Sub(bY2, bY1));
        var den = Fix64.Mul(Fix64.Sub(aX2, aX1), Fix64.Sub(bY2, bY1)) -
                  Fix64.Mul(Fix64.Sub(aY2, aY1), Fix64.Sub(bX2, bX1));

        if (Fix64.Abs(den) < Epsilon) return (0, 0, false);

        var r = Fix64.Div(num, den);

        return (
            Fix64.Add(aX1, Fix64.Mul(r, Fix64.Sub(aX2, aX1))),
            Fix64.Add(aY1, Fix64.Mul(r, Fix64.Sub(aY2, aY1))),
            true);
    }

    public static long CalcSquareDistance(long x1, long y1, long x2, long y2)
    {
        var dx = Fix64.Sub(x2, x1);
        var dy = Fix64.Sub(y2, y1);
        var value = Fix64.Add(Fix64.Mul(dx, dx), Fix64.Mul(dy, dy));
        return value;
    }

    public static long CalcDistance(long x1, long y1, long x2, long y2)
    {
        var dx = Fix64.Sub(x2, x1);
        var dy = Fix64.Sub(y2, y1);
        var distance = Trig256.Sqrt(Fix64.Add(Fix64.Mul(dx, dx), Fix64.Mul(dy, dy)));
        return distance;
    }
}