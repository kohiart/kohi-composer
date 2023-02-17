// Copyright Kohi Art Community, Inc.. All rights reserved.

using Kohi.Composer.V1;

namespace Kohi.Composer
{
    public class RandomV1
    {
        private const int Big = 0x7fffffff;
        private const int Seed = 161803398;

        public static PRNG BuildSeedTable(int seed)
        {
            byte ii = 0;
            int mj;
            int mk;

            var prng = new PRNG();
            prng.SeedArray = new int[56];

            var subtraction = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);
            mj = Seed - subtraction;
            prng.SeedArray[55] = mj;
            mk = 1;
            for (var i = 1; i < 55; i++)
            {
                if ((ii += 21) >= 55) ii -= 55;
                prng.SeedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += Big;
                mj = prng.SeedArray[ii];
            }

            for (var k = 1; k < 5; k++)
            for (var i = 1; i < 56; i++)
            {
                var n = i + 30;
                if (n >= 55) n -= 55;

                long an = prng.SeedArray[1 + n];
                long ai = prng.SeedArray[i];
                prng.SeedArray[i] = (int)(ai - an);

                if (prng.SeedArray[i] < 0)
                {
                    long x = prng.SeedArray[i];
                    x += Big;
                    prng.SeedArray[i] = (int)x;
                }
            }

            prng.Inextp = 21;
            return prng;
        }

        public static int Next(PRNG prng, int maxValue)
        {
            var retval = Next(prng);
            var fretval = retval * Fix64.One;
            var sample = Fix64.Mul(fretval, Fix64.Div(Fix64.One, Big * Fix64.One));
            var sr = Fix64.Mul(sample, maxValue * Fix64.One);
            var r = (int)(sr >> 32); /* FRACTIONAL_PLACES */
            ;
            return r;
        }

        public static int Next(PRNG prng, int minValue, int maxValue)
        {
            int64 range = maxValue - minValue;

            if (range <= int.MaxValue)
            {
                int32 retval = Next(prng);

                int64 fretval = retval * Fix64.One;
                int64 sample = Fix64.Mul(fretval, Fix64.Div(Fix64.One, Big * Fix64.One));
                int64 sr = Fix64.Mul(sample, range * Fix64.One);
                int32 r = (int32)(sr >> 32  /* FRACTIONAL_PLACES */) + minValue;

                return r;
            }
            else
            {
                int64 fretval = NextForLargeRange(prng);
                int64 sr = Fix64.Mul(fretval, range * Fix64.One);
                int32 r = (int32)(sr >> 32 /* FRACTIONAL_PLACES */) + minValue;
                return r;
            }
        }

        public static int Next(PRNG prng)
        {
            int64 retVal;
            int32 locINext = prng.Inext;
            int32 locINextp = prng.Inextp;

            if (++locINext >= 56) locINext = 1;
            if (++locINextp >= 56) locINextp = 1;

            int64 a = prng.SeedArray[(uint)locINext];
            int64 b = prng.SeedArray[(uint)locINextp];
            retVal = a - b;

            if (retVal == Big) retVal--;
            if (retVal < 0) retVal += Big;

            prng.SeedArray[locINext] = (int32)retVal;
            prng.Inext = locINext;
            prng.Inextp = locINextp;

            int32 r = (int32)retVal;
            return r;
        }

        public static long NextForLargeRange(PRNG prng)
        {
            var sample1 = Next(prng);
            var sample2 = Next(prng);

            var negative = sample2 % 2 == 0;
            if (negative) sample1 = -sample1;

            var d = sample1 * Fix64.One;
            d = Fix64.Add(d, int.MaxValue - 1);
            d = Fix64.Div(d, 2L * (int.MaxValue - 1));

            return d;
        }

        public static long NextGaussian(PRNG prng)
        {
            var u1 = Fix64.Sub(Fix64.One, Fix64.Mul(Next(prng) * Fix64.One, Fix64.Div(Fix64.One, Fix64.MaxValue)));
            var u2 = Fix64.Sub(Fix64.One, Fix64.Mul(Next(prng) * Fix64.One, Fix64.Div(Fix64.One, Fix64.MaxValue)));
            var sqrt = Trig256.Sqrt(Fix64.Mul(-2 * Fix64.One, Trig256.Log(u1)));
            var randStdNormal = Fix64.Mul(sqrt, Trig256.Sin(Fix64.Mul(Fix64.Two, Fix64.Mul(Fix64.Pi, u2))));
            var randNormal = Fix64.Add(0, Fix64.Mul(Fix64.One, randStdNormal));
            return randNormal;
        }
    }
}