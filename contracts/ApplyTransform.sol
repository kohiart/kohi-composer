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

import "./VertexData.sol";
import "./Matrix.sol";

library ApplyTransform {
    function applyTransform(
        VertexData[] memory vertices,
        Matrix memory transform,
        VertexData[] memory transformed
    ) internal pure {
        for (uint32 i = 0; i < vertices.length; i++) {
            if (
                vertices[i].command != Command.Stop &&
                vertices[i].command != Command.EndPoly
            ) {
                (int64 x, int64 y) = MatrixMethods.transform(
                    transform,
                    vertices[i].position.x,
                    vertices[i].position.y
                );
                transformed[i].command = vertices[i].command;
                transformed[i].position.x = x;
                transformed[i].position.y = y;
            }
        }
    }
}
