// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class CellDataMethods
{
    public static CellData Create(int scale)
    {
        var cellData = new CellData();
        cellData.Cb = CellBlockMethods.Create(12, scale);
        cellData.Cells = new Cell[cellData.Cb.Limit];
        cellData.SortedCells = new Cell[cellData.Cb.Limit];
        cellData.SortedY = new SortedY[2401 * scale];
        cellData.Sorted = false;
        cellData.Style = new Cell();
        cellData.Current = new Cell();

        cellData.MinX = 0x7FFFFFFF;
        cellData.MinY = 0x7FFFFFFF;
        cellData.MaxX = -0x7FFFFFFF;
        cellData.MaxY = -0x7FFFFFFF;
        return cellData;
    }
}