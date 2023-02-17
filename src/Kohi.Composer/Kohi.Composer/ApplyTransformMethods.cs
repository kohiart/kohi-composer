// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class ApplyTransformMethods
{
    public static int MaxTransformSize = int.MinValue;

    public static IList<VertexData> ApplyTransform(IList<VertexData> vertices, Matrix transform)
    {
        if (vertices.Count > MaxTransformSize)
            MaxTransformSize = vertices.Count;

        var results = new VertexData[vertices.Count];
        for (var i = 0; i < vertices.Count; i++)
        {
            var vertexData = vertices[i];
            var transformedVertex = vertexData;

            if (transformedVertex.Command != Command.Stop && transformedVertex.Command != Command.EndPoly)
            {
                var position = transformedVertex.Position;

                var x = position.X;
                var y = position.Y;
                transform.Transform(ref x, ref y);
                position.X = x;
                position.Y = y;

                transformedVertex.Position = position;
            }

            results[i] = transformedVertex;
        }

        return results;
    }

    public static IList<VertexData> ApplyTransform(IList<VertexData> vertices, Matrix transform, int yUp)
    {
        var results = new List<VertexData>();
        foreach (var vertexData in vertices)
        {
            var transformedVertex = vertexData;

            Command? command = transformedVertex.Command;
            if (command != Command.Stop && command != Command.EndPoly)
            {
                var position = transformedVertex.Position;

                var x = position.X;
                var y = position.Y;
                transform.Transform(ref x, ref y);
                position.X = x;
                position.Y = y;

                position.Y = Fix64.Sub(yUp * Fix64.One, position.Y);
                transformedVertex.Position = position;
            }

            results.Add(transformedVertex);
        }

        return results;
    }
}