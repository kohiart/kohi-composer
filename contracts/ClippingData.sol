// SPDX-License-Identifier: MIT
/* Copyright (c) Kohi Art Community, Inc. All rights reserved. */

/*
/*
///////////////////////////////////////////////////////////////////////////////////
//                                                                               //
//     @@@@@@@@@@@@@@                        @@@@                                // 
//               @@@@                        @@@@ @@@@@@@@                       // 
//               @@@@    @@@@@@@@@@@@@@@@    @@@@@@@          @@@@@@@@@@@@@@@@   // 
//               @@@@                        @@@@                                // 
//     @@@@@@@@@@@@@@                        @@@@@@@@@@@@@                       // 
//               @@@@                          @@@@@@@@@@@                       // 
//                                                                               //
///////////////////////////////////////////////////////////////////////////////////
*/

pragma solidity ^0.8.13;

import "./Matrix.sol";
import "./RectangleInt.sol";
import "./SubpixelScale.sol";

struct ClippingData {
    int32 f1;
    int32 x1;
    int32 y1;
    RectangleInt clipBox;
    bool clipping;
    Vector2[] clipPoly;
}

library ClippingDataMethods {
    function create(
        uint32 width,
        uint32 height,
        SubpixelScale calldata ss
    ) external pure returns (ClippingData memory clippingData) {
        clippingData.x1 = 0;
        clippingData.y1 = 0;
        clippingData.f1 = 0;
        clippingData.clipBox = RectangleInt(
            0,
            0,
            upscale(int64(int32(width) * Fix64.ONE), ss),
            upscale(int64(int32(height) * Fix64.ONE), ss)
        );
        RectangleIntMethods.normalize(clippingData.clipBox);
        clippingData.clipping = true;
        return clippingData;
    }

    function upscale(int64 v, SubpixelScale memory ss)
        internal
        pure
        returns (int32)
    {
        return
            int32(
                Fix64.round(Fix64.mul(v, int32(ss.scale) * Fix64.ONE)) /
                    Fix64.ONE
            );
    }
}
