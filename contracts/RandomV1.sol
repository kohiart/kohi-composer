// SPDX-License-Identifier: UNLICENSED
/* Copyright (c) 2021 Kohi Art Community, Inc. All rights reserved. */

pragma solidity ^0.8.0;

import "./Fix64.sol";
import "./Trig256.sol";

/*
    A pseudo-random number generator, adapted from and matching the algorithm for .NET maximum compatibility Random implementation.

    See: https://github.com/dotnet/runtime/blob/f7633f498a8be34bee739b240a0aa9ae6a660cd9/src/libraries/System.Private.CoreLib/src/System/Random.Net5CompatImpl.cs#L192
         https://github.com/dotnet/runtime/blob/main/LICENSE.TXT

    THIRD PARTY NOTICES:
    ====================

    The MIT License (MIT)

    Copyright (c) .NET Foundation and Contributors

    All rights reserved.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

library RandomV1 {

    int32 private constant MBIG = 0x7fffffff;
    int32 private constant MSEED = 161803398;

    struct PRNG {
        int32[56] _seedArray;
        int32 _inext;
        int32 _inextp;
    }
    
    function buildSeedTable(int32 seed) internal pure returns(PRNG memory prng) {
        uint8 ii = 0;
        int32 mj;
        int32 mk;

        int32 subtraction = (seed == type(int32).min) ? type(int32).max : int32(abs(seed));
        mj = MSEED - subtraction;
        prng._seedArray[55] = mj;
        mk = 1;
        for (uint8 i = 1; i < 55; i++) {  
            if ((ii += 21) >= 55) {
                ii -= 55;
            }
            prng._seedArray[uint64(ii)] = mk;
            mk = mj - mk;
            if (mk < 0) mk += MBIG;
            mj = prng._seedArray[uint8(ii)];
        }

        for (uint8 k = 1; k < 5; k++) {

            for (uint8 i = 1; i < 56; i++) {                
                uint8 n = i + 30;           
                if (n >= 55) {
                    n -= 55;                
                }

                int64 an = prng._seedArray[1 + n];                
                int64 ai = prng._seedArray[i];
                prng._seedArray[i] = int32(ai - an);
                
                if (prng._seedArray[i] < 0) {
                    int64 x = prng._seedArray[i];
                    x += MBIG;
                    prng._seedArray[i] = int32(x);
                }               
            }
        }

        prng._inextp = 21;
    }   

    function next(PRNG memory prng, int32 maxValue) internal pure returns (int32) {
        int32 retval = next(prng);

        int64 fretval = retval * Fix64.ONE;
        int64 sample = Fix64.mul(fretval, Fix64.div(Fix64.ONE, MBIG * Fix64.ONE));
        int64 sr = Fix64.mul(sample, maxValue * Fix64.ONE);
        int32 r = int32(sr >> 32 /* FRACTIONAL_PLACES */);

        return r;
    }

    function next(PRNG memory prng, int32 minValue, int32 maxValue) internal pure returns(int32) {
        int64 range = maxValue - minValue;
        
        if (range <= type(int32).max) {
            int32 retval = next(prng);

            int64 fretval = retval * Fix64.ONE;
            int64 sample = Fix64.mul(fretval, Fix64.div(Fix64.ONE, MBIG * Fix64.ONE));
            int64 sr = Fix64.mul(sample, range * Fix64.ONE);
            int32 r = int32(sr >> 32  /* FRACTIONAL_PLACES */) + minValue;
            
            return r;
        }
        else {
            int64 fretval = nextForLargeRange(prng);
            int64 sr = Fix64.mul(fretval, range * Fix64.ONE);
            int32 r = int32(sr >> 32  /* FRACTIONAL_PLACES */) + minValue;
            return r;
        }
    }

    function next(PRNG memory prng) internal pure returns(int32) {

        int64 retVal;        
        int32 locINext = prng._inext;
        int32 locINextp = prng._inextp;

        if (++locINext >= 56) locINext = 1;
        if (++locINextp >= 56) locINextp = 1;

        int64 a = int64(prng._seedArray[uint32(locINext)]);
        int64 b = int64(prng._seedArray[uint32(locINextp)]);
        retVal = a - b;        

        if (retVal == MBIG) {
            retVal--;
        }
        if (retVal < 0) {
            retVal += MBIG;
        }

        prng._seedArray[uint32(locINext)] = int32(retVal);
        prng._inext = locINext;
        prng._inextp = locINextp;        

        int32 r = int32(retVal);
        return r;
    }

    function nextForLargeRange(PRNG memory prng) private pure returns(int64) {

        int sample1 = next(prng);
        int sample2 = next(prng);

        bool negative = sample2 % 2 == 0;
        if (negative) {
            sample1 = -sample1;
        }

        int64 d = int64(sample1) * Fix64.ONE;
        d = Fix64.add(int64(d), (type(int32).max - 1));
        d = Fix64.div(int64(d), int64(2) * (type(int32).max - 1));

        return d; 
    }

    function nextGaussian(PRNG memory prng) internal pure returns (int64 randNormal) {
        int64 u1 = Fix64.sub(Fix64.ONE, Fix64.mul(next(prng) * Fix64.ONE, Fix64.div(Fix64.ONE, Fix64.MAX_VALUE)));
        int64 u2 = Fix64.sub(Fix64.ONE, Fix64.mul(next(prng) * Fix64.ONE, Fix64.div(Fix64.ONE, Fix64.MAX_VALUE)));
        int64 sqrt = Trig256.sqrt(Fix64.mul(-2 * Fix64.ONE, Trig256.log(u1)));
        int64 randStdNormal = Fix64.mul(sqrt, Trig256.sin(Fix64.mul(Fix64.TWO, Fix64.mul(Fix64.PI, u2))));
        randNormal = Fix64.add(0, Fix64.mul(Fix64.ONE, randStdNormal));
        return randNormal;
    }

    function abs(int256 x) internal pure returns (int256) {
        return x >= 0 ? x : -x;
    }
}