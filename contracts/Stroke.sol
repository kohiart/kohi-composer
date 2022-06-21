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
import "./VertexDistance.sol";
import "./VertexStatus.sol";
import "./StrokeStatus.sol";
import "./LineCap.sol";
import "./LineJoin.sol";
import "./Command.sol";
import "./MathUtils.sol";

import "./Errors.sol";

import "@openzeppelin/contracts/utils/Strings.sol";

struct Stroke {
    int64 startX;
    int64 startY;
    int64 width;
    int64 widthAbs;
    int64 widthEps;
    int64 widthSign;
    int32 srcVertex;
    int32 outVertexCount;
    int32 outVerticesCount;
    int32 distanceCount;
    bool closed;
    Vector2[] outVertices;
    VertexDistance[] distances;
    VertexData[] vertexSource;
    VertexStatus vertexStatus;
    StrokeStatus status;
    StrokeStatus previousStatus;
    LineCap lineCap;
    LineJoin lineJoin;
    Command lastCommand;
}

library StrokeMethods {
    function create(
        VertexData[] memory v,
        int64 width,
        uint32 maxDistanceCount,
        uint32 maxVertexCount
    ) external pure returns (Stroke memory stroke) {
        stroke.vertexSource = v;
        stroke.vertexStatus = VertexStatus.Initial;

        stroke.distances = new VertexDistance[](maxDistanceCount);
        stroke.outVertices = new Vector2[](maxVertexCount);
        stroke.status = StrokeStatus.Initial;

        stroke.lineCap = LineCap.Butt;
        stroke.lineJoin = LineJoin.Miter;

        stroke.width = Fix64.mul(
            width,
            2147483648 /* 0.5 */
        );
        if (stroke.width < 0) {
            stroke.widthAbs = -stroke.width;
            stroke.widthSign = -Fix64.ONE;
        } else {
            stroke.widthAbs = stroke.width;
            stroke.widthSign = Fix64.ONE;
        }
        stroke.widthEps = Fix64.div(
            stroke.width,
            4398046511104 /* 1024 */
        );
    }

    function vertices(Stroke memory self)
        external
        pure
        returns (VertexData[] memory results)
    {
        self.vertexStatus = VertexStatus.Initial;

        uint32 count = 0;
        {
            Command command;
            uint32 i = 0;
            do {
                (command, i, , ) = vertex(self, i, self.vertexSource);
                count++;
            } while (command != Command.Stop);
        }

        self.vertexStatus = VertexStatus.Initial;

        results = new VertexData[](count);
        {
            Command command;
            uint32 i = 0;
            count = 0;
            do {
                int64 x;
                int64 y;
                (command, i, x, y) = vertex(self, i, self.vertexSource);
                results[count++] = VertexData(command, Vector2(x, y));
            } while (command != Command.Stop);
        }

        return results;
    }

    function vertex(
        Stroke memory self,
        uint32 i,
        VertexData[] memory v
    )
        private
        pure
        returns (
            Command,
            uint32,
            int64,
            int64
        )
    {
        int64 x = 0;
        int64 y = 0;

        Command command = Command.Stop;
        bool done = false;

        while (!done) {
            VertexData memory c;

            if (self.vertexStatus == VertexStatus.Initial) {
                c = v[i++];
                self.lastCommand = c.command;
                self.startX = c.position.x;
                self.startY = c.position.y;
                self.vertexStatus = VertexStatus.Accumulate;
            } else if (self.vertexStatus == VertexStatus.Accumulate) {
                if (self.lastCommand == Command.Stop)
                    return (Command.Stop, i, x, y);

                clear(self);
                addVertex(self, self.startX, self.startY, Command.MoveTo);

                for (;;) {
                    c = v[i++];

                    self.lastCommand = c.command;
                    x = c.position.x;
                    y = c.position.y;

                    command = c.command;

                    if (command != Command.Stop && command != Command.EndPoly) {
                        self.lastCommand = command;
                        if (command == Command.MoveTo) {
                            self.startX = x;
                            self.startY = y;
                            break;
                        }

                        addVertex(self, x, y, command);
                    } else {
                        if (command == Command.Stop) {
                            self.lastCommand = Command.Stop;
                            break;
                        }

                        addVertex(self, x, y, command);
                        break;
                    }
                }

                rewind(self);
                self.vertexStatus = VertexStatus.Generate;
            } else if (self.vertexStatus == VertexStatus.Generate) {
                (command, x, y) = strokeVertex(self);

                if (command == Command.Stop) {
                    self.vertexStatus = VertexStatus.Accumulate;
                } else {
                    done = true;
                }
            } else {
                revert ArgumentOutOfRange();
            }
        }

        return (command, i, x, y);
    }

    function addVertex(
        Stroke memory self,
        int64 x,
        int64 y,
        Command command
    ) private pure {
        self.status = StrokeStatus.Initial;
        if (command == Command.MoveTo) {
            if (self.distanceCount != 0) self.distanceCount--;
            add(self, VertexDistance(x, y, 0));
        } else {
            if (command != Command.Stop && command != Command.EndPoly)
                add(self, VertexDistance(x, y, 0));
            else self.closed = command == Command.EndPoly;
        }
    }

    function strokeVertex(Stroke memory self)
        private
        pure
        returns (
            Command,
            int64 x,
            int64 y
        )
    {
        x = 0;
        y = 0;

        Command command = Command.LineTo;
        while (command != Command.Stop) {
            if (self.status == StrokeStatus.Initial) {
                rewind(self);
            } else if (self.status == StrokeStatus.Ready) {
                if (
                    self.distanceCount < 2 + (self.closed ? int8(1) : int8(0))
                ) {
                    command = Command.Stop;
                } else {
                    self.status = self.closed
                        ? StrokeStatus.Outline1
                        : StrokeStatus.Cap1;
                    command = Command.MoveTo;
                    self.srcVertex = 0;
                    self.outVertexCount = 0;
                }
            } else if (self.status == StrokeStatus.Cap1) {
                calcCap(
                    self,
                    self.distances[0],
                    self.distances[1],
                    self.distances[0].distance
                );

                self.srcVertex = 1;
                self.previousStatus = StrokeStatus.Outline1;
                self.status = StrokeStatus.OutVertices;
                self.outVertexCount = 0;
            } else if (self.status == StrokeStatus.Cap2) {
                calcCap(
                    self,
                    self.distances[uint32(self.distanceCount - 1)],
                    self.distances[uint32(self.distanceCount - 2)],
                    self.distances[uint32(self.distanceCount - 2)].distance
                );

                self.previousStatus = StrokeStatus.Outline2;
                self.status = StrokeStatus.OutVertices;
                self.outVertexCount = 0;
            } else if (self.status == StrokeStatus.Outline1) {
                bool join = true;
                if (self.closed) {
                    if (self.srcVertex >= self.distanceCount) {
                        self.previousStatus = StrokeStatus.CloseFirst;
                        self.status = StrokeStatus.EndPoly1;
                        join = false;
                    }
                } else {
                    if (self.srcVertex >= self.distanceCount - 1) {
                        self.status = StrokeStatus.Cap2;
                        join = false;
                    }
                }

                if (join) {
                    calcJoin(
                        self,
                        previous(self, self.srcVertex),
                        current(self, self.srcVertex),
                        next(self, self.srcVertex),
                        previous(self, self.srcVertex).distance,
                        current(self, self.srcVertex).distance
                    );

                    ++self.srcVertex;
                    self.previousStatus = self.status;
                    self.status = StrokeStatus.OutVertices;
                    self.outVertexCount = 0;
                }
            } else if (self.status == StrokeStatus.CloseFirst) {
                self.status = StrokeStatus.Outline2;
                command = Command.MoveTo;
            } else if (self.status == StrokeStatus.Outline2) {
                bool join = true;
                if (self.srcVertex <= (!self.closed ? int8(1) : int8(0))) {
                    self.status = StrokeStatus.EndPoly2;
                    self.previousStatus = StrokeStatus.Stop;
                    join = false;
                }

                if (join) {
                    --self.srcVertex;

                    calcJoin(
                        self,
                        next(self, self.srcVertex),
                        current(self, self.srcVertex),
                        previous(self, self.srcVertex),
                        current(self, self.srcVertex).distance,
                        previous(self, self.srcVertex).distance
                    );

                    self.previousStatus = self.status;
                    self.status = StrokeStatus.OutVertices;
                    self.outVertexCount = 0;
                }
            } else if (self.status == StrokeStatus.OutVertices) {
                if (self.outVertexCount >= self.outVerticesCount) {
                    self.status = self.previousStatus;
                } else {
                    Vector2 memory c = self.outVertices[
                        uint32(self.outVertexCount++)
                    ];
                    x = c.x;
                    y = c.y;
                    return (command, c.x, y);
                }
            } else if (self.status == StrokeStatus.EndPoly1) {
                self.status = self.previousStatus;
                return (Command.EndPoly, x, y);
            } else if (self.status == StrokeStatus.EndPoly2) {
                self.status = self.previousStatus;
                return (Command.EndPoly, x, y);
            } else if (self.status == StrokeStatus.Stop) {
                command = Command.Stop;
            } else {
                revert ArgumentOutOfRange();
            }
        }

        return (command, x, y);
    }

    function rewind(Stroke memory self) private pure {
        if (self.status == StrokeStatus.Initial) {
            while (self.distanceCount > 1) {
                if (
                    VertexDistanceMethods.isEqual(
                        self.distances[uint32(self.distanceCount - 2)],
                        self.distances[uint32(self.distanceCount - 1)]
                    )
                ) break;
                VertexDistance memory t = self.distances[
                    uint32(self.distanceCount - 1)
                ];
                if (self.distanceCount != 0) self.distanceCount--;
                if (self.distanceCount != 0) self.distanceCount--;
                add(self, t);
            }

            if (self.closed)
                while (self.distanceCount > 1) {
                    if (
                        VertexDistanceMethods.isEqual(
                            self.distances[uint32(self.distanceCount - 1)],
                            self.distances[0]
                        )
                    ) break;
                    if (self.distanceCount != 0) self.distanceCount--;
                }

            if (self.distanceCount < 3) self.closed = false;
        }

        self.status = StrokeStatus.Ready;
        self.srcVertex = 0;
        self.outVertexCount = 0;
    }

    function add(Stroke memory self, VertexDistance memory value) private pure {
        if (self.distanceCount > 1)
            if (
                !VertexDistanceMethods.isEqual(
                    self.distances[uint32(self.distanceCount - 2)],
                    self.distances[uint32(self.distanceCount - 1)]
                )
            )
                if (self.distanceCount != 0) self.distanceCount--;
        self.distances[uint32(self.distanceCount++)] = value;
    }

    struct CalcCapArgs {
        uint32 vertexCount;
        int64 dx1;
        int64 dy1;
        int64 dx2;
        int64 dy2;
        int64 da;
        int64 a1;
        int32 i;
        int32 n;
    }

    function calcCap(
        Stroke memory self,
        VertexDistance memory v0,
        VertexDistance memory v1,
        int64 len
    ) private pure {
        self.outVerticesCount = 0;

        CalcCapArgs memory a;

        a.dx1 = Fix64.div(Fix64.sub(v1.y, v0.y), len);
        a.dy1 = Fix64.div(Fix64.sub(v1.x, v0.x), len);
        a.dx2 = 0;
        a.dy2 = 0;

        a.dx1 = Fix64.mul(a.dx1, self.width);
        a.dy1 = Fix64.mul(a.dy1, self.width);

        if (self.lineCap != LineCap.Round) {
            if (self.lineCap == LineCap.Square) {
                a.dx2 = a.dy1 * self.widthSign;
                a.dy2 = a.dx1 * self.widthSign;
            }

            self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                Fix64.sub(v0.x, Fix64.sub(a.dx1, a.dx2)),
                Fix64.add(v0.y, Fix64.sub(a.dy1, a.dy2))
            );
            self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                Fix64.add(v0.x, Fix64.sub(a.dx1, a.dx2)),
                Fix64.sub(v0.y, Fix64.sub(a.dy1, a.dy2))
            );
        } else {
            a.da = Fix64.mul(
                Trig256.acos(
                    Fix64.div(
                        self.widthAbs,
                        Fix64.add(
                            self.widthAbs,
                            Fix64.div(
                                536870912, /* 0.125 */
                                Fix64.ONE
                            )
                        )
                    )
                ),
                Fix64.TWO
            );

            a.n = (int32)(Fix64.div(Fix64.PI, a.da) / Fix64.ONE);

            a.da = Fix64.div(Fix64.PI, (a.n + 1) * Fix64.ONE);

            self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                Fix64.sub(v0.x, a.dx1),
                Fix64.add(v0.y, a.dy1)
            );

            if (self.widthSign > 0) {
                a.a1 = Trig256.atan2(a.dy1, -a.dx1);
                a.a1 = Fix64.add(a.a1, a.da);
                for (a.i = 0; a.i < a.n; a.i++) {
                    self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                        Fix64.add(
                            v0.x,
                            Fix64.mul(Trig256.cos(a.a1), self.width)
                        ),
                        Fix64.add(
                            v0.y,
                            Fix64.mul(Trig256.sin(a.a1), self.width)
                        )
                    );
                    a.a1 += a.da;
                }
            } else {
                a.a1 = Trig256.atan2(-a.dy1, a.dx1);
                a.a1 = Fix64.sub(a.a1, a.da);
                for (a.i = 0; a.i < a.n; a.i++) {
                    self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                        Fix64.add(
                            v0.x,
                            Fix64.mul(Trig256.cos(a.a1), self.width)
                        ),
                        Fix64.add(
                            v0.y,
                            Fix64.mul(Trig256.sin(a.a1), self.width)
                        )
                    );

                    a.a1 = Fix64.sub(a.a1, a.da);
                }
            }

            self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                Fix64.add(v0.x, a.dx1),
                Fix64.sub(v0.y, a.dy1)
            );
        }
    }

    struct CalcJoinArgs {
        int64 dx1;
        int64 dy1;
        int64 dx2;
        int64 dy2;
        int64 cp;
        int64 dx;
        int64 dy;
        int64 bevelDistance;
        bool intersects;
    }

    function calcJoin(
        Stroke memory self,
        VertexDistance memory v0,
        VertexDistance memory v1,
        VertexDistance memory v2,
        int64 len1,
        int64 len2
    ) private pure {
        self.outVerticesCount = 0;

        CalcJoinArgs memory a;

        a.dx1 = Fix64.mul(self.width, Fix64.div(Fix64.sub(v1.y, v0.y), len1));
        a.dy1 = Fix64.mul(self.width, Fix64.div(Fix64.sub(v1.x, v0.x), len1));
        a.dx2 = Fix64.mul(self.width, Fix64.div(Fix64.sub(v2.y, v1.y), len2));
        a.dy2 = Fix64.mul(self.width, Fix64.div(Fix64.sub(v2.x, v1.x), len2));
        a.cp = MathUtils.crossProduct(v0.x, v0.y, v1.x, v1.y, v2.x, v2.y);

        if (a.cp != 0 && a.cp > 0 == self.width > 0) {
            int64 limit = 0;
            if (self.widthAbs != 0) {
                limit = Fix64.div((len1 < len2 ? len1 : len2), self.widthAbs);
            }

            if (
                limit < 4337916928 /* 1.01 */
            ) {
                limit = 4337916928; /* 1.01 */
            }

            calcMiter(
                self,
                CalcMiter(
                    v0,
                    v1,
                    v2,
                    a.dx1,
                    a.dy1,
                    a.dx2,
                    a.dy2,
                    LineJoin.MiterRevert,
                    limit,
                    0
                )
            );
        } else {
            a.dx = Fix64.div(Fix64.add(a.dx1, a.dx2), Fix64.TWO);
            a.dy = Fix64.div(Fix64.add(a.dy1, a.dy2), Fix64.TWO);
            a.bevelDistance = Trig256.sqrt(
                Fix64.add(Fix64.mul(a.dx, a.dx), Fix64.mul(a.dy, a.dy))
            );

            if (
                self.lineJoin == LineJoin.Round ||
                self.lineJoin == LineJoin.Bevel
            ) {
                if (
                    Fix64.mul(
                        Fix64.ONE,
                        Fix64.sub(self.widthAbs, a.bevelDistance)
                    ) < self.widthEps
                ) {
                    (a.dx, a.dy, a.intersects) = MathUtils.calcIntersection(
                        MathUtils.CalcIntersection(
                            Fix64.add(v0.x, a.dx1),
                            Fix64.sub(v0.y, a.dy1),
                            Fix64.add(v1.x, a.dx1),
                            Fix64.sub(v1.y, a.dy1),
                            Fix64.add(v1.x, a.dx2),
                            Fix64.sub(v1.y, a.dy2),
                            Fix64.add(v2.x, a.dx2),
                            Fix64.sub(v2.y, a.dy2)
                        )
                    );

                    if (a.intersects) {
                        self.outVertices[
                            uint32(self.outVerticesCount++)
                        ] = Vector2(a.dx, a.dy);
                    } else {
                        self.outVertices[
                            uint32(self.outVerticesCount++)
                        ] = Vector2(
                            Fix64.add(v1.x, a.dx1),
                            Fix64.sub(v1.y, a.dy1)
                        );
                    }

                    return;
                }
            }

            if (
                self.lineJoin == LineJoin.Miter ||
                self.lineJoin == LineJoin.MiterRevert ||
                self.lineJoin == LineJoin.MiterRound
            ) {
                calcMiter(
                    self,
                    CalcMiter(
                        v0,
                        v1,
                        v2,
                        a.dx1,
                        a.dy1,
                        a.dx2,
                        a.dy2,
                        self.lineJoin,
                        17179869184, /* 4 */
                        a.bevelDistance
                    )
                );
            } else if (self.lineJoin == LineJoin.Round) {
                calcArc(
                    self,
                    CalcArc(v1.x, v1.y, a.dx1, -a.dy1, a.dx2, -a.dy2)
                );
            } else if (self.lineJoin == LineJoin.Bevel) {
                self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                    Fix64.add(v1.x, a.dx1),
                    Fix64.sub(v1.y, a.dy1)
                );
                self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                    Fix64.add(v1.x, a.dx2),
                    Fix64.sub(v1.y, a.dy2)
                );
            } else {
                revert ArgumentOutOfRange();
            }
        }
    }

    struct CalcArc {
        int64 x;
        int64 y;
        int64 dx1;
        int64 dy1;
        int64 dx2;
        int64 dy2;
    }

    function calcArc(Stroke memory self, CalcArc memory f) private pure {
        int64 a1 = Trig256.atan2(
            Fix64.mul(f.dy1, self.widthSign),
            Fix64.mul(f.dx1, self.widthSign)
        );

        int64 a2 = Trig256.atan2(
            Fix64.mul(f.dy2, self.widthSign),
            Fix64.mul(f.dx2, self.widthSign)
        );

        int32 n;

        int64 da = Fix64.mul(
            Trig256.acos(
                Fix64.div(
                    self.widthAbs,
                    Fix64.add(
                        self.widthAbs,
                        Fix64.div(
                            536870912, /* 0.125 */
                            Fix64.ONE
                        )
                    )
                )
            ),
            Fix64.TWO
        );

        self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
            Fix64.add(f.x, f.dx1),
            Fix64.add(f.y, f.dy1)
        );

        if (self.widthSign > 0) {
            if (a1 > a2) {
                a2 = Fix64.add(a2, Fix64.TWO_PI);
            }

            int64 t1 = Fix64.div(Fix64.sub(a2, a1), da);
            n = (int32)(t1 / Fix64.ONE);

            da = Fix64.div(Fix64.sub(a2, a1), (n + 1) * Fix64.ONE);
            a1 = Fix64.add(a1, da);

            for (int32 i = 0; i < n; i++) {
                int64 vx = Fix64.add(
                    f.x,
                    Fix64.mul(Trig256.cos(a1), self.width)
                );
                int64 vy = Fix64.add(
                    f.y,
                    Fix64.mul(Trig256.sin(a1), self.width)
                );
                self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                    vx,
                    vy
                );
                a1 = Fix64.add(a1, da);
            }
        } else {
            if (a1 < a2) {
                a2 = Fix64.sub(a2, Fix64.TWO_PI);
            }

            int64 t1 = Fix64.div(Fix64.sub(a1, a2), da);
            n = (int32)(t1 / Fix64.ONE);

            da = Fix64.div(Fix64.sub(a1, a2), (n + 1) * Fix64.ONE);
            a1 = Fix64.sub(a1, da);

            for (int32 i = 0; i < n; i++) {
                int64 vx = Fix64.add(
                    f.x,
                    Fix64.mul(Trig256.cos(a1), self.width)
                );
                int64 vy = Fix64.add(
                    f.y,
                    Fix64.mul(Trig256.sin(a1), self.width)
                );
                self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                    vx,
                    vy
                );
                a1 = Fix64.sub(a1, da);
            }
        }

        self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
            Fix64.add(f.x, f.dx2),
            Fix64.add(f.y, f.dy2)
        );
    }

    struct CalcMiter {
        VertexDistance v0;
        VertexDistance v1;
        VertexDistance v2;
        int64 dx1;
        int64 dy1;
        int64 dx2;
        int64 dy2;
        LineJoin lj;
        int64 miterLimit;
        int64 distanceBevel;
    }

    struct CalcMiterArgs {
        int64 di;
        int64 lim;
        bool miterLimitExceeded;
        bool intersectionFailed;
    }

    function calcMiter(Stroke memory self, CalcMiter memory f) private pure {
        CalcMiterArgs memory a;

        a.di = Fix64.ONE;
        a.lim = Fix64.mul(self.widthAbs, f.miterLimit);
        a.miterLimitExceeded = true;
        a.intersectionFailed = true;

        (int64 xi, int64 yi, bool intersects) = MathUtils.calcIntersection(
            MathUtils.CalcIntersection(
                Fix64.add(f.v0.x, f.dx1),
                Fix64.sub(f.v0.y, f.dy1),
                Fix64.add(f.v1.x, f.dx1),
                Fix64.sub(f.v1.y, f.dy1),
                Fix64.add(f.v1.x, f.dx2),
                Fix64.sub(f.v1.y, f.dy2),
                Fix64.add(f.v2.x, f.dx2),
                Fix64.sub(f.v2.y, f.dy2)
            )
        );

        if (intersects) {
            a.di = MathUtils.calcDistance(f.v1.x, f.v1.y, xi, yi);

            if (a.di <= a.lim) {
                self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                    xi,
                    yi
                );
                a.miterLimitExceeded = false;
            }

            a.intersectionFailed = false;
        } else {
            int64 x2 = Fix64.add(f.v1.x, f.dx1);
            int64 y2 = Fix64.sub(f.v1.y, f.dy1);

            if (
                MathUtils.crossProduct(f.v0.x, f.v0.y, f.v1.x, f.v1.y, x2, y2) <
                0 ==
                MathUtils.crossProduct(f.v1.x, f.v1.y, f.v2.x, f.v2.y, x2, y2) <
                0
            ) {
                self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                    Fix64.add(f.v1.x, f.dx1),
                    Fix64.sub(f.v1.y, f.dy1)
                );
                a.miterLimitExceeded = false;
            }
        }

        if (!a.miterLimitExceeded) return;

        {
            if (f.lj == LineJoin.MiterRevert) {
                self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                    Fix64.add(f.v1.x, f.dx1),
                    Fix64.sub(f.v1.y, f.dy1)
                );
                self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                    Fix64.add(f.v1.x, f.dx2),
                    Fix64.sub(f.v1.y, f.dy2)
                );
            } else if (f.lj == LineJoin.MiterRound) {
                calcArc(
                    self,
                    CalcArc(f.v1.x, f.v1.y, f.dx1, -f.dy1, f.dx2, -f.dy2)
                );
            } else if (f.lj == LineJoin.Miter) {} else if (
                f.lj == LineJoin.Round
            ) {} else if (f.lj == LineJoin.Bevel) {} else {
                if (a.intersectionFailed) {
                    f.miterLimit = Fix64.mul(f.miterLimit, self.widthSign);

                    self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                        Fix64.add(
                            f.v1.x,
                            Fix64.add(f.dx1, Fix64.mul(f.dy1, f.miterLimit))
                        ),
                        Fix64.sub(
                            f.v1.y,
                            Fix64.add(f.dy1, Fix64.mul(f.dx1, f.miterLimit))
                        )
                    );

                    self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                        Fix64.add(
                            f.v1.x,
                            Fix64.sub(f.dx2, Fix64.mul(f.dy2, f.miterLimit))
                        ),
                        Fix64.sub(
                            f.v1.y,
                            Fix64.sub(f.dy2, Fix64.mul(f.dx2, f.miterLimit))
                        )
                    );
                } else {
                    int64 x1 = Fix64.add(f.v1.x, f.dx1);
                    int64 y1 = Fix64.sub(f.v1.y, f.dy1);
                    int64 x2 = Fix64.add(f.v1.x, f.dx2);
                    int64 y2 = Fix64.sub(f.v1.y, f.dy2);

                    a.di = Fix64.div(
                        Fix64.sub(a.lim, f.distanceBevel),
                        Fix64.sub(a.di, f.distanceBevel)
                    );

                    self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                        Fix64.add(x1, Fix64.mul(Fix64.sub(xi, x1), a.di)),
                        Fix64.add(y1, Fix64.mul(Fix64.sub(yi, y1), a.di))
                    );

                    self.outVertices[uint32(self.outVerticesCount++)] = Vector2(
                        Fix64.add(x2, Fix64.mul(Fix64.sub(xi, x2), a.di)),
                        Fix64.add(y2, Fix64.mul(Fix64.sub(yi, y2), a.di))
                    );
                }
            }
        }
    }

    function previous(Stroke memory self, int32 i)
        private
        pure
        returns (VertexDistance memory)
    {
        return
            self.distances[
                uint32((i + self.distanceCount - 1) % self.distanceCount)
            ];
    }

    function current(Stroke memory self, int32 i)
        private
        pure
        returns (VertexDistance memory)
    {
        return self.distances[uint32(i)];
    }

    function next(Stroke memory self, int32 i)
        private
        pure
        returns (VertexDistance memory)
    {
        return self.distances[uint32((i + 1) % self.distanceCount)];
    }

    function clear(Stroke memory self) private pure {
        self.distanceCount = 0;
        self.closed = false;
        self.status = StrokeStatus.Initial;
    }
}
