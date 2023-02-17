// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class Fix64
{
    public const int FractionalPlaces = 32;
    public const long One = 4294967296; // 1 << FRACTIONAL_PLACES
    public const long Two = One * 2;
    public const long Pi = 0x3243F6A88;
    public const long TwoPi = 0x6487ED511;
    public const long MaxValue = long.MaxValue;
    public const long MinValue = long.MinValue;
    public const long PiOver2 = 0x1921FB544;
    public const int NumBits = 64;

    public static long Mul(long x, long y)
    {
        var xl = x;
        var yl = y;

        var xlo = (ulong) (xl & 0x00000000FFFFFFFF);
        var xhi = xl >> 32; // FRACTIONAL_PLACES
        var ylo = (ulong) (yl & 0x00000000FFFFFFFF);
        var yhi = yl >> 32; // FRACTIONAL_PLACES

        var lolo = xlo * ylo;
        var lohi = (long) xlo * yhi;
        var hilo = xhi * (long) ylo;
        var hihi = xhi * yhi;

        var loResult = lolo >> 32; // FRACTIONAL_PLACES
        var midResult1 = lohi;
        var midResult2 = hilo;
        var hiResult = hihi << 32; // FRACTIONAL_PLACES

        var sum = (long) loResult + midResult1 + midResult2 + hiResult;

        return sum;
    }

    public static long Floor(long x)
    {
        return (long) ((ulong) x & 0xFFFFFFFF00000000);
    }

    public static long Round(long x)
    {
        var fractionalPart = x & 0x00000000FFFFFFFF;
        var integralPart = Floor(x);
        if (fractionalPart < 0x80000000) return integralPart;
        if (fractionalPart > 0x80000000) return integralPart + One;
        if ((integralPart & One) == 0) return integralPart;
        return integralPart + One;
    }

    public static long Div(long x, long y)
    {
        var xl = x;
        var yl = y;

        if (yl == 0) throw new DivideByZeroException();

        var remainder = (ulong) (xl >= 0 ? xl : -xl);
        var divider = (ulong) (yl >= 0 ? yl : -yl);
        var quotient = 0UL;
        var bitPos = NumBits / 2 + 1;

        while ((divider & 0xF) == 0 && bitPos >= 4)
        {
            divider >>= 4;
            bitPos -= 4;
        }

        while (remainder != 0 && bitPos >= 0)
        {
            var shift = CountLeadingZeros(remainder);
            if (shift > bitPos) shift = bitPos;
            remainder <<= shift;
            bitPos -= shift;

            var div = remainder / divider;
            remainder = remainder % divider;
            quotient += div << bitPos;

            if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
                return ((xl ^ yl) & MinValue) == 0 ? MaxValue : MinValue;

            remainder <<= 1;
            --bitPos;
        }

        ++quotient;
        var result = (long) (quotient >> 1);
        if (((xl ^ yl) & MinValue) != 0) result = -result;

        return result;
    }

    private static int CountLeadingZeros(ulong x)
    {
        var result = 0;
        while ((x & 0xF000000000000000) == 0)
        {
            result += 4;
            x <<= 4;
        }

        while ((x & 0x8000000000000000) == 0)
        {
            result += 1;
            x <<= 1;
        }

        return result;
    }

    public static long Add(long x, long y)
    {
        var xl = x;
        var yl = y;
        var sum = xl + yl;
        if ((~(xl ^ yl) & (xl ^ sum) & MinValue) != 0) sum = xl > 0 ? MaxValue : MinValue;
        return sum;
    }

    public static long Sub(long x, long y)
    {
        var xl = x;
        var yl = y;
        var diff = xl - yl;
        if (((xl ^ yl) & (xl ^ diff) & MinValue) != 0) diff = xl < 0 ? MinValue : MaxValue;
        return diff;
    }

    public static int Sign(long x)
    {
        return x == 0 ? 0 : x > 0 ? 1 : -1;
    }

    public static long Abs(long x)
    {
        // http://www.strchr.com/optimized_abs_function
        var mask = x >> 63;
        return (x + mask) ^ mask;
    }

    public static long Max(long a, long b)
    {
        return a >= b ? a : b;
    }

    public static long Min(long a, long b)
    {
        return a < b ? a : b;
    }

    public static long Map(long n, long start1, long stop1, long start2, long stop2)
    {
        var value = Mul(
            Div(Sub(n, start1), Sub(stop1, start1)),
            Add(Sub(stop2, start2), start2));

        return start2 < stop2 ? Constrain(value, start2, stop2) : Constrain(value, stop2, start2);
    }

    public static long Constrain(long n, long low, long high)
    {
        return Max(Min(n, high), low);
    }
}