// Copyright (c) Kohi Art Community, Inc.

using System.Drawing;

namespace Kohi.Composer;

public static class ColorExtensions
{
    public static uint ToUInt32(this Color c)
    {
        return (uint) (((c.A << 24) | (c.R << 16) | (c.G << 8) | c.B) & 0xffffffff);
    }

    public static Color ToColor(this uint u)
    {
        var color = Color.FromArgb(
            (byte) (u >> 24),
            (byte) (u >> 16),
            (byte) (u >> 8), (byte) u);
        return color;
    }

    public static string Html(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
    }
}