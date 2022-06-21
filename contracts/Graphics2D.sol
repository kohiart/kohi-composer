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

import "./Vector2.sol";
import "./Fix64.sol";
import "./AntiAlias.sol";
import "./SubpixelScale.sol";
import "./RectangleInt.sol";
import "./Matrix.sol";
import "./ScanlineData.sol";
import "./ClippingData.sol";
import "./CellData.sol";
import "./Clipping.sol";
import "./PixelClipping.sol";
import "./ColorMath.sol";
import "./ApplyTransform.sol";
import "./ScanlineRasterizer.sol";
import "./DrawContext.sol";

struct Graphics2D {
    uint32 width;
    uint32 height;
    uint8[] buffer;
    AntiAlias aa;
    SubpixelScale ss;
    ScanlineData scanlineData;
    ClippingData clippingData;
    CellData cellData;
}

library Graphics2DMethods {
    // int32 public constant OrderB = 0;
    int32 public constant OrderG = 1;
    int32 public constant OrderR = 2;
    int32 public constant OrderA = 3;

    function create(uint32 width, uint32 height)
        external
        pure
        returns (Graphics2D memory g)
    {
        g.width = width;
        g.height = height;

        g.aa = AntiAliasMethods.create(8);
        g.ss = SubpixelScaleMethods.create(8);
        g.scanlineData = ScanlineDataMethods.create(width);
        g.clippingData = ClippingDataMethods.create(width, height, g.ss);
        g.cellData = CellDataMethods.create();
        g.buffer = new uint8[](width * 4 * height);
    }

    function clear(Graphics2D memory g, uint32 color) internal pure {
        int32 scale = int32(g.ss.scale);

        RectangleInt memory clippingRect = RectangleInt(
            g.clippingData.clipBox.left / scale,
            g.clippingData.clipBox.bottom / scale,
            g.clippingData.clipBox.right / scale,
            g.clippingData.clipBox.top / scale
        );

        for (int32 y = clippingRect.bottom; y < clippingRect.top; y++) {
            int32 bufferOffset = getBufferOffsetXy(g, clippingRect.left, y);

            for (int32 x = 0; x < clippingRect.right - clippingRect.left; x++) {
                g.buffer[
                    uint32(
                        bufferOffset /*+ OrderB */
                    )
                ] = uint8(color >> 0);
                g.buffer[uint32(bufferOffset + OrderG)] = uint8(color >> 8);
                g.buffer[uint32(bufferOffset + OrderR)] = uint8(color >> 16);
                g.buffer[uint32(bufferOffset + OrderA)] = uint8(color >> 24);
                bufferOffset += 4;
            }
        }
    }

    function renderWithTransform(
        Graphics2D memory g,
        DrawContext memory f,
        VertexData[] memory vertices,
        bool blend
    ) internal pure {
        if (!MatrixMethods.isIdentity(f.t)) {
            ApplyTransform.applyTransform(vertices, f.t, f.transformed);
        }

        addPath(g, f.transformed, f);
        ScanlineRasterizer.renderSolid(g, f, blend);

        if (!MatrixMethods.isIdentity(f.t)) {
            uint256 i;
            while (f.transformed[i].command != Command.Stop) {
                f.transformed[i].command = Command.Stop;
                f.transformed[i].position.x = 0;
                f.transformed[i].position.y = 0;
                i++;
            }
        }
    }

    function render(
        Graphics2D memory g,
        DrawContext memory f,
        VertexData[] memory vertices,
        bool blend
    ) internal pure {
        addPath(g, vertices, f);
        ScanlineRasterizer.renderSolid(g, f, blend);
    }

    function getBufferOffsetY(Graphics2D memory g, int32 y)
        internal
        pure
        returns (int32)
    {
        return y * int32(g.width) * 4;
    }

    function getBufferOffsetXy(
        Graphics2D memory g,
        int32 x,
        int32 y
    ) internal pure returns (int32) {
        if (x < 0 || x >= int32(g.width) || y < 0 || y >= int32(g.height))
            return -1;
        return y * int32(g.width) * 4 + x * 4;
    }

    function copyPixels(
        uint8[] memory buffer,
        int32 bufferOffset,
        uint32 sourceColor,
        int32 count
    ) internal pure //, PixelClipping memory clipping
    {
        int32 i = 0;
        do {
            /*
            if (
                clipping.area.length > 0 &&
                !PixelClippingMethods.isPointInPolygon(
                    clipping,
                    clipping.x + i,
                    clipping.y
                )
            ) {
                i++;
                bufferOffset += 4;
                continue;
            }
            */

            buffer[uint32(bufferOffset + OrderR)] = uint8(sourceColor >> 16);
            buffer[uint32(bufferOffset + OrderG)] = uint8(sourceColor >> 8);
            buffer[
                uint32(
                    bufferOffset /*+ OrderB */
                )
            ] = uint8(sourceColor >> 0);
            buffer[uint32(bufferOffset + OrderA)] = uint8(sourceColor >> 24);
            bufferOffset += 4;
            i++;
        } while (--count != 0);
    }

    function blendPixel(
        uint8[] memory buffer,
        int32 bufferOffset,
        uint32 sourceColor
    ) internal pure //, PixelClipping memory clipping
    {
        if (bufferOffset == -1) return;

        /*
        if (
            clipping.area.length > 0 &&
            !PixelClippingMethods.isPointInPolygon(
                clipping,
                clipping.x,
                clipping.y
            )
        ) {
            return;
        }
        */

        {
            uint8 sr = uint8(sourceColor >> 16);
            uint8 sg = uint8(sourceColor >> 8);
            uint8 sb = uint8(sourceColor >> 0);
            uint8 sa = uint8(sourceColor >> 24);

            unchecked {
                if (sourceColor >> 24 == 255) {
                    buffer[uint32(bufferOffset + OrderR)] = sr;
                    buffer[uint32(bufferOffset + OrderG)] = sg;
                    buffer[
                        uint32(
                            bufferOffset /*+ OrderB */
                        )
                    ] = sb;
                    buffer[uint32(bufferOffset + OrderA)] = sa;
                } else {
                    uint8 r = buffer[uint32(bufferOffset + OrderR)];
                    uint8 g = buffer[uint32(bufferOffset + OrderG)];
                    uint8 b = buffer[
                        uint32(
                            bufferOffset /*+ OrderB */
                        )
                    ];
                    uint8 a = buffer[uint32(bufferOffset + OrderA)];

                    buffer[uint32(bufferOffset + OrderR)] = uint8(
                        int8(
                            int32(
                                (((int32(uint32(sr)) - int32(uint32(r))) *
                                    int32(uint32(sa))) +
                                    (int32(uint32(r)) << 8)) >> 8
                            )
                        )
                    );
                    buffer[uint32(bufferOffset + OrderG)] = uint8(
                        int8(
                            int32(
                                (((int32(uint32(sg)) - int32(uint32(g))) *
                                    int32(uint32(sa))) +
                                    (int32(uint32(g)) << 8)) >> 8
                            )
                        )
                    );
                    buffer[
                        uint32(
                            bufferOffset /*+ OrderB */
                        )
                    ] = uint8(
                        int8(
                            int32(
                                (((int32(uint32(sb)) - int32(uint32(b))) *
                                    int32(uint32(sa))) +
                                    (int32(uint32(b)) << 8)) >> 8
                            )
                        )
                    );
                    buffer[uint32(bufferOffset + OrderA)] = uint8(
                        uint32(
                            (uint32(sa)) + a - (((uint32(sa)) * a + 255) >> 8)
                        )
                    );
                }
            }
        }
    }

    function addPath(
        Graphics2D memory g,
        VertexData[] memory vertices,
        DrawContext memory f
    ) private pure {
        if (g.cellData.sorted) {
            CellRasterizer.resetCells(g.cellData);
            g.scanlineData.status = ScanlineStatus.Initial;
        }

        for (uint32 i = 0; i < vertices.length; i++) {
            if (vertices[i].command == Command.Stop) break;
            if (vertices[i].command == Command.MoveTo) {
                g.scanlineData.startX = ClippingDataMethods.upscale(
                    vertices[i].position.x,
                    g.ss
                );
                g.scanlineData.startY = ClippingDataMethods.upscale(
                    vertices[i].position.y,
                    g.ss
                );
                Clipping.moveToClip(
                    g.scanlineData.startX,
                    g.scanlineData.startY,
                    g.clippingData
                );
                g.scanlineData.status = ScanlineStatus.MoveTo;
            } else {
                if (
                    vertices[i].command != Command.Stop &&
                    vertices[i].command != Command.EndPoly
                ) {
                    Clipping.lineToClip(
                        g,
                        f,
                        ClippingDataMethods.upscale(
                            vertices[i].position.x,
                            g.ss
                        ),
                        ClippingDataMethods.upscale(
                            vertices[i].position.y,
                            g.ss
                        )
                    );
                    g.scanlineData.status = ScanlineStatus.LineTo;
                } else {
                    if (vertices[i].command == Command.EndPoly)
                        closePolygon(g, f);
                }
            }
        }
    }

    function closePolygon(Graphics2D memory g, DrawContext memory f)
        internal
        pure
    {
        if (g.scanlineData.status != ScanlineStatus.LineTo) {
            return;
        }
        Clipping.lineToClip(g, f, g.scanlineData.startX, g.scanlineData.startY);
        g.scanlineData.status = ScanlineStatus.Closed;
    }
}
