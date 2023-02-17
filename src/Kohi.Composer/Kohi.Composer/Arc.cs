// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public class Arc
{
    public readonly long Angle;
    public readonly long DeltaAngle;
    public readonly Vector2 Origin;
    public readonly Vector2 Radius;
    public readonly long StartAngle;
    public readonly int Steps;

    public Arc(long originX, long originY, long radiusX, long radiusY, long startAngle, long angle)
    {
        Origin = new Vector2(originX, originY);
        Radius = new Vector2(radiusX, radiusY);

        StartAngle = startAngle;

        var averageRadius = Fix64.Div(Fix64.Add(Fix64.Abs(Radius.X), Fix64.Abs(Radius.Y)), Fix64.Two);

        DeltaAngle = Fix64.Mul(
            Trig256.Acos(Fix64.Div(averageRadius,
                Fix64.Add(averageRadius, Fix64.Div(536870912L /* 0.125 */, Fix64.One)))),
            Fix64.Two);

        while (angle < StartAngle)
            angle = Fix64.Add(angle, Fix64.TwoPi);

        Angle = angle;

        Steps = (int) (Fix64.Div(Fix64.Sub(Angle, StartAngle), DeltaAngle) / Fix64.One);
    }

    public IEnumerable<VertexData> Vertices()
    {
        var results = new VertexData[Steps + 3];

        var vertexData = new VertexData();
        vertexData.Command = Command.MoveTo;

        {
            vertexData.Position = new Vector2(
                Fix64.Add(Origin.X, Fix64.Mul(Trig256.Cos(StartAngle), Radius.X)),
                Fix64.Add(Origin.Y, Fix64.Mul(Trig256.Sin(StartAngle), Radius.Y))
            );

            results[0] = vertexData;
        }

        vertexData.Command = Command.LineTo;
        var angle = StartAngle;

        for (var i = 0; i <= Steps; i++)
            if (angle < Angle)
            {
                vertexData.Position = new Vector2(
                    Fix64.Add(Origin.X, Fix64.Mul(Trig256.Cos(angle), Radius.X)),
                    Fix64.Add(Origin.Y, Fix64.Mul(Trig256.Sin(angle), Radius.Y))
                );

                results[1 + i] = vertexData;

                angle = Fix64.Add(angle, DeltaAngle);
            }

        {
            vertexData.Position = new Vector2(
                Fix64.Add(Origin.X, Fix64.Mul(Trig256.Cos(angle), Radius.X)),
                Fix64.Add(Origin.Y, Fix64.Mul(Trig256.Sin(angle), Radius.Y))
            );

            results[1 + Steps] = vertexData;
        }


        vertexData.Command = Command.Stop;
        results[2 + Steps] = vertexData;

        return results;
    }
}