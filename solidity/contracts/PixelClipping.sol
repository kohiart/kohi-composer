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

import "./Vector2.sol";
import "./Fix64.sol";

struct PixelClipping {
    Vector2[] area;
    int32 x;
    int32 y;
}

library PixelClippingMethods {
    function isPointInPolygon(
        PixelClipping memory self,
        int32 px,
        int32 py
    ) internal pure returns (bool) {
        if (self.area.length < 3) {
            return false;
        }

        Vector2 memory oldPoint = self.area[self.area.length - 1];

        bool inside = false;

        for (uint256 i = 0; i < self.area.length; i++) {
            Vector2 memory newPoint = self.area[i];

            Vector2 memory p2;
            Vector2 memory p1;

            if (newPoint.x > oldPoint.x) {
                p1 = oldPoint;
                p2 = newPoint;
            } else {
                p1 = newPoint;
                p2 = oldPoint;
            }

            int64 pxF = px * Fix64.ONE;
            int64 pyF = py * Fix64.ONE;

            int64 t1 = Fix64.sub(pyF, p1.y);
            int64 t2 = Fix64.sub(p2.x, p1.x);
            int64 t3 = Fix64.sub(p2.y, p1.y);
            int64 t4 = Fix64.sub(pxF, p1.x);

            if (
                newPoint.x < pxF == pxF <= oldPoint.x &&
                Fix64.mul(t1, t2) < Fix64.mul(t3, t4)
            ) inside = !inside;

            oldPoint = newPoint;
        }

        return inside;
    }
}
