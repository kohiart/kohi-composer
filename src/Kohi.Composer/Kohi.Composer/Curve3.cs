// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public sealed class Curve3
{
    private readonly long _distanceToleranceSquare;
    private readonly List<Vector2> _points;

    public Curve3(long x1, long y1, long cx, long cy, long x2, long y2)
    {
        _points = new List<Vector2>();
        _distanceToleranceSquare = 2147483648L; /* 0.5 */
        _distanceToleranceSquare = Fix64.Mul(_distanceToleranceSquare, _distanceToleranceSquare);
        Bezier(x1, y1, cx, cy, x2, y2);
    }

    public IEnumerable<VertexData> Vertices()
    {
        var results = new VertexData[_points.Count + 1];

        for (var i = 0; i < _points.Count; i++)
            if (i == 0)
                results[i] = new VertexData(Command.MoveTo, _points[i]);
            else
                results[i] = new VertexData(Command.LineTo, _points[i]);

        results[_points.Count] = new VertexData(Command.Stop, new Vector2());
        return results;
    }

    private void Bezier(long x1, long y1, long x2, long y2, long x3, long y3)
    {
        _points.Add(new Vector2(x1, y1));
        RecursiveBezier(x1, y1, x2, y2, x3, y3, 0);
        _points.Add(new Vector2(x3, y3));
    }

    private void RecursiveBezier(long x1, long y1, long x2, long y2, long x3, long y3, int level)
    {
        if (level > MathUtils.RecursionLimit) return;

        var x12 = Fix64.Div(Fix64.Add(x1, x2), Fix64.Two);
        var y12 = Fix64.Div(Fix64.Add(y1, y2), Fix64.Two);
        var x23 = Fix64.Div(Fix64.Add(x2, x3), Fix64.Two);
        var y23 = Fix64.Div(Fix64.Add(y2, y3), Fix64.Two);
        var x123 = Fix64.Div(Fix64.Add(x12, x23), Fix64.Two);
        var y123 = Fix64.Div(Fix64.Add(y12, y23), Fix64.Two);

        var dx = Fix64.Sub(x3, x1);
        var dy = Fix64.Sub(y3, y1);

        var d = Fix64.Abs(
            Fix64.Sub(
                Fix64.Mul(Fix64.Sub(x2, x3), dy),
                Fix64.Mul(Fix64.Sub(y2, y3), dx)));

        long da;

        if (d > MathUtils.Epsilon)
        {
            if (Fix64.Mul(d, d) <= Fix64.Mul(_distanceToleranceSquare, Fix64.Mul(dx, dx) + Fix64.Mul(dy, dy)))
            {
                if (0 < MathUtils.AngleTolerance)
                {
                    _points.Add(new Vector2(x123, y123));
                    return;
                }

                da = Fix64.Abs(
                    Fix64.Sub(
                        Trig256.Atan2(Fix64.Sub(y3, y2), Fix64.Sub(x3, x2)),
                        Trig256.Atan2(Fix64.Sub(y2, y1), Fix64.Sub(x2, x1)))
                );

                if (da >= Fix64.Pi) da = Fix64.Sub(Fix64.TwoPi, da);

                if (da < 0)
                {
                    _points.Add(new Vector2(x123, y123));
                    return;
                }
            }
        }
        else
        {
            da = Fix64.Add(Fix64.Mul(dx, dx), Fix64.Mul(dy, dy));

            if (da == 0)
            {
                d = MathUtils.CalcSquareDistance(x1, y1, x2, y2);
            }
            else
            {
                d = Fix64.Add(
                    Fix64.Mul(Fix64.Sub(x2, x1), dx),
                    Fix64.Div(Fix64.Mul(Fix64.Sub(y2, y1), dy), da)
                );

                if (d > 0 && d < Fix64.One) return;

                if (d <= 0)
                    d = MathUtils.CalcSquareDistance(x2, y2, x1, y1);
                else if (d > Fix64.One)
                    d = MathUtils.CalcSquareDistance(x2, y2, x3, y3);
                else
                    d = MathUtils.CalcSquareDistance(x2, y2,
                        Fix64.Add(x1, Fix64.Mul(d, dx)),
                        Fix64.Add(y1, Fix64.Mul(d, dy))
                    );
            }

            if (d < _distanceToleranceSquare)
            {
                _points.Add(new Vector2(x2, y2));
                return;
            }
        }

        RecursiveBezier(x1, y1, x12, y12, x123, y123, level + 1);
        RecursiveBezier(x123, y123, x23, y23, x3, y3, level + 1);
    }
}