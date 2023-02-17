// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class ClippingDataMethods
{
    public static ClippingData Create(int width, int height, SubpixelScale ss, int scale)
    {
        width = width * scale;
        height = height * scale;

        var clippingData = new ClippingData();
        clippingData.X1 = 0;
        clippingData.Y1 = 0;
        clippingData.F1 = 0;
        clippingData.ClipBox = new RectangleInt(0, 0,
            Upscale(width * Fix64.One, ss),
            Upscale(height * Fix64.One, ss)
        );
        clippingData.ClipBox.Normalize();
        clippingData.Clipping = true;
        return clippingData;
    }

    public static int Upscale(long v, SubpixelScale ss)
    {
        return (int) (Fix64.Round(Fix64.Mul(v, ss.Scale * Fix64.One)) / Fix64.One);
    }
}