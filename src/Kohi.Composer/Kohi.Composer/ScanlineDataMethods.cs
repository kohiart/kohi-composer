// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class ScanlineDataMethods
{
    public static ScanlineData Create(int width, int scale)
    {
        var scanlineData = new ScanlineData();
        scanlineData.StartX = 0;
        scanlineData.StartY = 0;
        scanlineData.Status = ScanlineStatus.Initial;
        scanlineData.LastX = 0x7FFFFFF0;
        scanlineData.Covers = new byte[(width + 3) * scale];
        scanlineData.Spans = new ScanlineSpan[(width + 3) * scale];
        return scanlineData;
    }
}