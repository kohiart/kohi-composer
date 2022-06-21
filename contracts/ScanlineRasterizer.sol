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

import "./SubpixelScale.sol";
import "./ScanlineData.sol";
import "./ClippingData.sol";
import "./CellData.sol";
import "./Graphics2D.sol";
import "./CellRasterizer.sol";
import "./ColorMath.sol";
import "./PixelClipping.sol";
import "./Clipping.sol";
import "./DrawContext.sol";

import "@openzeppelin/contracts/utils/Strings.sol";

library ScanlineRasterizer {
    function renderSolid(
        Graphics2D memory g,
        DrawContext memory f,
        bool blend
    ) internal pure {
        Graphics2DMethods.closePolygon(g, f);
        CellRasterizer.sortCells(g.cellData);
        if (g.cellData.used == 0) return;
        g.scanlineData.scanY = g.cellData.minY;

        resetScanline(g.scanlineData);

        while (sweepScanline(g, f.current)) {
            int32 y = g.scanlineData.y;
            int32 spanCount = g.scanlineData.spanIndex;

            g.scanlineData.current = 1;
            g.scanlineData.current++;
            f.scanlineSpan = g.scanlineData.spans[
                uint32(g.scanlineData.current - 1)
            ];

            for (;;) {
                int32 x = f.scanlineSpan.x;
                if (f.scanlineSpan.length > 0) {
                    f.blendSolidHorizontalSpan.x = x;
                    f.blendSolidHorizontalSpan.y = y;
                    f.blendSolidHorizontalSpan.len = f.scanlineSpan.length;
                    f.blendSolidHorizontalSpan.sourceColor = f.color;
                    f.blendSolidHorizontalSpan.covers = g.scanlineData.covers;
                    f.blendSolidHorizontalSpan.coversIndex = f
                        .scanlineSpan
                        .coverIndex;
                    f.blendSolidHorizontalSpan.blend = blend;
                    blendSolidHorizontalSpan(g, f.blendSolidHorizontalSpan);
                } else {
                    int32 x2 = x - f.scanlineSpan.length - 1;

                    f.blendHorizontalLine.x1 = x;
                    f.blendHorizontalLine.y = y;
                    f.blendHorizontalLine.x2 = x2;
                    f.blendHorizontalLine.sourceColor = f.color;
                    f.blendHorizontalLine.cover = g.scanlineData.covers[
                        uint32(f.scanlineSpan.coverIndex)
                    ];
                    f.blendHorizontalLine.blend = blend;
                    blendHorizontalLine(g, f.blendHorizontalLine);
                }

                if (--spanCount == 0) break;
                g.scanlineData.current++;
                f.scanlineSpan = g.scanlineData.spans[
                    uint32(g.scanlineData.current - 1)
                ];
            }
        }
    }

    function sweepScanline(Graphics2D memory g, Cell memory current)
        private
        pure
        returns (bool)
    {
        for (;;) {
            if (g.scanlineData.scanY > g.cellData.maxY) return false;

            resetSpans(g.scanlineData);
            int32 cellCount = g
                .cellData
                .sortedY[uint32(g.scanlineData.scanY - g.cellData.minY)]
                .count;

            int32 offset = g
                .cellData
                .sortedY[uint32(g.scanlineData.scanY - g.cellData.minY)]
                .start;
            int32 cover = 0;

            while (cellCount != 0) {
                current = g.cellData.sortedCells[uint32(offset)];
                int32 x = current.x;
                int32 area = current.area;
                int32 alpha;

                cover += current.cover;

                while (--cellCount != 0) {
                    offset++;
                    current = g.cellData.sortedCells[uint32(offset)];
                    if (current.x != x) break;

                    area += current.area;
                    cover += current.cover;
                }

                if (area != 0) {
                    alpha = calculateAlpha(
                        g,
                        (cover << (g.ss.value + 1)) - area
                    );
                    if (alpha != 0) {
                        addCell(g.scanlineData, x, alpha);
                    }
                    x++;
                }

                if (cellCount != 0 && current.x > x) {
                    alpha = calculateAlpha(g, cover << (g.ss.value + 1));
                    if (alpha != 0) {
                        addSpan(g.scanlineData, x, current.x - x, alpha);
                    }
                }
            }

            if (g.scanlineData.spanIndex != 0) break;
            ++g.scanlineData.scanY;
        }

        g.scanlineData.y = g.scanlineData.scanY;
        ++g.scanlineData.scanY;
        return true;
    }

    function calculateAlpha(Graphics2D memory g, int32 area)
        private
        pure
        returns (int32)
    {
        int32 cover = area >> (g.ss.value * 2 + 1 - g.aa.value);
        if (cover < 0) cover = -cover;
        if (cover > int32(g.aa.mask)) cover = int32(g.aa.mask);
        return cover;
    }

    function addSpan(
        ScanlineData memory scanlineData,
        int32 x,
        int32 len,
        int32 cover
    ) private pure {
        if (
            x == scanlineData.lastX + 1 &&
            scanlineData.spans[uint32(scanlineData.spanIndex)].length < 0 &&
            cover ==
            scanlineData.spans[uint32(scanlineData.spanIndex)].coverIndex
        ) {
            scanlineData.spans[uint32(scanlineData.spanIndex)].length -= int16(
                len
            );
        } else {
            scanlineData.covers[uint32(scanlineData.coverIndex)] = uint8(
                uint32(cover)
            );
            scanlineData.spanIndex++;
            scanlineData
                .spans[uint32(scanlineData.spanIndex)]
                .coverIndex = scanlineData.coverIndex++;
            scanlineData.spans[uint32(scanlineData.spanIndex)].x = int16(x);
            scanlineData.spans[uint32(scanlineData.spanIndex)].length = int16(
                -len
            );
        }

        scanlineData.lastX = x + len - 1;
    }

    function addCell(
        ScanlineData memory scanlineData,
        int32 x,
        int32 cover
    ) private pure {
        scanlineData.covers[uint32(scanlineData.coverIndex)] = uint8(
            uint32(cover)
        );
        if (
            x == scanlineData.lastX + 1 &&
            scanlineData.spans[uint32(scanlineData.spanIndex)].length > 0
        ) {
            scanlineData.spans[uint32(scanlineData.spanIndex)].length++;
        } else {
            scanlineData.spanIndex++;
            scanlineData
                .spans[uint32(scanlineData.spanIndex)]
                .coverIndex = scanlineData.coverIndex;
            scanlineData.spans[uint32(scanlineData.spanIndex)].x = int16(x);
            scanlineData.spans[uint32(scanlineData.spanIndex)].length = 1;
        }
        scanlineData.lastX = x;
        scanlineData.coverIndex++;
    }

    function resetSpans(ScanlineData memory scanlineData) private pure {
        scanlineData.lastX = 0x7FFFFFF0;
        scanlineData.coverIndex = 0;
        scanlineData.spanIndex = 0;
        scanlineData.spans[uint32(scanlineData.spanIndex)].length = 0;
    }

    function resetScanline(ScanlineData memory scanlineData) private pure {
        scanlineData.lastX = 0x7FFFFFF0;
        scanlineData.coverIndex = 0;
        scanlineData.spanIndex = 0;
        scanlineData.spans[uint32(scanlineData.spanIndex)].length = 0;
    }

    function blendSolidHorizontalSpan(
        Graphics2D memory g,
        BlendSolidHorizontalSpan memory f
    ) private pure {
        int32 colorAlpha = (int32)(f.sourceColor >> 24);

        if (colorAlpha != 0) {
            unchecked {
                int32 bufferOffset = Graphics2DMethods.getBufferOffsetXy(
                    g,
                    f.x,
                    f.y
                );
                if (bufferOffset == -1) return;

                int32 i = 0;
                do {
                    int32 alpha = !f.blend
                        ? colorAlpha
                        : (colorAlpha *
                            (
                                int32(
                                    uint32(f.covers[uint32(f.coversIndex)]) + 1
                                )
                            )) >> 8;

                    if (alpha == 255) {
                        Graphics2DMethods.copyPixels(
                            g.buffer,
                            bufferOffset,
                            f.sourceColor,
                            1
                            // , g.clippingData.clipPoly.length == 0
                            //     ? PixelClipping(new Vector2[](0), 0, 0)
                            //     : PixelClipping(
                            //         g.clippingData.clipPoly,
                            //         f.x + i,
                            //         f.y
                            //     )
                        );
                    } else {
                        uint32 targetColor = ColorMath.toColor(
                            uint8(uint32(alpha)),
                            uint8(f.sourceColor >> 16),
                            uint8(f.sourceColor >> 8),
                            uint8(f.sourceColor >> 0)
                        );

                        Graphics2DMethods.blendPixel(
                            g.buffer,
                            bufferOffset,
                            targetColor
                            // , g.clippingData.clipPoly.length == 0
                            //     ? PixelClipping(new Vector2[](0), 0, 0)
                            //     : PixelClipping(
                            //         g.clippingData.clipPoly,
                            //         f.x + i,
                            //         f.y
                            //     )
                        );
                    }

                    bufferOffset += 4;
                    f.coversIndex++;
                    i++;
                } while (--f.len != 0);
            }
        }
    }

    function blendHorizontalLine(
        Graphics2D memory g,
        BlendHorizontalLine memory f
    ) private pure {
        int32 colorAlpha = (int32)(f.sourceColor >> 24);

        if (colorAlpha != 0) {
            int32 len = f.x2 - f.x1 + 1;
            int32 bufferOffset = Graphics2DMethods.getBufferOffsetXy(
                g,
                f.x1,
                f.y
            );
            int32 alpha = !f.blend
                ? colorAlpha
                : (colorAlpha * (int32(uint32(f.cover)) + 1)) >> 8;

            if (alpha == 255) {
                Graphics2DMethods.copyPixels(
                    g.buffer,
                    bufferOffset,
                    f.sourceColor,
                    len
                    // , g.clippingData.clipPoly.length == 0
                    //     ? PixelClipping(new Vector2[](0), 0, 0)
                    //     : PixelClipping(g.clippingData.clipPoly, f.x1, f.y)
                );
            } else {
                int32 i = 0;

                uint32 targetColor = ColorMath.toColor(
                    uint8(uint32(alpha)),
                    uint8(f.sourceColor >> 16),
                    uint8(f.sourceColor >> 8),
                    uint8(f.sourceColor >> 0)
                );

                do {
                    Graphics2DMethods.blendPixel(
                        g.buffer,
                        bufferOffset,
                        targetColor
                        // , g.clippingData.clipPoly.length == 0
                        //     ? PixelClipping(new Vector2[](0), 0, 0)
                        //     : PixelClipping(
                        //         g.clippingData.clipPoly,
                        //         f.x1 + i,
                        //         f.y
                        //     )
                    );

                    bufferOffset += 4;
                    i++;
                } while (--len != 0);
            }
        }
    }
}
