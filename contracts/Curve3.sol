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
import "./Fix64.sol";
import "./MathUtils.sol";

library Curve3 {
    struct CurveData {
        uint32 pointCount;
        Vector2[] points;
        int64 distanceToleranceSquare;
    }

    function create(
        int64 x1,
        int64 y1,
        int64 cx,
        int64 cy,
        int64 x2,
        int64 y2
    ) external pure returns (CurveData memory curve) {
        curve.points = new Vector2[](2000);
        curve.distanceToleranceSquare = 2147483648; /* 0.5 */
        curve.distanceToleranceSquare = Fix64.mul(
            curve.distanceToleranceSquare,
            curve.distanceToleranceSquare
        );
        bezier(curve, x1, y1, cx, cy, x2, y2);
    }

    function vertices(
        CurveData memory data
    ) external pure returns (VertexData[] memory results) {
        results = new VertexData[](data.pointCount + 1);
        for (uint32 i = 0; i < data.pointCount; i++) {
            if (i == 0) {
                results[i] = VertexData(Command.MoveTo, data.points[i]);
            } else {
                results[i] = VertexData(Command.LineTo, data.points[i]);
            }
        }
        results[data.pointCount] = VertexData(Command.Stop, Vector2(0, 0));
        return results;
    }

    function bezier(
        CurveData memory self,
        int64 x1,
        int64 y1,
        int64 x2,
        int64 y2,
        int64 x3,
        int64 y3
    ) private pure {
        self.points[self.pointCount++] = Vector2(x1, y1);
        recursiveBezier(self, RecursiveBezier(x1, y1, x2, y2, x3, y3, 0));
        self.points[self.pointCount++] = Vector2(x3, y3);
    }

    struct RecursiveBezier {
        int64 x1;
        int64 y1;
        int64 x2;
        int64 y2;
        int64 x3;
        int64 y3;
        int32 level;
    }

    struct RecursiveBezierArgs {
        int64 x12;
        int64 y12;
        int64 x23;
        int64 y23;
        int64 x123;
        int64 y123;
        int64 dx;
        int64 dy;
    }

    function recursiveBezier(
        CurveData memory self,
        RecursiveBezier memory f
    ) private pure {
        if (f.level > MathUtils.RecursionLimit) return;

        RecursiveBezierArgs memory a;

        a.x12 = Fix64.div(Fix64.add(f.x1, f.x2), Fix64.TWO);
        a.y12 = Fix64.div(Fix64.add(f.y1, f.y2), Fix64.TWO);
        a.x23 = Fix64.div(Fix64.add(f.x2, f.x3), Fix64.TWO);
        a.y23 = Fix64.div(Fix64.add(f.y2, f.y3), Fix64.TWO);
        a.x123 = Fix64.div(Fix64.add(a.x12, a.x23), Fix64.TWO);
        a.y123 = Fix64.div(Fix64.add(a.y12, a.y23), Fix64.TWO);

        a.dx = Fix64.sub(f.x3, f.x1);
        a.dy = Fix64.sub(f.y3, f.y1);

        int64 d = Fix64.abs(
            Fix64.sub(
                Fix64.mul(Fix64.sub(f.x2, f.x3), a.dy),
                Fix64.mul((Fix64.sub(f.y2, f.y3)), a.dx)
            )
        );

        int64 da;

        if (d > MathUtils.Epsilon) {
            if (
                Fix64.mul(d, d) <=
                Fix64.mul(
                    self.distanceToleranceSquare,
                    Fix64.mul(a.dx, a.dx) + Fix64.mul(a.dy, a.dy)
                )
            ) {
                if (0 < MathUtils.AngleTolerance) {
                    self.points[self.pointCount++] = Vector2(a.x123, a.y123);
                    return;
                }

                da = Fix64.abs(
                    Fix64.sub(
                        Trig256.atan2(
                            Fix64.sub(f.y3, f.y2),
                            Fix64.sub(f.x3, f.x2)
                        ),
                        Trig256.atan2(
                            Fix64.sub(f.y2, f.y1),
                            Fix64.sub(f.x2, f.x1)
                        )
                    )
                );

                if (da >= Fix64.PI) {
                    da = Fix64.sub(Fix64.TWO_PI, da);
                }

                if (da < 0) {
                    self.points[self.pointCount++] = Vector2(a.x123, a.y123);
                    return;
                }
            }
        } else {
            da = Fix64.add(Fix64.mul(a.dx, a.dx), Fix64.mul(a.dy, a.dy));

            if (da == 0) {
                d = MathUtils.calcSquareDistance(f.x1, f.y1, f.x2, f.y2);
            } else {
                d = Fix64.add(
                    Fix64.mul(Fix64.sub(f.x2, f.x1), a.dx),
                    Fix64.div(Fix64.mul(Fix64.sub(f.y2, f.y1), a.dy), da)
                );

                if (d > 0 && d < Fix64.ONE) {
                    return;
                }

                if (d <= 0) {
                    d = MathUtils.calcSquareDistance(f.x2, f.y2, f.x1, f.y1);
                } else if (d > Fix64.ONE) // *** was d >= 1f
                {
                    d = MathUtils.calcSquareDistance(f.x2, f.y2, f.x3, f.y3);
                } else {
                    d = MathUtils.calcSquareDistance(
                        f.x2,
                        f.y2,
                        Fix64.add(f.x1, Fix64.mul(d, a.dx)),
                        Fix64.add(f.y1, Fix64.mul(d, a.dy))
                    );
                }
            }

            if (d < self.distanceToleranceSquare) {
                self.points[self.pointCount++] = Vector2(f.x2, f.y2);
                return;
            }
        }

        recursiveBezier(
            self,
            RecursiveBezier(
                f.x1,
                f.y1,
                a.x12,
                a.y12,
                a.x123,
                a.y123,
                f.level + 1
            )
        );
        recursiveBezier(
            self,
            RecursiveBezier(
                a.x123,
                a.y123,
                a.x23,
                a.y23,
                f.x3,
                f.y3,
                f.level + 1
            )
        );
    }
}
