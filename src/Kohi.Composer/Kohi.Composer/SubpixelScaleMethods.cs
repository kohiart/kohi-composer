// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class SubpixelScaleMethods
{
    public static SubpixelScale Create(int sampling)
    {
        var ss = new SubpixelScale();
        ss.Value = sampling;
        ss.Scale = 1 << ss.Value;
        ss.Mask = ss.Scale - 1;
        ss.DxLimit = 16384 << ss.Value;
        return ss;
    }
}