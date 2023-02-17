// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class JoinPaths
{
    public static void Join(List<VertexData> vertices, Arc sourcePath, int i)
    {
        var firstMove = true;
        var list = sourcePath.Vertices().ToList();
        foreach (var vertexData in list)
        {
            if (i > 0
                && firstMove
                && vertexData.Command == Command.MoveTo)
            {
                firstMove = false;
                continue;
            }

            if (vertexData.Command == Command.Stop) break;
            vertices.Add(vertexData);
        }
    }
}