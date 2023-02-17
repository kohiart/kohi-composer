// Copyright Kohi Art Community, Inc.. All rights reserved.

namespace Kohi.Composer.V1;

public sealed class PRNG
{
    public int[] SeedArray { get; set; }
    public int Inext { get; set; }
    public int Inextp { get; set; }
}