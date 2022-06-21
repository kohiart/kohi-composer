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

import "./CellData.sol";
import "./SubpixelScale.sol";
import "./Graphics2D.sol";

library CellRasterizer {
    function resetCells(CellData memory cellData) internal pure {
        cellData.used = 0;
        CellMethods.reset(cellData.style);
        CellMethods.reset(cellData.current);
        cellData.sorted = false;

        cellData.minX = cellData.minY = type(int32).max;
        cellData.maxX = cellData.maxY = type(int32).min;
    }

    function line(
        DrawContext memory c,
        Line memory f,
        CellData memory cellData,
        SubpixelScale memory ss
    ) internal pure {
        c.lineArgs.dx = f.x2 - f.x1;

        if (
            c.lineArgs.dx >= int32(ss.dxLimit) ||
            c.lineArgs.dx <= -int32(ss.dxLimit)
        ) {
            int32 cx = (f.x1 + f.x2) >> 1;
            int32 cy = (f.y1 + f.y2) >> 1;

            c.lineRecursive.x1 = f.x1;
            c.lineRecursive.y1 = f.y1;
            c.lineRecursive.x2 = cx;
            c.lineRecursive.y2 = cy;
            line(c, c.lineRecursive, cellData, ss);

            c.lineRecursive.x1 = cx;
            c.lineRecursive.y1 = cy;
            c.lineRecursive.x2 = f.x2;
            c.lineRecursive.y2 = f.y2;
            line(c, c.lineRecursive, cellData, ss);
        }

        c.lineArgs.dy = f.y2 - f.y1;
        c.lineArgs.ex1 = f.x1 >> ss.value;
        c.lineArgs.ex2 = f.x2 >> ss.value;
        c.lineArgs.ey1 = f.y1 >> ss.value;
        c.lineArgs.ey2 = f.y2 >> ss.value;
        c.lineArgs.fy1 = f.y1 & int32(ss.mask);
        c.lineArgs.fy2 = f.y2 & int32(ss.mask);

        {
            if (c.lineArgs.ex1 < cellData.minX) cellData.minX = c.lineArgs.ex1;
            if (c.lineArgs.ex1 > cellData.maxX) cellData.maxX = c.lineArgs.ex1;
            if (c.lineArgs.ey1 < cellData.minY) cellData.minY = c.lineArgs.ey1;
            if (c.lineArgs.ey1 > cellData.maxY) cellData.maxY = c.lineArgs.ey1;
            if (c.lineArgs.ex2 < cellData.minX) cellData.minX = c.lineArgs.ex2;
            if (c.lineArgs.ex2 > cellData.maxX) cellData.maxX = c.lineArgs.ex2;
            if (c.lineArgs.ey2 < cellData.minY) cellData.minY = c.lineArgs.ey2;
            if (c.lineArgs.ey2 > cellData.maxY) cellData.maxY = c.lineArgs.ey2;

            setCurrentCell(c.lineArgs.ex1, c.lineArgs.ey1, cellData);

            if (c.lineArgs.ey1 == c.lineArgs.ey2) {
                c.horizontalLine.ey = c.lineArgs.ey1;
                c.horizontalLine.x1 = f.x1;
                c.horizontalLine.y1 = c.lineArgs.fy1;
                c.horizontalLine.x2 = f.x2;
                c.horizontalLine.y2 = c.lineArgs.fy2;

                renderHorizontalLine(
                    c.horizontalLine,
                    c.horizontalLineArgs,
                    cellData,
                    ss
                );
                return;
            }
        }

        c.lineArgs.incr = 1;

        if (c.lineArgs.dx == 0) {
            int32 ex = f.x1 >> ss.value;
            int32 twoFx = (f.x1 - (ex << ss.value)) << 1;

            c.lineArgs.first = int32(ss.scale);
            if (c.lineArgs.dy < 0) {
                c.lineArgs.first = 0;
                c.lineArgs.incr = -1;
            }

            c.lineArgs.delta = c.lineArgs.first - c.lineArgs.fy1;
            cellData.current.cover += c.lineArgs.delta;
            cellData.current.area += twoFx * c.lineArgs.delta;

            c.lineArgs.ey1 += c.lineArgs.incr;
            setCurrentCell(ex, c.lineArgs.ey1, cellData);

            c.lineArgs.delta =
                c.lineArgs.first +
                c.lineArgs.first -
                int32(ss.scale);
            int32 area = twoFx * c.lineArgs.delta;
            while (c.lineArgs.ey1 != c.lineArgs.ey2) {
                cellData.current.cover = c.lineArgs.delta;
                cellData.current.area = area;
                c.lineArgs.ey1 += c.lineArgs.incr;
                setCurrentCell(ex, c.lineArgs.ey1, cellData);
            }

            c.lineArgs.delta =
                c.lineArgs.fy2 -
                int32(ss.scale) +
                c.lineArgs.first;
            cellData.current.cover += c.lineArgs.delta;
            cellData.current.area += twoFx * c.lineArgs.delta;
            return;
        }

        int32 p = (int32(ss.scale) - c.lineArgs.fy1) * c.lineArgs.dx;
        c.lineArgs.first = int32(ss.scale);

        if (c.lineArgs.dy < 0) {
            p = c.lineArgs.fy1 * c.lineArgs.dx;
            c.lineArgs.first = 0;
            c.lineArgs.incr = -1;
            c.lineArgs.dy = -c.lineArgs.dy;
        }

        c.lineArgs.delta = p / c.lineArgs.dy;
        int32 mod = p % c.lineArgs.dy;

        if (mod < 0) {
            c.lineArgs.delta--;
            mod += c.lineArgs.dy;
        }

        int32 xFrom = f.x1 + c.lineArgs.delta;

        c.horizontalLine.ey = c.lineArgs.ey1;
        c.horizontalLine.x1 = f.x1;
        c.horizontalLine.y1 = c.lineArgs.fy1;
        c.horizontalLine.x2 = xFrom;
        c.horizontalLine.y2 = c.lineArgs.first;
        renderHorizontalLine(
            c.horizontalLine,
            c.horizontalLineArgs,
            cellData,
            ss
        );

        c.lineArgs.ey1 += c.lineArgs.incr;
        setCurrentCell(xFrom >> ss.value, c.lineArgs.ey1, cellData);

        if (c.lineArgs.ey1 != c.lineArgs.ey2) {
            p = int32(ss.scale) * c.lineArgs.dx;
            int32 lift = p / c.lineArgs.dy;
            int32 rem = p % c.lineArgs.dy;

            if (rem < 0) {
                lift--;
                rem += c.lineArgs.dy;
            }

            mod -= c.lineArgs.dy;

            while (c.lineArgs.ey1 != c.lineArgs.ey2) {
                c.lineArgs.delta = lift;
                mod += rem;
                if (mod >= 0) {
                    mod -= c.lineArgs.dy;
                    c.lineArgs.delta++;
                }

                int32 xTo = xFrom + c.lineArgs.delta;

                c.horizontalLine.ey = c.lineArgs.ey1;
                c.horizontalLine.x1 = xFrom;
                c.horizontalLine.y1 = int32(ss.scale) - c.lineArgs.first;
                c.horizontalLine.x2 = xTo;
                c.horizontalLine.y2 = c.lineArgs.first;

                renderHorizontalLine(
                    c.horizontalLine,
                    c.horizontalLineArgs,
                    cellData,
                    ss
                );
                xFrom = xTo;

                c.lineArgs.ey1 += c.lineArgs.incr;
                setCurrentCell(xFrom >> ss.value, c.lineArgs.ey1, cellData);
            }
        }

        c.horizontalLine.ey = c.lineArgs.ey1;
        c.horizontalLine.x1 = xFrom;
        c.horizontalLine.y1 = int32(ss.scale) - c.lineArgs.first;
        c.horizontalLine.x2 = f.x2;
        c.horizontalLine.y2 = c.lineArgs.fy2;
        renderHorizontalLine(
            c.horizontalLine,
            c.horizontalLineArgs,
            cellData,
            ss
        );
    }

    function sortCells(CellData memory cellData) internal pure {
        if (cellData.sorted) return;

        addCurrentCell(cellData);

        cellData.current.x = 0x7FFFFFFF;
        cellData.current.y = 0x7FFFFFFF;
        cellData.current.cover = 0;
        cellData.current.area = 0;

        if (cellData.used == 0) return;

        uint32 sortedYSize = uint32(
            uint32(cellData.maxY) - uint32(cellData.minY) + 1
        );

        for (uint32 i = 0; i < sortedYSize; i++) {
            cellData.sortedY[i].start = 0;
            cellData.sortedY[i].count = 0;
        }

        for (uint32 i = 0; i < cellData.used; i++) {
            int32 index = cellData.cells[i].y - cellData.minY;
            cellData.sortedY[uint32(index)].start++;
        }

        int32 start = 0;
        for (uint32 i = 0; i < sortedYSize; i++) {
            int32 v = cellData.sortedY[i].start;
            cellData.sortedY[i].start = start;
            start += v;
        }

        for (uint32 i = 0; i < cellData.used; i++) {
            int32 index = cellData.cells[i].y - cellData.minY;
            int32 currentYStart = cellData.sortedY[uint32(index)].start;
            int32 currentYCount = cellData.sortedY[uint32(index)].count;
            cellData.sortedCells[
                uint32(currentYStart) + uint32(currentYCount)
            ] = cellData.cells[i];
            ++cellData.sortedY[uint32(index)].count;
        }

        for (uint32 i = 0; i < sortedYSize; i++)
            if (cellData.sortedY[i].count != 0)
                sort(
                    cellData.sortedCells,
                    cellData.sortedY[i].start,
                    cellData.sortedY[i].start + cellData.sortedY[i].count - 1
                );

        cellData.sorted = true;
    }

    function renderHorizontalLine(
        RenderHorizontalLine memory f,
        RenderHorizontalLineArgs memory a,
        CellData memory cellData,
        SubpixelScale memory ss
    ) private pure {
        a.ex1 = f.x1 >> ss.value;
        a.ex2 = f.x2 >> ss.value;
        a.fx1 = f.x1 & int32(ss.mask);
        a.fx2 = f.x2 & int32(ss.mask);
        a.delta = 0;

        if (f.y1 == f.y2) {
            setCurrentCell(a.ex2, f.ey, cellData);
            return;
        }

        if (a.ex1 == a.ex2) {
            a.delta = f.y2 - f.y1;
            cellData.current.cover += a.delta;
            cellData.current.area += (a.fx1 + a.fx2) * a.delta;
            return;
        }

        int32 p = (int32(ss.scale) - a.fx1) * (f.y2 - f.y1);
        int32 first = int32(ss.scale);
        int32 incr = 1;
        int32 dx = f.x2 - f.x1;

        if (dx < 0) {
            p = a.fx1 * (f.y2 - f.y1);
            first = 0;
            incr = -1;
            dx = -dx;
        }

        a.delta = p / dx;
        int32 mod = p % dx;

        if (mod < 0) {
            a.delta--;
            mod += dx;
        }

        cellData.current.cover += a.delta;
        cellData.current.area += (a.fx1 + first) * a.delta;

        a.ex1 += incr;
        setCurrentCell(a.ex1, f.ey, cellData);
        f.y1 += a.delta;

        if (a.ex1 != a.ex2) {
            p = int32(ss.scale) * (f.y2 - f.y1 + a.delta);
            int32 lift = p / dx;
            int32 rem = p % dx;

            if (rem < 0) {
                lift--;
                rem += dx;
            }

            mod -= dx;

            while (a.ex1 != a.ex2) {
                a.delta = lift;
                mod += rem;
                if (mod >= 0) {
                    mod -= dx;
                    a.delta++;
                }

                cellData.current.cover += a.delta;
                cellData.current.area += int32(ss.scale) * a.delta;
                f.y1 += a.delta;
                a.ex1 += incr;
                setCurrentCell(a.ex1, f.ey, cellData);
            }
        }

        a.delta = f.y2 - f.y1;
        cellData.current.cover += a.delta;
        cellData.current.area += (a.fx2 + int32(ss.scale) - first) * a.delta;
    }

    function setCurrentCell(
        int32 x,
        int32 y,
        CellData memory cellData
    ) private pure {
        if (CellMethods.notEqual(cellData.current, x, y, cellData.style)) {
            addCurrentCell(cellData);
            CellMethods.style(cellData.current, cellData.style);
            cellData.current.x = x;
            cellData.current.y = y;
            cellData.current.cover = 0;
            cellData.current.area = 0;
        }
    }

    function addCurrentCell(CellData memory cellData) private pure {
        if ((cellData.current.area | cellData.current.cover) != 0) {
            if (cellData.used >= cellData.cb.limit) return;
            CellMethods.set(cellData.cells[cellData.used], cellData.current);
            cellData.used++;
        }
    }

    function sort(
        Cell[] memory cells,
        int32 start,
        int32 stop
    ) private pure {
        while (true) {
            if (stop == start) return;

            int32 pivot;
            {
                int32 m = start + 1;
                int32 n = stop;
                while (m < stop && cells[uint32(start)].x >= cells[uint32(m)].x)
                    m++;

                while (
                    n > start && cells[uint32(start)].x <= cells[uint32(n)].x
                ) n--;
                while (m < n) {
                    (cells[uint32(m)], cells[uint32(n)]) = (
                        cells[uint32(n)],
                        cells[uint32(m)]
                    );
                    while (
                        m < stop && cells[uint32(start)].x >= cells[uint32(m)].x
                    ) m++;
                    while (
                        n > start &&
                        cells[uint32(start)].x <= cells[uint32(n)].x
                    ) n--;
                }

                if (start != n) {
                    (cells[uint32(n)], cells[uint32(start)]) = (
                        cells[uint32(start)],
                        cells[uint32(n)]
                    );
                }
                pivot = n;
            }

            if (pivot > start) sort(cells, start, pivot - 1);

            if (pivot < stop) {
                start = pivot + 1;
                continue;
            }

            break;
        }
    }
}
