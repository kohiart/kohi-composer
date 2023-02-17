// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class AntiAliasMethods
{
    public static AntiAlias Create(int sampling)
    {
        var aa = new AntiAlias();
        aa.Value = sampling;
        aa.Scale = 1 << aa.Value;
        aa.Mask = aa.Scale - 1;
        return aa;
    }
}