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

import "./LineClipY.sol";
import "./Line.sol";
import "./LineArgs.sol";
import "./RenderHorizontalLine.sol";
import "./RenderHorizontalLineArgs.sol";
import "./VertexData.sol";
import "./Matrix.sol";
import "./ScanlineSpan.sol";
import "./Cell.sol";
import "./BlendSolidHorizontalSpan.sol";
import "./BlendHorizontalLine.sol";

struct DrawContext {
    LineClipY lineClipY;
    Line line;
    Line lineRecursive;
    LineArgs lineArgs;
    RenderHorizontalLine horizontalLine;
    RenderHorizontalLineArgs horizontalLineArgs;
    BlendHorizontalLine blendHorizontalLine;
    BlendSolidHorizontalSpan blendSolidHorizontalSpan;
    Matrix t;
    VertexData[] transformed;
    uint32 color;
    uint32 tint;
    ScanlineSpan scanlineSpan;
    Cell current;
}
