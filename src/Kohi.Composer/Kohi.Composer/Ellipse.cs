// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public class Ellipse
{
    public readonly long RadiusX;
    public readonly long RadiusY;
    public readonly int Steps;
    public long OriginX;
    public long OriginY;

    public Ellipse(long originX, long originY, long radiusX, long radiusY)
    {
        OriginX = originX;
        OriginY = originY;
        RadiusX = radiusX;
        RadiusY = radiusY;

        var ra = Fix64.Div(Fix64.Add(
                MathExtensions.Abs(RadiusX),
                MathExtensions.Abs(RadiusY)),
            Fix64.Two);

        var da = Fix64.Mul(
            Trig256.Acos(Fix64.Div(ra, Fix64.Add(ra,
                Fix64.Div(536870912L /* 0.125 */, Fix64.One)))),
            Fix64.Two
        );

        var t1 = Fix64.Mul(Fix64.Two, Fix64.Div(Fix64.Pi, da));
        Steps = (int) (Fix64.Round(t1) / (float) Fix64.One);
    }

    public IEnumerable<VertexData> Vertices()
    {
        var results = new VertexData[Steps + 3];

        var vertexData = new VertexData();
        vertexData.Command = Command.MoveTo;
        vertexData.Position = new Vector2(Fix64.Add(OriginX, RadiusX), OriginY);
        results[0] = vertexData;

        var anglePerStep = Fix64.Div(Fix64.TwoPi, Steps * Fix64.One);
        var angle = 0L;

        vertexData.Command = Command.LineTo;
        for (var i = 1; i < Steps; i++)
        {
            angle = Fix64.Add(angle, anglePerStep);

            var x = Fix64.Add(OriginX,
                Fix64.Mul(
                    Trig256.Cos(angle),
                    RadiusX
                ));

            var y = Fix64.Add(OriginY,
                Fix64.Mul(
                    Trig256.Sin(angle),
                    RadiusY
                ));

            vertexData.Position = new Vector2(x, y);
            results[i] = vertexData;
        }

        vertexData.Position = new Vector2();
        vertexData.Command = Command.EndPoly;
        results[Steps] = vertexData;

        vertexData.Command = Command.Stop;
        results[Steps + 1] = vertexData;

        return results;
    }
}