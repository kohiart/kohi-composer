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
import "./Arc.sol";
import "./Rectangle.sol";

struct RoundedRect {
    Rectangle bounds;
    Matrix transform;
    Vector2 leftBottomRadius;
    Vector2 leftTopRadius;
    Vector2 rightBottomRadius;
    Vector2 rightTopRadius;
    int32 yShift;
}

library RoundedRectMethods {
    function create(
        int64 left,
        int64 bottom,
        int64 right,
        int64 top,
        int64 radius,
        Matrix memory transform
    ) external pure returns (RoundedRect memory rect) {
        rect.bounds = Rectangle(left, bottom, right, top);

        rect.leftBottomRadius.x = radius;
        rect.leftBottomRadius.y = radius;
        rect.rightBottomRadius.x = radius;
        rect.rightBottomRadius.y = radius;
        rect.rightTopRadius.x = radius;
        rect.rightTopRadius.y = radius;
        rect.leftTopRadius.x = radius;
        rect.leftTopRadius.y = radius;

        rect.transform = transform;

        if (left > right) {
            rect.bounds = Rectangle(right, bottom, left, top);
        }

        if (bottom > top) {
            rect.bounds = Rectangle(left, top, right, bottom);
        }
    }

    function vertices(RoundedRect memory self)
        external
        pure
        returns (VertexData[] memory results)
    {
        uint32 count = 0;
        results = new VertexData[](1000);

        Vector2 memory v0 = MatrixMethods.transform(
            self.transform,
            Vector2(
                Fix64.add(self.bounds.left, self.leftBottomRadius.x),
                Fix64.add(self.bounds.bottom, self.leftBottomRadius.y)
            )
        );

        count = join(
            results,
            ArcMethods.create(
                v0.x,
                Fix64.sub(self.yShift * Fix64.ONE, v0.y),
                self.leftBottomRadius.x,
                self.leftBottomRadius.y,
                Fix64.PI,
                Fix64.add(Fix64.PI, Fix64.PI_OVER_2)
            ),
            count
        );

        Vector2 memory v1 = MatrixMethods.transform(
            self.transform,
            Vector2(
                Fix64.sub(self.bounds.right, self.rightBottomRadius.x),
                Fix64.add(self.bounds.bottom, self.rightBottomRadius.y)
            )
        );

        count = join(
            results,
            ArcMethods.create(
                v1.x,
                Fix64.sub(self.yShift * Fix64.ONE, v1.y),
                self.rightBottomRadius.x,
                self.rightBottomRadius.y,
                Fix64.add(Fix64.PI, Fix64.PI_OVER_2),
                0
            ),
            count
        );

        Vector2 memory v2 = MatrixMethods.transform(
            self.transform,
            Vector2(
                Fix64.sub(self.bounds.right, self.rightTopRadius.x),
                Fix64.sub(self.bounds.top, self.rightTopRadius.y)
            )
        );

        count = join(
            results,
            ArcMethods.create(
                v2.x,
                Fix64.sub(self.yShift * Fix64.ONE, v2.y),
                self.rightTopRadius.x,
                self.rightTopRadius.y,
                0,
                Fix64.PI_OVER_2
            ),
            count
        );

        Vector2 memory v3 = MatrixMethods.transform(
            self.transform,
            Vector2(
                Fix64.add(self.bounds.left, self.leftTopRadius.x),
                Fix64.sub(self.bounds.top, self.leftTopRadius.y)
            )
        );

        count = join(
            results,
            ArcMethods.create(
                v3.x,
                Fix64.sub(self.yShift * Fix64.ONE, v3.y),
                self.leftTopRadius.x,
                self.leftTopRadius.y,
                Fix64.PI_OVER_2,
                Fix64.PI
            ),
            count
        );

        results[count++] = VertexData(Command.EndPoly, Vector2(0, 0));
        results[count++] = (VertexData(Command.Stop, Vector2(0, 0)));

        return results;
    }

    function join(
        VertexData[] memory v,
        Arc memory sourcePath,
        uint32 count
    ) private pure returns (uint32) {
        bool firstMove = true;
        VertexData[] memory results = ArcMethods.vertices(sourcePath);
        for (uint32 j = 0; j < results.length; j++) {
            VertexData memory vertexData = results[j];
            if (j > 0 && firstMove && vertexData.command == Command.MoveTo) {
                firstMove = false;
                continue;
            }

            if (vertexData.command == Command.Stop) break;
            v[count++] = vertexData;
        }
        return count;
    }
}
