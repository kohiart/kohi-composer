// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public struct VertexDistance
{
    public long X;
    public long Y;
    public long Distance;

    public VertexDistance(long x, long y)
    {
        X = x;
        Y = y;
        Distance = 0;
    }

    public bool IsEqual(VertexDistance other)
    {
        var d = Distance = MathUtils.CalcDistance(X, Y, other.X, other.Y);
        var r = d > MathUtils.Epsilon;
        if (!r) Distance = Fix64.Div(Fix64.One, MathUtils.Epsilon);
        return r;
    }
}