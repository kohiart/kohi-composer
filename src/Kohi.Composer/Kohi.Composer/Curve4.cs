// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public sealed class Curve4
{
    private readonly long _angleTolerance;
    private readonly long _cuspLimit;
    private readonly long _distanceToleranceSquare;

    private readonly List<Vector2> _points;

    public Curve4(long x1, long y1, long x2, long y2, long x3, long y3, long x4, long y4)
    {
        _points = new List<Vector2>();
        _angleTolerance = 0;
        _cuspLimit = 0;

        _distanceToleranceSquare = 2147483648L; /* 0.5 */
        _distanceToleranceSquare = Fix64.Mul(_distanceToleranceSquare, _distanceToleranceSquare);

        _points.Clear();
        Bezier(x1, y1, x2, y2, x3, y3, x4, y4);
    }

    public IEnumerable<VertexData> Vertices()
    {
        var results = new VertexData[_points.Count + 2];
        results[0] = new VertexData(Command.MoveTo, _points[0]);
        for (var i = 1; i < _points.Count; i++) results[i] = new VertexData(Command.LineTo, _points[i]);
        results[_points.Count] = new VertexData(Command.Stop, 0, 0);
        return results;
    }

    private void Bezier(long x1, long y1, long x2, long y2, long x3, long y3, long x4, long y4)
    {
        _points.Add(new Vector2(x1, y1));
        RecursiveBezier(x1, y1, x2, y2, x3, y3, x4, y4, 0);
        _points.Add(new Vector2(x4, y4));
    }

    private void RecursiveBezier(long x1, long y1, long x2, long y2, long x3, long y3, long x4, long y4, int level)
    {
        if (level > MathUtils.RecursionLimit) return;

        var x12 = Fix64.Div(Fix64.Add(x1, x2), Fix64.Two);
        var y12 = Fix64.Div(Fix64.Add(y1, y2), Fix64.Two);
        var x23 = Fix64.Div(Fix64.Add(x2, x3), Fix64.Two);
        var y23 = Fix64.Div(Fix64.Add(y2, y3), Fix64.Two);
        var x34 = Fix64.Div(Fix64.Add(x3, x4), Fix64.Two);
        var y34 = Fix64.Div(Fix64.Add(y3, y4), Fix64.Two);
        var x123 = Fix64.Div(Fix64.Add(x12, x23), Fix64.Two);
        var y123 = Fix64.Div(Fix64.Add(y12, y23), Fix64.Two);
        var x234 = Fix64.Div(Fix64.Add(x23, x34), Fix64.Two);
        var y234 = Fix64.Div(Fix64.Add(y23, y34), Fix64.Two);
        var x1234 = Fix64.Div(Fix64.Add(x123, x234), Fix64.Two);
        var y1234 = Fix64.Div(Fix64.Add(y123, y234), Fix64.Two);

        var dx = Fix64.Sub(x4, x1);
        var dy = Fix64.Sub(y4, y1);

        var d2 = Fix64.Abs(Fix64.Sub(Fix64.Mul(Fix64.Sub(x2, x4), dy), Fix64.Mul(Fix64.Sub(y2, y4), dx)));
        var d3 = Fix64.Abs(Fix64.Sub(Fix64.Mul(Fix64.Sub(x3, x4), dy), Fix64.Mul(Fix64.Sub(y3, y4), dx)));
        long da1;
        long da2;
        long k;

        var switchCase = 0;
        if (d2 > MathUtils.Epsilon) switchCase = 2;
        if (d3 > MathUtils.Epsilon) switchCase++;

        switch (switchCase)
        {
            case 0:
                k = Fix64.Add(Fix64.Mul(dx, dx), Fix64.Mul(dy, dy));
                if (k == 0)
                {
                    d2 = MathUtils.CalcSquareDistance(x1, y1, x2, y2);
                    d3 = MathUtils.CalcSquareDistance(x4, y4, x3, y3);
                }
                else
                {
                    k = Fix64.Div(Fix64.One, k);
                    da1 = Fix64.Sub(x2, x1);
                    da2 = Fix64.Sub(y2, y1);
                    d2 = Fix64.Mul(k, Fix64.Add(Fix64.Mul(da1, dx), Fix64.Mul(da2, dy)));
                    da1 = Fix64.Sub(x3, x1);
                    da2 = Fix64.Sub(y3, y1);
                    d3 = Fix64.Mul(k, Fix64.Add(Fix64.Mul(da1, dx), Fix64.Mul(da2, dy)));

                    if (d2 > 0 && d2 < Fix64.One && d3 > 0 && d3 < Fix64.One) return;

                    if (d2 <= 0)
                        d2 = MathUtils.CalcSquareDistance(x2, y2, x1, y1);
                    else if (d2 >= Fix64.One)
                        d2 = MathUtils.CalcSquareDistance(x2, y2, x4, y4);
                    else
                        d2 = MathUtils.CalcSquareDistance(x2, y2, Fix64.Add(x1, Fix64.Mul(d2, dx)),
                            Fix64.Add(y1, Fix64.Mul(d2, dy)));

                    if (d3 <= 0)
                        d3 = MathUtils.CalcSquareDistance(x3, y3, x1, y1);
                    else if (d3 >= Fix64.One)
                        d3 = MathUtils.CalcSquareDistance(x3, y3, x4, y4);
                    else
                        d3 = MathUtils.CalcSquareDistance(x3, y3, Fix64.Add(x1, Fix64.Mul(d3, dx)),
                            Fix64.Add(y1, Fix64.Mul(d3, dy)));
                }

                if (d2 > d3)
                {
                    if (d2 < _distanceToleranceSquare)
                    {
                        _points.Add(new Vector2(x2, y2));
                        return;
                    }
                }
                else
                {
                    if (d3 < _distanceToleranceSquare)
                    {
                        _points.Add(new Vector2(x3, y3));
                        return;
                    }
                }

                break;

            case 1:
                if (Fix64.Mul(d3, d3) <= Fix64.Mul(_distanceToleranceSquare,
                        Fix64.Add(Fix64.Mul(dx, dx), Fix64.Mul(dy, dy))))
                {
                    if (_angleTolerance < MathUtils.AngleTolerance)
                    {
                        _points.Add(new Vector2(x23, y23));
                        return;
                    }

                    da1 = Fix64.Abs(
                        Fix64.Sub(
                            Trig256.Atan2(Fix64.Sub(y4, y3), Fix64.Sub(x4, x3)),
                            Trig256.Atan2(Fix64.Sub(y3, y2), Fix64.Sub(x3, x2))
                        ));

                    if (da1 >= Fix64.Pi) da1 = Fix64.Sub(Fix64.TwoPi, da1);

                    if (da1 < _angleTolerance)
                    {
                        _points.Add(new Vector2(x2, y2));
                        _points.Add(new Vector2(x3, y3));
                        return;
                    }

                    if (_cuspLimit != 0)
                        if (da1 > _cuspLimit)
                        {
                            _points.Add(new Vector2(x3, y3));
                            return;
                        }
                }

                break;

            case 2:
                if (Fix64.Mul(d2, d2) <= Fix64.Mul(_distanceToleranceSquare,
                        Fix64.Add(Fix64.Mul(dx, dx), Fix64.Mul(dy, dy))))
                {
                    if (_angleTolerance < MathUtils.AngleTolerance)
                    {
                        _points.Add(new Vector2(x23, y23));
                        return;
                    }

                    da1 = MathExtensions.Abs(Fix64.Sub(
                        Trig256.Atan2(Fix64.Sub(y3, y2), Fix64.Sub(x3, x2)),
                        Trig256.Atan2(Fix64.Sub(y2, y1), Fix64.Sub(x2, x1))));

                    if (da1 >= Fix64.Pi) da1 = Fix64.Sub(Fix64.TwoPi, da1);

                    if (da1 < _angleTolerance)
                    {
                        _points.Add(new Vector2(x2, y2));
                        _points.Add(new Vector2(x3, y3));
                        return;
                    }

                    if (_cuspLimit != 0)
                        if (da1 > _cuspLimit)
                        {
                            _points.Add(new Vector2(x2, y2));
                            return;
                        }
                }

                break;

            case 3:
                if (Fix64.Mul(Fix64.Add(d2, d3), Fix64.Add(d2, d3)) <= Fix64.Mul(_distanceToleranceSquare,
                        Fix64.Add(Fix64.Mul(dx, dx), Fix64.Mul(dy, dy))))
                {
                    if (_angleTolerance < MathUtils.AngleTolerance)
                    {
                        _points.Add(new Vector2(x23, y23));
                        return;
                    }

                    k = Trig256.Atan2(Fix64.Sub(y3, y2), Fix64.Sub(x3, x2));
                    da1 = Fix64.Abs(Trig256.Atan2(Fix64.Sub(y2, y1), Fix64.Sub(x2, x1)));
                    da2 = Fix64.Abs(Fix64.Sub(Trig256.Atan2(Fix64.Sub(y4, y3), Fix64.Sub(x4, x3)), k));

                    if (da1 >= Fix64.Pi) da1 = Fix64.Sub(Fix64.TwoPi, da1);
                    if (da2 >= Fix64.Pi) da2 = Fix64.Sub(Fix64.TwoPi, da2);

                    if (da1 + da2 < _angleTolerance)
                    {
                        _points.Add(new Vector2(x23, y23));
                        return;
                    }

                    if (_cuspLimit != 0)
                    {
                        if (da1 > _cuspLimit)
                        {
                            _points.Add(new Vector2(x2, y2));
                            return;
                        }

                        if (da2 > _cuspLimit)
                        {
                            _points.Add(new Vector2(x3, y3));
                            return;
                        }
                    }
                }

                break;
        }

        RecursiveBezier(x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1);
        RecursiveBezier(x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1);
    }
}