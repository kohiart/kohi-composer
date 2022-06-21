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
import "./CellData.sol";
import "./Vector2.sol";
import "./VertexData.sol";

struct Arc {
    Vector2 origin;
    Vector2 radius;
    int64 angle;
    int64 startAngle;
    int64 deltaAngle;
    int32 steps;
}

library ArcMethods {
    function create(
        int64 originX,
        int64 originY,
        int64 radiusX,
        int64 radiusY,
        int64 startAngle,
        int64 angle
    ) external pure returns (Arc memory data) {
        data.origin = Vector2(originX, originY);
        data.radius = Vector2(radiusX, radiusY);
        data.startAngle = startAngle;

        int64 averageRadius = Fix64.div(
            Fix64.add(Fix64.abs(data.radius.x), Fix64.abs(data.radius.y)),
            Fix64.TWO
        );

        data.deltaAngle = Fix64.mul(
            Trig256.acos(
                Fix64.div(
                    averageRadius,
                    Fix64.add(
                        averageRadius,
                        Fix64.div(
                            536870912,
                            /* 0.125 */
                            Fix64.ONE
                        )
                    )
                )
            ),
            Fix64.TWO
        );

        while (angle < data.startAngle) angle = Fix64.add(angle, Fix64.TWO_PI);

        data.angle = angle;

        data.steps = (int32)(
            Fix64.div(Fix64.sub(data.angle, data.startAngle), data.deltaAngle) /
                Fix64.ONE
        );
    }

    function vertices(Arc memory data)
        public
        pure
        returns (VertexData[] memory results)
    {
        results = new VertexData[](uint32(data.steps + 3));

        VertexData memory vertexData;
        vertexData.command = Command.MoveTo;

        {
            vertexData.position = Vector2(
                Fix64.add(
                    data.origin.x,
                    Fix64.mul(Trig256.cos(data.startAngle), data.radius.x)
                ),
                Fix64.add(
                    data.origin.y,
                    Fix64.mul(Trig256.sin(data.startAngle), data.radius.y)
                )
            );

            results[0] = vertexData;
        }

        vertexData.command = Command.LineTo;
        int64 angle = data.startAngle;

        for (uint32 i = 0; i <= uint32(data.steps); i++) {
            if (angle < data.angle) {
                vertexData.position = Vector2(
                    Fix64.add(
                        data.origin.x,
                        Fix64.mul(Trig256.cos(angle), data.radius.x)
                    ),
                    Fix64.add(
                        data.origin.y,
                        Fix64.mul(Trig256.sin(angle), data.radius.y)
                    )
                );

                results[1 + i] = vertexData;

                angle = Fix64.add(angle, data.deltaAngle);
            }
        }

        {
            vertexData.position = Vector2(
                Fix64.add(
                    data.origin.x,
                    Fix64.mul(Trig256.cos(angle), data.radius.x)
                ),
                Fix64.add(
                    data.origin.y,
                    Fix64.mul(Trig256.sin(angle), data.radius.y)
                )
            );

            results[uint32(data.steps) + 1] = vertexData;
        }

        vertexData.command = Command.Stop;
        results[uint32(data.steps) + 2] = vertexData;

        return results;
    }
}
