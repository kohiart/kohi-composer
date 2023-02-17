// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public class CustomPath
{
    public static int MaxNumVertices = int.MinValue;

    public int NumVertices;
    public VertexData[] VertexData;

    public CustomPath(int maxVertices = 8)
    {
        VertexData = new VertexData[maxVertices];
    }

    public IEnumerable<VertexData> Vertices()
    {
        var results = new VertexData[NumVertices + 1];
        for (var i = 0; i < NumVertices; i++)
        {
            var command = Vertex(i, out var x, out var y);
            results[i] = new VertexData(command, new Vector2(x, y));
        }

        results[NumVertices] = new VertexData(Command.Stop, new Vector2(0, 0));
        return results;
    }

    public void Add(long x, long y, Command command)
    {
        VertexData[NumVertices++] = new VertexData(command, x, y);
        if (MaxNumVertices < NumVertices) MaxNumVertices = NumVertices;
    }

    public void EndPoly()
    {
        var command = LastCommand();
        if (command != Command.Stop && command != Command.EndPoly)
        {
            VertexData[NumVertices++] = new VertexData(Command.MoveTo, 0, 0);
            if (MaxNumVertices < NumVertices) MaxNumVertices = NumVertices;
        }
    }

    public void MoveTo(long x, long y)
    {
        VertexData[NumVertices++] = new VertexData(Command.MoveTo, x, y);
        if (MaxNumVertices < NumVertices) MaxNumVertices = NumVertices;
    }

    public void LineTo(long x, long y)
    {
        VertexData[NumVertices++] = new VertexData(Command.LineTo, x, y);
        if (MaxNumVertices < NumVertices) MaxNumVertices = NumVertices;
    }

    public Command LastCommand()
    {
        return NumVertices != 0 ? VertexData[NumVertices - 1].Command : Command.Stop;
    }


    public long LastX()
    {
        if (NumVertices > 0)
        {
            var index = NumVertices - 1;
            return VertexData[index].Position.X;
        }

        return 0;
    }

    public long LastY()
    {
        if (NumVertices > 0)
        {
            var index = NumVertices - 1;
            return VertexData[index].Position.Y;
        }

        return 0;
    }

    public Command PreviousVertex(out long x, out long y)
    {
        if (NumVertices > 1) return Vertex(NumVertices - 2, out x, out y);

        x = 0;
        y = 0;

        return Command.Stop;
    }

    public Command Vertex(int index, out long x, out long y)
    {
        x = VertexData[index].Position.X;
        y = VertexData[index].Position.Y;
        return VertexData[index].Command;
    }

    public Command CommandAt(int index)
    {
        return VertexData[index].Command;
    }

    public Command LastVertex(out long x, out long y)
    {
        if (NumVertices != 0) return Vertex(NumVertices - 1, out x, out y);
        x = 0;
        y = 0;
        return Command.Stop;
    }
}