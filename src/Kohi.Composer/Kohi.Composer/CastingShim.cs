// Copyright (c) Kohi Art Community, Inc.

// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace Kohi.Composer;

public class CastingShim
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Solidity")]
    public static byte uint8(int v)
    {
        return (byte) v;
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Solidity")]
    public static byte uint8(uint v)
    {
        return (byte) v;
    }
}