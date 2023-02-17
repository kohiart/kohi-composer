// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class MathExtensions
{
    public static long Abs(long x)
    {
        return x >= 0 ? x : -x;
    }
}