// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class CellBlockMethods
{
    public static CellBlock Create(int sampling, int scale)
    {
        var cb = new CellBlock();
        cb.Shift = sampling;
        cb.Size = 1 << cb.Shift;
        cb.Mask = cb.Size - 1;
        cb.Limit = cb.Size * scale * 2;
        return cb;
    }
}