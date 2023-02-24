// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class ColorMath
{
    public static uint ToColor(byte a, byte r, byte g, byte b)
    {
        uint c = 0;
        c |= (uint) a << 24;
        c |= (uint) r << 16;
        c |= (uint) g << 8;
        c |= (uint) b << 0;
        return c & 0xffffffff;
    }

    public static uint Lerp(uint s, uint t, long k)
    {
        var bk = Fix64.Sub(Fix64.One, k);

        var a = Fix64.Add(
            Fix64.Mul((byte) (s >> 24) * Fix64.One, bk),
            Fix64.Mul((byte) (t >> 24) * Fix64.One, k)
        );
        var r = Fix64.Add(
            Fix64.Mul((byte) (s >> 16) * Fix64.One, bk),
            Fix64.Mul((byte) (t >> 16) * Fix64.One, k)
        );
        var g = Fix64.Add(
            Fix64.Mul((byte) (s >> 8) * Fix64.One, bk),
            Fix64.Mul((byte) (t >> 8) * Fix64.One, k)
        );
        var b = Fix64.Add(
            Fix64.Mul((byte) (s >> 0) * Fix64.One, bk),
            Fix64.Mul((byte) (t >> 0) * Fix64.One, k)
        );

        var ra = (int) ((a / Fix64.One) << 24);
        var rr = (int) ((r / Fix64.One) << 16);
        var rg = (int) ((g / Fix64.One) << 8);
        var rb = (int) (b / Fix64.One);

        var x = ra | rr | rg | rb;
        return (uint) (x & 0xffffffff);
    }

    public static uint Tint(uint targetColor, uint tintColor)
    {
        var a = (byte) (targetColor >> 24);
        var r = (byte) (targetColor >> 16);
        var g = (byte) (targetColor >> 8);
        var b = (byte) (targetColor >> 0);

        if (a != 0 && r == 0 && g == 0 && b == 0) return targetColor;

        var tr = (byte) (tintColor >> 16);
        var tg = (byte) (tintColor >> 8);
        var tb = (byte) (tintColor >> 0);

        var tinted = ToColor(a, tr, tg, tb);
        return tinted;
    }
}