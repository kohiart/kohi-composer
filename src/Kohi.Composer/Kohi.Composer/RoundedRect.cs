// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public class RoundedRect
{
    private readonly Rectangle _bounds;
    private readonly Vector2 _leftBottomRadius;
    private readonly Vector2 _leftTopRadius;
    private readonly Vector2 _rightBottomRadius;
    private readonly Vector2 _rightTopRadius;
    private readonly Matrix _transform;

    public RoundedRect(long left, long bottom, long right, long top, long radius, Matrix transform)
    {
        _bounds = new Rectangle(left, bottom, right, top);

        _leftBottomRadius.X = radius;
        _leftBottomRadius.Y = radius;
        _rightBottomRadius.X = radius;
        _rightBottomRadius.Y = radius;
        _rightTopRadius.X = radius;
        _rightTopRadius.Y = radius;
        _leftTopRadius.X = radius;
        _leftTopRadius.Y = radius;

        _transform = transform;

        if (left > right) _bounds = new Rectangle(right, bottom, left, top);

        if (bottom > top) _bounds = new Rectangle(left, top, right, bottom);
    }

    public int YShift { get; set; }

    public IEnumerable<VertexData> Vertices()
    {
        var right = _bounds.Right;
        var left = _bounds.Left;
        var bottom = _bounds.Bottom;
        var top = _bounds.Top;

        var vertices = new List<VertexData>();

        var v0 = _transform.Transform(new Vector2(
            Fix64.Add(left, _leftBottomRadius.X),
            Fix64.Add(bottom, _leftBottomRadius.Y)
        ));

        var arc0 = new Arc(
            v0.X,
            Fix64.Sub(YShift * Fix64.One, v0.Y),
            _leftBottomRadius.X,
            _leftBottomRadius.Y,
            Fix64.Pi,
            Fix64.Add(Fix64.Pi, Fix64.PiOver2)
        );

        JoinPaths.Join(vertices, arc0, 0);

        var v1 = _transform.Transform(new Vector2(
            Fix64.Sub(right, _rightBottomRadius.X),
            Fix64.Add(bottom, _rightBottomRadius.Y)
        ));

        var arc1 = new Arc(
            v1.X,
            Fix64.Sub(YShift * Fix64.One, v1.Y),
            _rightBottomRadius.X,
            _rightBottomRadius.Y,
            Fix64.Add(Fix64.Pi, Fix64.PiOver2),
            0);

        JoinPaths.Join(vertices, arc1, 1);

        var v2 = _transform.Transform(new Vector2(
            Fix64.Sub(right, _rightTopRadius.X),
            Fix64.Sub(top, _rightTopRadius.Y)
        ));

        var arc2 = new Arc(
            v2.X,
            Fix64.Sub(YShift * Fix64.One, v2.Y),
            _rightTopRadius.X,
            _rightTopRadius.Y,
            0,
            Fix64.PiOver2);

        JoinPaths.Join(vertices, arc2, 2);

        var v3 = _transform.Transform(new Vector2(
            Fix64.Add(left, _leftTopRadius.X),
            Fix64.Sub(top, _leftTopRadius.Y)
        ));

        var arc3 = new Arc(
            v3.X,
            Fix64.Sub(YShift * Fix64.One, v3.Y),
            _leftTopRadius.X,
            _leftTopRadius.Y,
            Fix64.PiOver2,
            Fix64.Pi);

        JoinPaths.Join(vertices, arc3, 3);

        vertices.Add(new VertexData(Command.EndPoly, new Vector2()));
        vertices.Add(new VertexData(Command.Stop, new Vector2()));

        return vertices;
    }
}