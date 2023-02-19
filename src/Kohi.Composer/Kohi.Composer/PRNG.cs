// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public sealed class PRNG
{
    public int[] SeedArray { get; set; } = null!;
    public int Inext { get; set; }
    public int Inextp { get; set; }
}