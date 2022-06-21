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

import "./ScanlineStatus.sol";
import "./ScanlineSpan.sol";
import "./AntiAlias.sol";

struct ScanlineData {
    int32 scanY;
    int32 startX;
    int32 startY;
    ScanlineStatus status;
    int32 coverIndex;
    uint8[] covers;
    int32 spanIndex;
    ScanlineSpan[] spans;
    int32 current;
    int32 lastX;
    int32 y;
}

library ScanlineDataMethods {
    function create(uint32 width)
        external
        pure
        returns (ScanlineData memory scanlineData)
    {
        scanlineData.startX = 0;
        scanlineData.startY = 0;
        scanlineData.status = ScanlineStatus.Initial;
        scanlineData.lastX = 0x7FFFFFF0;
        scanlineData.covers = new uint8[](width + 3);
        scanlineData.spans = new ScanlineSpan[](width + 3);
        return scanlineData;
    }
}
