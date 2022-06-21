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

contract Curve4 {
    struct CurveData {
        int64 angleTolerance;
        int64 cuspLimit;
        int64 distanceToleranceSquare;
        uint32 pointCount;
        Vector2[] points;
    }

    function create(
        int64 x1,
        int64 y1,
        int64 x2,
        int64 y2,
        int64 x3,
        int64 y3,
        int64 x4,
        int64 y4
    ) external pure returns (CurveData memory curve) {
        curve.points = new Vector2[](2000);

        curve.angleTolerance = 0;
        curve.cuspLimit = 0;

        curve.distanceToleranceSquare = 2147483648; /* 0.5 */
        curve.distanceToleranceSquare = Fix64.mul(
            curve.distanceToleranceSquare,
            curve.distanceToleranceSquare
        );

        bezier(curve, x1, y1, x2, y2, x3, y3, x4, y4);
    }

    function vertices(CurveData memory data)
        external
        pure
        returns (VertexData[] memory results)
    {
        results = new VertexData[](data.pointCount + 2);
        results[0] = VertexData(Command.MoveTo, data.points[0]);
        for (uint32 i = 1; i < data.pointCount; i++) {
            results[i] = VertexData(Command.LineTo, data.points[i]);
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
        int64 y3,
        int64 x4,
        int64 y4
    ) private pure {
        self.points[self.pointCount++] = Vector2(x1, y1);
        recursiveBezier(
            self,
            RecursiveBezier(x1, y1, x2, y2, x3, y3, x4, y4, 0)
        );
        self.points[self.pointCount++] = Vector2(x4, y4);
    }

    struct RecursiveBezier {
        int64 x1;
        int64 y1;
        int64 x2;
        int64 y2;
        int64 x3;
        int64 y3;
        int64 x4;
        int64 y4;
        int32 level;
    }

    struct RecursiveBezierArgs {
        int64 x12;
        int64 y12;
        int64 x23;
        int64 y23;
        int64 x34;
        int64 y34;
        int64 x123;
        int64 y123;
        int64 x234;
        int64 y234;
        int64 x1234;
        int64 y1234;
        int64 dx;
        int64 dy;
    }

    function recursiveBezier(CurveData memory self, RecursiveBezier memory f)
        private
        pure
    {
        if (f.level > MathUtils.RecursionLimit) return;

        RecursiveBezierArgs memory a;

        a.x12 = Fix64.div(Fix64.add(f.x1, f.x2), Fix64.TWO);
        a.y12 = Fix64.div(Fix64.add(f.y1, f.y2), Fix64.TWO);
        a.x23 = Fix64.div(Fix64.add(f.x2, f.x3), Fix64.TWO);
        a.y23 = Fix64.div(Fix64.add(f.y2, f.y3), Fix64.TWO);
        a.x34 = Fix64.div(Fix64.add(f.x3, f.x4), Fix64.TWO);
        a.y34 = Fix64.div(Fix64.add(f.y3, f.y4), Fix64.TWO);
        a.x123 = Fix64.div(Fix64.add(a.x12, a.x23), Fix64.TWO);
        a.y123 = Fix64.div(Fix64.add(a.y12, a.y23), Fix64.TWO);
        a.x234 = Fix64.div(Fix64.add(a.x23, a.x34), Fix64.TWO);
        a.y234 = Fix64.div(Fix64.add(a.y23, a.y34), Fix64.TWO);
        a.x1234 = Fix64.div(Fix64.add(a.x123, a.x234), Fix64.TWO);
        a.y1234 = Fix64.div(Fix64.add(a.y123, a.y234), Fix64.TWO);

        a.dx = Fix64.sub(f.x4, f.x1);
        a.dy = Fix64.sub(f.y4, f.y1);

        int64 d2 = Fix64.abs(
            Fix64.sub(
                Fix64.mul(Fix64.sub(f.x2, f.x4), a.dy),
                Fix64.mul(Fix64.sub(f.y2, f.y4), a.dx)
            )
        );
        int64 d3 = Fix64.abs(
            Fix64.sub(
                Fix64.mul(Fix64.sub(f.x3, f.x4), a.dy),
                Fix64.mul(Fix64.sub(f.y3, f.y4), a.dx)
            )
        );
        int64 da1;
        int64 da2;
        int64 k;

        uint8 switchCase = 0;
        if (d2 > MathUtils.Epsilon) switchCase = 2;
        if (d3 > MathUtils.Epsilon) switchCase++;

        if (switchCase == 0) {
            k = Fix64.add(Fix64.mul(a.dx, a.dx), Fix64.mul(a.dy, a.dy));
            if (k == 0) {
                d2 = MathUtils.calcSquareDistance(f.x1, f.y1, f.x2, f.y2);
                d3 = MathUtils.calcSquareDistance(f.x4, f.y4, f.x3, f.y3);
            } else {
                k = Fix64.div(Fix64.ONE, k);
                da1 = Fix64.sub(f.x2, f.x1);
                da2 = Fix64.sub(f.y2, f.y1);
                d2 = Fix64.mul(
                    k,
                    Fix64.add(Fix64.mul(da1, a.dx), Fix64.mul(da2, a.dy))
                );
                da1 = Fix64.sub(f.x3, f.x1);
                da2 = Fix64.sub(f.y3, f.y1);
                d3 = Fix64.mul(
                    k,
                    Fix64.add(Fix64.mul(da1, a.dx), Fix64.mul(da2, a.dy))
                );

                if (d2 > 0 && d2 < Fix64.ONE && d3 > 0 && d3 < Fix64.ONE) {
                    return;
                }

                if (d2 <= 0) {
                    d2 = MathUtils.calcSquareDistance(f.x2, f.y2, f.x1, f.y1);
                } else if (d2 >= Fix64.ONE) {
                    d2 = MathUtils.calcSquareDistance(f.x2, f.y2, f.x4, f.y4);
                } else {
                    d2 = MathUtils.calcSquareDistance(
                        f.x2,
                        f.y2,
                        Fix64.add(f.x1, Fix64.mul(d2, a.dx)),
                        Fix64.add(f.y1, Fix64.mul(d2, a.dy))
                    );
                }

                if (d3 <= 0) {
                    d3 = MathUtils.calcSquareDistance(f.x3, f.y3, f.x1, f.y1);
                } else if (d3 >= Fix64.ONE) {
                    d3 = MathUtils.calcSquareDistance(f.x3, f.y3, f.x4, f.y4);
                } else {
                    d3 = MathUtils.calcSquareDistance(
                        f.x3,
                        f.y3,
                        Fix64.add(f.x1, Fix64.mul(d3, a.dx)),
                        Fix64.add(f.y1, Fix64.mul(d3, a.dy))
                    );
                }
            }

            if (d2 > d3) {
                if (d2 < self.distanceToleranceSquare) {
                    self.points[self.pointCount++] = Vector2(f.x2, f.y2);
                    return;
                }
            } else {
                if (d3 < self.distanceToleranceSquare) {
                    self.points[self.pointCount++] = Vector2(f.x3, f.y3);
                    return;
                }
            }
        } else if (switchCase == 1) {
            if (
                Fix64.mul(d3, d3) <=
                Fix64.mul(
                    self.distanceToleranceSquare,
                    Fix64.add(Fix64.mul(a.dx, a.dx), Fix64.mul(a.dy, a.dy))
                )
            ) {
                if (self.angleTolerance < MathUtils.AngleTolerance) {
                    self.points[self.pointCount++] = Vector2(a.x23, a.y23);
                    return;
                }

                da1 = Fix64.abs(
                    Fix64.sub(
                        Trig256.atan2(
                            Fix64.sub(f.y4, f.y3),
                            Fix64.sub(f.x4, f.x3)
                        ),
                        Trig256.atan2(
                            Fix64.sub(f.y3, f.y2),
                            Fix64.sub(f.x3, f.x2)
                        )
                    )
                );

                if (da1 >= Fix64.PI) {
                    da1 = Fix64.sub(Fix64.TWO_PI, da1);
                }

                if (da1 < self.angleTolerance) {
                    self.points[self.pointCount++] = Vector2(f.x2, f.y2);
                    self.points[self.pointCount++] = Vector2(f.x3, f.y3);
                    return;
                }

                if (self.cuspLimit != 0)
                    if (da1 > self.cuspLimit) {
                        self.points[self.pointCount++] = Vector2(f.x3, f.y3);
                        return;
                    }
            }
        } else if (switchCase == 2) {
            if (
                Fix64.mul(d2, d2) <=
                Fix64.mul(
                    self.distanceToleranceSquare,
                    Fix64.add(Fix64.mul(a.dx, a.dx), Fix64.mul(a.dy, a.dy))
                )
            ) {
                if (self.angleTolerance < MathUtils.AngleTolerance) {
                    self.points[self.pointCount++] = Vector2(a.x23, a.y23);
                    return;
                }

                da1 = Fix64.abs(
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

                if (da1 >= Fix64.PI) {
                    da1 = Fix64.sub(Fix64.TWO_PI, da1);
                }

                if (da1 < self.angleTolerance) {
                    self.points[self.pointCount++] = Vector2(f.x2, f.y2);
                    self.points[self.pointCount++] = Vector2(f.x3, f.y3);
                    return;
                }

                if (self.cuspLimit != 0)
                    if (da1 > self.cuspLimit) {
                        self.points[self.pointCount++] = Vector2(f.x2, f.y2);
                        return;
                    }
            }
        } else if (switchCase == 3) {
            if (
                Fix64.mul(Fix64.add(d2, d3), Fix64.add(d2, d3)) <=
                Fix64.mul(
                    self.distanceToleranceSquare,
                    Fix64.add(Fix64.mul(a.dx, a.dx), Fix64.mul(a.dy, a.dy))
                )
            ) {
                if (self.angleTolerance < MathUtils.AngleTolerance) {
                    self.points[self.pointCount++] = Vector2(a.x23, a.y23);
                    return;
                }

                k = Trig256.atan2(Fix64.sub(f.y3, f.y2), Fix64.sub(f.x3, f.x2));
                da1 = Fix64.abs(
                    Trig256.atan2(Fix64.sub(f.y2, f.y1), Fix64.sub(f.x2, f.x1))
                );
                da2 = Fix64.abs(
                    Fix64.sub(
                        Trig256.atan2(
                            Fix64.sub(f.y4, f.y3),
                            Fix64.sub(f.x4, f.x3)
                        ),
                        k
                    )
                );

                if (da1 >= Fix64.PI) da1 = Fix64.sub(Fix64.TWO_PI, da1);
                if (da2 >= Fix64.PI) da2 = Fix64.sub(Fix64.TWO_PI, da2);

                if (da1 + da2 < self.angleTolerance) {
                    self.points[self.pointCount++] = Vector2(a.x23, a.y23);
                    return;
                }

                if (self.cuspLimit != 0) {
                    if (da1 > self.cuspLimit) {
                        self.points[self.pointCount++] = Vector2(f.x2, f.y2);
                        return;
                    }

                    if (da2 > self.cuspLimit) {
                        self.points[self.pointCount++] = Vector2(f.x3, f.y3);
                        return;
                    }
                }
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
                a.x1234,
                a.y1234,
                f.level + 1
            )
        );
        recursiveBezier(
            self,
            RecursiveBezier(
                a.x1234,
                a.y1234,
                a.x234,
                a.y234,
                a.x34,
                a.y34,
                f.x4,
                f.y4,
                f.level + 1
            )
        );
    }
}
