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

struct RectangleInt {
    int32 left;
    int32 bottom;
    int32 right;
    int32 top;
}

library RectangleIntMethods {
    function normalize(RectangleInt memory rect) internal pure {
        int32 t;

        if (rect.left > rect.right) {
            t = rect.left;
            rect.left = rect.right;
            rect.right = t;
        }

        if (rect.bottom > rect.top) {
            t = rect.bottom;
            rect.bottom = rect.top;
            rect.top = t;
        }
    }
}
