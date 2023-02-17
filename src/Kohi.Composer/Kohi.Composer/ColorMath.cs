// Copyright (c) Kohi Art Community, Inc.

using System.Drawing;

namespace Kohi.Composer;

public static class ColorMath
{
    public static uint ToColor(byte a, byte r, byte g, byte b)
    {
        return Color.FromArgb(a, r, g, b).ToUInt32();
    }

    public static uint Lerp(uint s, uint t, long k)
    {
        var bk = Fix64.Sub(Fix64.One, k);

        var sA = (byte) (s >> 24);
        var sR = (byte) (s >> 16);
        var sG = (byte) (s >> 8);
        var sB = (byte) (s >> 0);

        var tA = (byte) (t >> 24);
        var tR = (byte) (t >> 16);
        var tG = (byte) (t >> 8);
        var tB = (byte) (t >> 0);

        var a = Fix64.Add(Fix64.Mul(sA * Fix64.One, bk), Fix64.Mul(tA * Fix64.One, k));
        var r = Fix64.Add(Fix64.Mul(sR * Fix64.One, bk), Fix64.Mul(tR * Fix64.One, k));
        var g = Fix64.Add(Fix64.Mul(sG * Fix64.One, bk), Fix64.Mul(tG * Fix64.One, k));
        var b = Fix64.Add(Fix64.Mul(sB * Fix64.One, bk), Fix64.Mul(tB * Fix64.One, k));

        var ra = (int) (a / Fix64.One) << 24;
        var rr = (int) (r / Fix64.One) << 16;
        var rg = (int) (g / Fix64.One) << 8;
        var rb = (int) (b / Fix64.One) << 0;

        return (uint) ((ra | rr | rg | rb) & 0xffffffff);
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