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

import "./Fix64.sol";
import "./Trig256.sol";
import "./MathUtils.sol";

struct VertexDistance {
    int64 x;
    int64 y;
    int64 distance;
}

library VertexDistanceMethods {
    function isEqual(
        VertexDistance memory self,
        VertexDistance memory other
    ) internal pure returns (bool) {
        int64 d = self.distance = MathUtils.calcDistance(
            self.x,
            self.y,
            other.x,
            other.y
        );
        bool r = d > MathUtils.Epsilon;
        if (!r) {
            self.distance = Fix64.div(Fix64.ONE, MathUtils.Epsilon);
        }
        return r;
    }
}
