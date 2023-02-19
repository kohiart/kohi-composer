// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class Trig256
{
    public const long LargePi = 7244019458077122842;
    public const long Ln2 = 0xB17217F7;
    public const long LnMax = 0x157CD0E702;
    public const long LnMin = -0x162E42FEFA;
    public const long E = -0x2B7E15162;
    public const int NumBits = 64;

    public static long Sin(long x)
    {
        var (clamped, flipHorizontal, flipVertical) = Clamp(x);

        var lutInterval = Fix64.Div((256 - 1) * Fix64.One, Fix64.PiOver2);
        var rawIndex = Fix64.Mul(clamped, lutInterval);
        var roundedIndex = Fix64.Round(rawIndex);
        var indexError = Fix64.Sub(rawIndex, roundedIndex);

        roundedIndex = roundedIndex >> 32; /* FRACTIONAL_PLACES */

        var nearestValueIndex = flipHorizontal
            ? 256 - 1 - roundedIndex
            : roundedIndex;

        var nearestValue = SinLut256.SinLut(nearestValueIndex);

        var secondNearestValue = SinLut256.SinLut(
            flipHorizontal
                ? 256 - 1 -
                  roundedIndex -
                  Fix64.Sign(indexError)
                : roundedIndex + Fix64.Sign(indexError)
        );

        var delta = Fix64.Mul(indexError, Fix64.Abs(Fix64.Sub(nearestValue, secondNearestValue)));
        var interpolatedValue = nearestValue + (flipHorizontal ? -delta : delta);
        var finalValue = flipVertical ? -interpolatedValue : interpolatedValue;

        return finalValue;
    }


    public static long Cos(long x)
    {
        var xl = x;
        long angle;
        if (xl > 0)
            angle = Fix64.Add(xl, Fix64.Sub(0 - Fix64.Pi, Fix64.PiOver2));
        else
            angle = Fix64.Add(xl, Fix64.PiOver2);
        return Sin(angle);
    }

    public static long Acos(long x)
    {
        if (x < -Fix64.One || x > Fix64.One) Revert("invalid range for x");
        if (x == 0) return Fix64.PiOver2;

        var t1 = Fix64.One - Fix64.Mul(x, x);
        var t2 = Fix64.Div(Sqrt(t1), x);

        var result = Atan(t2);
        return x < 0 ? result + Fix64.Pi : result;
    }

    public static long Atan(long z)
    {
        if (z == 0) return 0;

        var neg = z < 0;
        if (neg) z = -z;

        long result;
        var two = 2 * Fix64.One;
        var three = 3 * Fix64.One;

        var invert = z > Fix64.One;
        if (invert) z = Fix64.Div(Fix64.One, z);

        result = Fix64.One;
        var term = Fix64.One;

        var zSq = Fix64.Mul(z, z);
        var zSq2 = Fix64.Mul(zSq, two);
        var zSqPlusOne = Fix64.Add(zSq, Fix64.One);
        var zSq12 = Fix64.Mul(zSqPlusOne, two);
        var dividend = zSq2;
        var divisor = Fix64.Mul(zSqPlusOne, three);

        for (var i = 2; i < 30; ++i)
        {
            term = Fix64.Mul(term, Fix64.Div(dividend, divisor));
            result = Fix64.Add(result, term);

            dividend = Fix64.Add(dividend, zSq2);
            divisor = Fix64.Add(divisor, zSq12);

            if (term == 0) break;
        }

        result = Fix64.Mul(result, Fix64.Div(z, zSqPlusOne));

        if (invert) result = Fix64.Sub(Fix64.PiOver2, result);

        if (neg) result = -result;

        return result;
    }

    public static long Atan2(long y, long x)
    {
        var e = 1202590848L;
        var yl = y;
        var xl = x;

        if (xl == 0)
        {
            if (yl > 0) return Fix64.PiOver2;
            if (yl == 0) return 0;
            return -Fix64.PiOver2;
        }

        long atan;
        var z = Fix64.Div(y, x);

        if (Fix64.Add(Fix64.One, Fix64.Mul(e, Fix64.Mul(z, z))) == long.MaxValue)
            return y < 0 ? -Fix64.PiOver2 : Fix64.PiOver2;

        if (MathExtensions.Abs(z) < Fix64.One)
        {
            atan = Fix64.Div(z, Fix64.Add(Fix64.One, Fix64.Mul(e, Fix64.Mul(z, z))));
            if (xl < 0)
            {
                if (yl < 0) return Fix64.Sub(atan, Fix64.Pi);

                return Fix64.Add(atan, Fix64.Pi);
            }
        }
        else
        {
            atan = Fix64.Sub(Fix64.PiOver2, Fix64.Div(z, Fix64.Add(Fix64.Mul(z, z), e)));

            if (yl < 0) return Fix64.Sub(atan, Fix64.Pi);
        }

        return atan;
    }

    public static (long, bool, bool) Clamp(long x)
    {
        var clamped2Pi = x;
        for (byte i = 0; i < 29; ++i) clamped2Pi %= LargePi >> i;
        if (x < 0) clamped2Pi += Fix64.TwoPi;

        var flipVertical = clamped2Pi >= Fix64.Pi;
        var clampedPi = clamped2Pi;
        while (clampedPi >= Fix64.Pi) clampedPi -= Fix64.Pi;

        var flipHorizontal = clampedPi >= Fix64.PiOver2;

        var clampedPiOver2 = clampedPi;
        if (clampedPiOver2 >= Fix64.PiOver2)
            clampedPiOver2 -= Fix64.PiOver2;

        return (clampedPiOver2, flipHorizontal, flipVertical);
    }

    public static long Sqrt(long x)
    {
        var xl = x;
        if (xl < 0)
            throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");

        var num = (ulong) xl;
        var result = (ulong) 0;

        var bit = 1UL << (NumBits - 2);

        while (bit > num) bit >>= 2;

        for (byte i = 0; i < 2; ++i)
        {
            while (bit != 0)
            {
                if (num >= result + bit)
                {
                    num -= result + bit;
                    result = (result >> 1) + bit;
                }
                else
                {
                    result = result >> 1;
                }

                bit >>= 2;
            }

            if (i == 0)
            {
                if (num > (1UL << (NumBits / 2)) - 1)
                {
                    num -= result;
                    num = (num << (NumBits / 2)) - 0x80000000UL;
                    result = (result << (NumBits / 2)) + 0x80000000UL;
                }
                else
                {
                    num <<= NumBits / 2;
                    result <<= NumBits / 2;
                }

                bit = 1UL << (NumBits / 2 - 2);
            }
        }

        if (num > result) ++result;
        return (long) result;
    }

    public static long Log(long x)
    {
        return Fix64.Mul(Log2(x), Ln2);
    }

    internal static long Log2(long x)
    {
        if (x <= 0) Revert("non-positive value passed to log2");

        // This implementation is based on Clay. S. Turner's fast binary logarithm
        // algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
        //     Processing Mag., pp. 124,140, Sep. 2010.)

        long b = 1U << (Fix64.FractionalPlaces - 1);
        long y = 0;

        var rawX = x;
        while (rawX < Fix64.One)
        {
            rawX <<= 1;
            y -= Fix64.One;
        }

        while (rawX >= Fix64.One << 1)
        {
            rawX >>= 1;
            y += Fix64.One;
        }

        var z = rawX;

        for (var i = 0; i < Fix64.FractionalPlaces; i++)
        {
            z = Fix64.Mul(z, z);
            if (z >= Fix64.One << 1)
            {
                z = z >> 1;
                y += b;
            }

            b >>= 1;
        }

        return y;
    }

    internal static long Exp(long x)
    {
        if (x == 0) return Fix64.One;
        if (x == Fix64.One) return E;
        if (x >= LnMax) return Fix64.MaxValue;
        if (x <= LnMin) return 0;

        // The algorithm is based on the power series for exp(x):
        //  http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
        // 
        // From term n, we get term n+1 by multiplying with x/n.
        // When the sum term drops to zero, we can stop summing.
        //
        //
        // The power-series converges much faster on positive values
        // and exp(-x) = 1/exp(x).

        var neg = x < 0;
        if (neg) x = -x;

        var result = Fix64.Add(
            x,
            Fix64.One
        );
        var term = x;

        for (uint i = 2; i < 40; i++)
        {
            term = Fix64.Mul(
                x,
                Fix64.Div(term, i * Fix64.One)
            );
            result = Fix64.Add(result, term);
            if (term == 0) break;
        }

        if (neg) result = Fix64.Div(Fix64.One, result);

        return result;
    }

    public static void Revert(string message)
    {
        throw new InvalidOperationException(message);
    }
}