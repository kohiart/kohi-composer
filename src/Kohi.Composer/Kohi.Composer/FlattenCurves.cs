// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public class FlattenCurves
{
    public static IList<VertexData> Flatten(IList<VertexData> vertexSource)
    {
        var result = new List<VertexData>();
        var lastPosition = new VertexData();

        var vertexDataEnumerator = vertexSource.GetEnumerator();
        while (vertexDataEnumerator.MoveNext())
        {
            var vertexData = vertexDataEnumerator.Current;
            switch (vertexData.Command)
            {
                case Command.Curve3:
                {
                    vertexDataEnumerator.MoveNext();
                    var vertexDataEnd = vertexDataEnumerator.Current;

                    var curve3 = new Curve3(
                        lastPosition.Position.X,
                        lastPosition.Position.Y,
                        vertexData.Position.X,
                        vertexData.Position.Y,
                        vertexDataEnd.Position.X,
                        vertexDataEnd.Position.Y
                    );

                    var curveIterator = curve3.Vertices().GetEnumerator();
                    curveIterator.MoveNext();
                    do
                    {
                        curveIterator.MoveNext();
                        if (curveIterator.Current.Command == Command.Stop) break;
                        vertexData = new VertexData(Command.LineTo,
                            curveIterator.Current.Position);
                        result.Add(vertexData);
                        lastPosition = vertexData;
                    } while (curveIterator.Current.Command != Command.Stop);
                }
                    break;

                case Command.Curve4:
                {
                    vertexDataEnumerator.MoveNext();
                    var vertexDataControl = vertexDataEnumerator.Current;
                    vertexDataEnumerator.MoveNext();
                    var vertexDataEnd = vertexDataEnumerator.Current;

                    var curve4 = new Curve4(
                        lastPosition.Position.X,
                        lastPosition.Position.Y,
                        vertexData.Position.X,
                        vertexData.Position.Y,
                        vertexDataControl.Position.X,
                        vertexDataControl.Position.Y,
                        vertexDataEnd.Position.X,
                        vertexDataEnd.Position.Y);

                    var curveIterator = curve4.Vertices().GetEnumerator();
                    curveIterator.MoveNext();
                    while (vertexData.Command != Command.Stop)
                    {
                        curveIterator.MoveNext();
                        if (curveIterator.Current.Command == Command.Stop) break;
                        vertexData = new VertexData(Command.LineTo,
                            curveIterator.Current.Position);

                        result.Add(vertexData);
                        lastPosition = vertexData;
                    }
                }
                    break;

                default:
                    result.Add(vertexData);
                    lastPosition = vertexData;
                    break;
            }
        }

        return result;
    }
}