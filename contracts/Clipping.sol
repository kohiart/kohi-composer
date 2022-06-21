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
import "./RectangleInt.sol";
import "./SubpixelScale.sol";
import "./ClippingData.sol";
import "./CellData.sol";
import "./CellRasterizer.sol";
import "./Graphics2D.sol";
import "./DrawContext.sol";

library Clipping {
    // function noClippingBox(ClippingData memory clippingData) internal pure {
    //     clippingData.clipPoly = new Vector2[](0);
    // }

    function setClippingBox(
        ClippingData memory clippingData,
        // ,int32 left,
        // int32 top,
        // int32 right,
        // int32 bottom,
        Matrix memory transform //,int32 height
    ) internal pure {
        /*
        Vector2 memory tl = MatrixMethods.transform(
            transform,
            Vector2(left * Fix64.ONE, top * Fix64.ONE)
        );
        Vector2 memory tr = MatrixMethods.transform(
            transform,
            Vector2(right * Fix64.ONE, top * Fix64.ONE)
        );
        Vector2 memory br = MatrixMethods.transform(
            transform,
            Vector2(right * Fix64.ONE, bottom * Fix64.ONE)
        );
        Vector2 memory bl = MatrixMethods.transform(
            transform,
            Vector2(left * Fix64.ONE, bottom * Fix64.ONE)
        );
        */

        clippingData.clipTransform = transform;

        // clippingData.clipPoly = new Vector2[](4);
        // clippingData.clipPoly[0] = Vector2(
        //     tl.x,
        //     Fix64.sub(height * Fix64.ONE, tl.y)
        // );
        // clippingData.clipPoly[1] = Vector2(
        //     tr.x,
        //     Fix64.sub(height * Fix64.ONE, tr.y)
        // );
        // clippingData.clipPoly[2] = Vector2(
        //     br.x,
        //     Fix64.sub(height * Fix64.ONE, br.y)
        // );
        // clippingData.clipPoly[3] = Vector2(
        //     bl.x,
        //     Fix64.sub(height * Fix64.ONE, bl.y)
        // );
    }

    function moveToClip(
        int32 x1,
        int32 y1,
        ClippingData memory clippingData
    ) internal pure {
        clippingData.x1 = x1;
        clippingData.y1 = y1;
        if (clippingData.clipping) {
            clippingData.f1 = clippingFlags(x1, y1, clippingData.clipBox);
        }
    }

    function lineToClip(
        Graphics2D memory g,
        DrawContext memory f,
        int32 x2,
        int32 y2
    ) internal pure {
        if (g.clippingData.clipping) {
            int32 f2 = clippingFlags(x2, y2, g.clippingData.clipBox);

            if (
                (g.clippingData.f1 & 10) == (f2 & 10) &&
                (g.clippingData.f1 & 10) != 0
            ) {
                g.clippingData.x1 = x2;
                g.clippingData.y1 = y2;
                g.clippingData.f1 = f2;
                return;
            }

            int32 x1 = g.clippingData.x1;
            int32 y1 = g.clippingData.y1;
            int32 f1 = g.clippingData.f1;
            int32 y3;
            int32 y4;
            int32 f3;
            int32 f4;

            if ((((f1 & 5) << 1) | (f2 & 5)) == 0) {
                setLineClipY(f.lineClipY, x1, y1, x2, y2, f1, f2);
                lineClipY(g, f);
            } else if ((((f1 & 5) << 1) | (f2 & 5)) == 1) {
                y3 =
                    y1 +
                    mulDiv(
                        (g.clippingData.clipBox.right - x1) * Fix64.ONE,
                        (y2 - y1) * Fix64.ONE,
                        (x2 - x1) * Fix64.ONE
                    );
                f3 = clippingFlagsY(y3, g.clippingData.clipBox);

                setLineClipY(
                    f.lineClipY,
                    x1,
                    y1,
                    g.clippingData.clipBox.right,
                    y3,
                    f1,
                    f3
                );
                lineClipY(g, f);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.right,
                    y3,
                    g.clippingData.clipBox.right,
                    y2,
                    f3,
                    f2
                );
                lineClipY(g, f);
            } else if ((((f1 & 5) << 1) | (f2 & 5)) == 2) {
                y3 =
                    y1 +
                    mulDiv(
                        (g.clippingData.clipBox.right - x1) * Fix64.ONE,
                        (y2 - y1) * Fix64.ONE,
                        (x2 - x1) * Fix64.ONE
                    );
                f3 = clippingFlagsY(y3, g.clippingData.clipBox);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.right,
                    y1,
                    g.clippingData.clipBox.right,
                    y3,
                    f1,
                    f3
                );
                lineClipY(g, f);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.right,
                    y3,
                    x2,
                    y2,
                    f3,
                    f2
                );
                lineClipY(g, f);
            } else if ((((f1 & 5) << 1) | (f2 & 5)) == 3) {
                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.right,
                    y1,
                    g.clippingData.clipBox.right,
                    y2,
                    f1,
                    f2
                );
                lineClipY(g, f);
            } else if ((((f1 & 5) << 1) | (f2 & 5)) == 4) {
                y3 =
                    y1 +
                    mulDiv(
                        (g.clippingData.clipBox.left - x1) * Fix64.ONE,
                        (y2 - y1) * Fix64.ONE,
                        (x2 - x1) * Fix64.ONE
                    );
                f3 = clippingFlagsY(y3, g.clippingData.clipBox);

                setLineClipY(
                    f.lineClipY,
                    x1,
                    y1,
                    g.clippingData.clipBox.left,
                    y3,
                    f1,
                    f3
                );
                lineClipY(g, f);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.left,
                    y3,
                    g.clippingData.clipBox.left,
                    y2,
                    f3,
                    f2
                );
                lineClipY(g, f);
            } else if ((((f1 & 5) << 1) | (f2 & 5)) == 6) {
                y3 =
                    y1 +
                    mulDiv(
                        (g.clippingData.clipBox.right - x1) * Fix64.ONE,
                        (y2 - y1) * Fix64.ONE,
                        (x2 - x1) * Fix64.ONE
                    );

                y4 =
                    y1 +
                    mulDiv(
                        (g.clippingData.clipBox.left - x1) * Fix64.ONE,
                        (y2 - y1) * Fix64.ONE,
                        (x2 - x1) * Fix64.ONE
                    );

                f3 = clippingFlagsY(y3, g.clippingData.clipBox);
                f4 = clippingFlagsY(y4, g.clippingData.clipBox);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.right,
                    y1,
                    g.clippingData.clipBox.right,
                    y3,
                    f1,
                    f3
                );
                lineClipY(g, f);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.right,
                    y3,
                    g.clippingData.clipBox.left,
                    y4,
                    f3,
                    f4
                );
                lineClipY(g, f);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.left,
                    y4,
                    g.clippingData.clipBox.left,
                    y2,
                    f4,
                    f2
                );
                lineClipY(g, f);
            } else if ((((f1 & 5) << 1) | (f2 & 5)) == 8) {
                y3 =
                    y1 +
                    mulDiv(
                        (g.clippingData.clipBox.left - x1) * Fix64.ONE,
                        (y2 - y1) * Fix64.ONE,
                        (x2 - x1) * Fix64.ONE
                    );

                f3 = clippingFlagsY(y3, g.clippingData.clipBox);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.left,
                    y1,
                    g.clippingData.clipBox.left,
                    y3,
                    f1,
                    f3
                );
                lineClipY(g, f);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.left,
                    y3,
                    x2,
                    y2,
                    f3,
                    f2
                );
                lineClipY(g, f);
            } else if ((((f1 & 5) << 1) | (f2 & 5)) == 9) {
                y3 =
                    y1 +
                    mulDiv(
                        (g.clippingData.clipBox.left - x1) * Fix64.ONE,
                        (y2 - y1) * Fix64.ONE,
                        (x2 - x1) * Fix64.ONE
                    );

                y4 =
                    y1 +
                    mulDiv(
                        (g.clippingData.clipBox.right - x1) * Fix64.ONE,
                        (y2 - y1) * Fix64.ONE,
                        (x2 - x1) * Fix64.ONE
                    );
                f3 = clippingFlagsY(y3, g.clippingData.clipBox);
                f4 = clippingFlagsY(y4, g.clippingData.clipBox);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.left,
                    y1,
                    g.clippingData.clipBox.left,
                    y3,
                    f1,
                    f3
                );
                lineClipY(g, f);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.left,
                    y3,
                    g.clippingData.clipBox.right,
                    y4,
                    f3,
                    f4
                );
                lineClipY(g, f);

                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.right,
                    y4,
                    g.clippingData.clipBox.right,
                    y2,
                    f4,
                    f2
                );
                lineClipY(g, f);
            } else if ((((f1 & 5) << 1) | (f2 & 5)) == 12) {
                setLineClipY(
                    f.lineClipY,
                    g.clippingData.clipBox.left,
                    y1,
                    g.clippingData.clipBox.left,
                    y2,
                    f1,
                    f2
                );
                lineClipY(g, f);
            }

            g.clippingData.f1 = f2;
        } else {
            f.line.x1 = g.clippingData.x1;
            f.line.y1 = g.clippingData.y1;
            f.line.x2 = x2;
            f.line.y2 = y2;
            CellRasterizer.line(f, f.line, g.cellData, g.ss);
        }

        g.clippingData.x1 = x2;
        g.clippingData.y1 = y2;
    }

    function setLineClipY(
        LineClipY memory l,
        int32 x1,
        int32 y1,
        int32 x2,
        int32 y2,
        int32 f1,
        int32 f2
    ) private pure {
        l.x1 = x1;
        l.y1 = y1;
        l.x2 = x2;
        l.y2 = y2;
        l.f1 = f1;
        l.f2 = f2;
    }

    function lineClipY(Graphics2D memory g, DrawContext memory f) private pure {
        f.lineClipY.f1 &= 10;
        f.lineClipY.f2 &= 10;
        if ((f.lineClipY.f1 | f.lineClipY.f2) == 0) {
            f.line.x1 = f.lineClipY.x1;
            f.line.y1 = f.lineClipY.y1;
            f.line.x2 = f.lineClipY.x2;
            f.line.y2 = f.lineClipY.y2;
            CellRasterizer.line(f, f.line, g.cellData, g.ss);
        } else {
            if (f.lineClipY.f1 == f.lineClipY.f2) return;

            int32 tx1 = f.lineClipY.x1;
            int32 ty1 = f.lineClipY.y1;
            int32 tx2 = f.lineClipY.x2;
            int32 ty2 = f.lineClipY.y2;

            if ((f.lineClipY.f1 & 8) != 0) {
                tx1 =
                    f.lineClipY.x1 +
                    mulDiv(
                        (g.clippingData.clipBox.bottom - f.lineClipY.y1) *
                            Fix64.ONE,
                        (f.lineClipY.x2 - f.lineClipY.x1) * Fix64.ONE,
                        (f.lineClipY.y2 - f.lineClipY.y1) * Fix64.ONE
                    );

                ty1 = g.clippingData.clipBox.bottom;
            }

            if ((f.lineClipY.f1 & 2) != 0) {
                tx1 =
                    f.lineClipY.x1 +
                    mulDiv(
                        (g.clippingData.clipBox.top - f.lineClipY.y1) *
                            Fix64.ONE,
                        (f.lineClipY.x2 - f.lineClipY.x1) * Fix64.ONE,
                        (f.lineClipY.y2 - f.lineClipY.y1) * Fix64.ONE
                    );

                ty1 = g.clippingData.clipBox.top;
            }

            if ((f.lineClipY.f2 & 8) != 0) {
                tx2 =
                    f.lineClipY.x1 +
                    mulDiv(
                        (g.clippingData.clipBox.bottom - f.lineClipY.y1) *
                            Fix64.ONE,
                        (f.lineClipY.x2 - f.lineClipY.x1) * Fix64.ONE,
                        (f.lineClipY.y2 - f.lineClipY.y1) * Fix64.ONE
                    );

                ty2 = g.clippingData.clipBox.bottom;
            }

            if ((f.lineClipY.f2 & 2) != 0) {
                tx2 =
                    f.lineClipY.x1 +
                    mulDiv(
                        (g.clippingData.clipBox.top - f.lineClipY.y1) *
                            Fix64.ONE,
                        (f.lineClipY.x2 - f.lineClipY.x1) * Fix64.ONE,
                        (f.lineClipY.y2 - f.lineClipY.y1) * Fix64.ONE
                    );

                ty2 = g.clippingData.clipBox.top;
            }

            f.line.x1 = tx1;
            f.line.y1 = ty1;
            f.line.x2 = tx2;
            f.line.y2 = ty2;
            CellRasterizer.line(f, f.line, g.cellData, g.ss);
        }
    }

    function clippingFlags(
        int32 x,
        int32 y,
        RectangleInt memory clipBox
    ) private pure returns (int32) {
        return
            (x > clipBox.right ? int32(1) : int32(0)) |
            (y > clipBox.top ? int32(1) << 1 : int32(0)) |
            (x < clipBox.left ? int32(1) << 2 : int32(0)) |
            (y < clipBox.bottom ? int32(1) << 3 : int32(0));
    }

    function clippingFlagsY(int32 y, RectangleInt memory clipBox)
        private
        pure
        returns (int32)
    {
        return
            ((y > clipBox.top ? int32(1) : int32(0)) << 1) |
            ((y < clipBox.bottom ? int32(1) : int32(0)) << 3);
    }

    function mulDiv(
        int64 a,
        int64 b,
        int64 c
    ) private pure returns (int32) {
        int64 div = Fix64.div(b, c);
        int64 muldiv = Fix64.mul(a, div);
        return (int32)(Fix64.round(muldiv) / Fix64.ONE);
    }
}
