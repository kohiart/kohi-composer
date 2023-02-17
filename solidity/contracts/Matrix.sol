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
import "./Vector2.sol";

struct Matrix {
    int64 sx;
    int64 shy;
    int64 shx;
    int64 sy;
    int64 tlx;
    int64 tly;
}

library MatrixMethods {
    function newIdentity() internal pure returns (Matrix memory value) {
        value.sx = Fix64.ONE;
        value.shy = 0;
        value.shx = 0;
        value.sy = Fix64.ONE;
        value.tlx = 0;
        value.tly = 0;
    }

    function newRotation(int64 radians) internal pure returns (Matrix memory) {
        int64 v0 = Trig256.cos(radians);
        int64 v1 = Trig256.sin(radians);
        int64 v2 = -Trig256.sin(radians);
        int64 v3 = Trig256.cos(radians);

        return Matrix(v0, v1, v2, v3, 0, 0);
    }

    function newScale(int64 scale) internal pure returns (Matrix memory) {
        return Matrix(scale, 0, 0, scale, 0, 0);
    }

    function newScale(
        int64 scaleX,
        int64 scaleY
    ) internal pure returns (Matrix memory) {
        return Matrix(scaleX, 0, 0, scaleY, 0, 0);
    }

    function newTranslation(
        int64 x,
        int64 y
    ) internal pure returns (Matrix memory) {
        return Matrix(Fix64.ONE, 0, 0, Fix64.ONE, x, y);
    }

    function transform(
        Matrix memory self,
        int64 x,
        int64 y
    ) internal pure returns (int64, int64) {
        int64 tmp = x;
        x = Fix64.add(
            Fix64.mul(tmp, self.sx),
            Fix64.add(Fix64.mul(y, self.shx), self.tlx)
        );
        y = Fix64.add(
            Fix64.mul(tmp, self.shy),
            Fix64.add(Fix64.mul(y, self.sy), self.tly)
        );
        return (x, y);
    }

    function transform(
        Matrix memory self,
        Vector2 memory v
    ) internal pure returns (Vector2 memory result) {
        result = v;
        (result.x, result.y) = transform(self, result.x, result.y);
        return result;
    }

    function invert(Matrix memory self) internal pure {
        int64 d = Fix64.div(
            Fix64.ONE,
            Fix64.sub(
                Fix64.mul(self.sx, self.sy),
                Fix64.mul(self.shy, self.shx)
            )
        );

        self.sy = Fix64.mul(self.sx, d);
        self.shy = Fix64.mul(-self.shy, d);
        self.shx = Fix64.mul(-self.shx, d);

        self.tly = Fix64.sub(
            Fix64.mul(-self.tlx, self.shy),
            Fix64.mul(self.tly, self.sy)
        );
        self.sx = Fix64.mul(self.sy, d);
        self.tlx = Fix64.sub(
            Fix64.mul(-self.tlx, Fix64.mul(self.sy, d)),
            Fix64.mul(self.tly, self.shx)
        );
    }

    function isIdentity(Matrix memory self) internal pure returns (bool) {
        return
            isEqual(self.sx, Fix64.ONE, MathUtils.Epsilon) &&
            isEqual(self.shy, 0, MathUtils.Epsilon) &&
            isEqual(self.shx, 0, MathUtils.Epsilon) &&
            isEqual(self.sy, Fix64.ONE, MathUtils.Epsilon) &&
            isEqual(self.tlx, 0, MathUtils.Epsilon) &&
            isEqual(self.tly, 0, MathUtils.Epsilon);
    }

    function isEqual(
        int64 v1,
        int64 v2,
        int64 epsilon
    ) internal pure returns (bool) {
        return Fix64.abs(Fix64.sub(v1, v2)) <= epsilon;
    }

    function mul(
        Matrix memory self,
        Matrix memory other
    ) internal pure returns (Matrix memory) {
        int64 t0 = Fix64.add(
            Fix64.mul(self.sx, other.sx),
            Fix64.mul(self.shy, other.shx)
        );
        int64 t1 = Fix64.add(
            Fix64.mul(self.shx, other.sx),
            Fix64.mul(self.sy, other.shx)
        );
        int64 t2 = Fix64.add(
            Fix64.mul(self.tlx, other.sx),
            Fix64.add(Fix64.mul(self.tly, other.shx), other.tlx)
        );
        int64 t3 = Fix64.add(
            Fix64.mul(self.sx, other.shy),
            Fix64.mul(self.shy, other.sy)
        );
        int64 t4 = Fix64.add(
            Fix64.mul(self.shx, other.shy),
            Fix64.mul(self.sy, other.sy)
        );
        int64 t5 = Fix64.add(
            Fix64.mul(self.tlx, other.shy),
            Fix64.add(Fix64.mul(self.tly, other.sy), other.tly)
        );

        self.shy = t3;
        self.sy = t4;
        self.tly = t5;
        self.sx = t0;
        self.shx = t1;
        self.tlx = t2;

        return self;
    }
}
