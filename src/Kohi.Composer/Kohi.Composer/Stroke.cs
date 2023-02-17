// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public sealed class Stroke
{
    private readonly VertexDistance[] _distances;
    private readonly Vector2[] _outVertices;

    private readonly VertexData[] _vertexSource;
    private readonly long _width;
    private readonly long _widthAbs;
    private readonly long _widthEps;
    private readonly long _widthSign;

    private bool _closed;
    private int _distanceCount;


    private Command _lastCommand;
    private int _outVertexCount;
    private int _outVerticesCount;
    private StrokeStatus _previousStatus;
    private int _srcVertex;
    private long _startX;
    private long _startY;

    private StrokeStatus _status;


    private VertexStatus _vertexStatus;

    public LineCap LineCap;
    public LineJoin LineJoin;

    public Stroke(IEnumerable<VertexData> vertexSource, long width = Fix64.One, int maxDistanceCount = 2000,
        int maxVertexCount = 2000)
    {
        _vertexSource = vertexSource.ToArray();
        _vertexStatus = VertexStatus.Initial;

        _distances = new VertexDistance[maxDistanceCount];
        _outVertices = new Vector2[maxVertexCount];
        _status = StrokeStatus.Initial;

        LineCap = LineCap.Butt;
        LineJoin = LineJoin.Miter;

        _width = Fix64.Mul(width, 2147483648 /* 0.5 */);
        if (_width < 0)
        {
            _widthAbs = -_width;
            _widthSign = -Fix64.One;
        }
        else
        {
            _widthAbs = _width;
            _widthSign = Fix64.One;
        }

        _widthEps = Fix64.Div(_width, 4398046511104 /* 1024 */);
    }

    public static IEnumerable<VertexData> Vertices(Stroke self)
    {
        self._vertexStatus = VertexStatus.Initial;

        uint count = 0;
        {
            Command command;
            var i = 0;
            do
            {
                (command, i, _, _) = Vertex(self, i, self._vertexSource);
                count++;
            } while (command != Command.Stop);
        }

        self._vertexStatus = VertexStatus.Initial;

        var results = new VertexData[count];
        {
            Command command;
            var i = 0;
            count = 0;
            do
            {
                (command, i, var x, var y) = Vertex(self, i, self._vertexSource);
                results[count++] = new VertexData(command, new Vector2(x, y));
            } while (command != Command.Stop);
        }

        return results;
    }


    public static (Command, int, long, long) Vertex(Stroke self, int i, IReadOnlyList<VertexData> v)
    {
        long x = 0;
        long y = 0;

        var command = Command.Stop;
        var done = false;

        while (!done)
        {
            VertexData c;

            if (self._vertexStatus == VertexStatus.Initial)
            {
                c = v[i++];
                self._lastCommand = c.Command;
                self._startX = c.Position.X;
                self._startY = c.Position.Y;
                self._vertexStatus = VertexStatus.Accumulate;
            }
            else if (self._vertexStatus == VertexStatus.Accumulate)
            {
                if (self._lastCommand == Command.Stop) return (Command.Stop, i, x, y);

                self.Clear();
                self.AddVertex(self._startX, self._startY, Command.MoveTo);

                for (;;)
                {
                    c = v[i++];

                    self._lastCommand = c.Command;
                    x = c.Position.X;
                    y = c.Position.Y;

                    command = c.Command;

                    if (command != Command.Stop && command != Command.EndPoly)
                    {
                        self._lastCommand = command;
                        if (command == Command.MoveTo)
                        {
                            self._startX = x;
                            self._startY = y;
                            break;
                        }

                        self.AddVertex(x, y, command);
                    }
                    else
                    {
                        if (command == Command.Stop)
                        {
                            self._lastCommand = Command.Stop;
                            break;
                        }

                        self.AddVertex(x, y, command);
                        break;
                    }
                }

                self.Rewind();
                self._vertexStatus = VertexStatus.Generate;
            }
            else if (self._vertexStatus == VertexStatus.Generate)
            {
                (command, x, y) = StrokeVertex(self);

                if (command == Command.Stop)
                    self._vertexStatus = VertexStatus.Accumulate;
                else
                    done = true;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return (command, i, x, y);
    }

    public void AddVertex(long x, long y, Command command)
    {
        _status = StrokeStatus.Initial;
        if (command == Command.MoveTo)
        {
            ModifyLast(new VertexDistance(x, y));
        }
        else
        {
            if (command != Command.Stop && command != Command.EndPoly)
                Add(new VertexDistance(x, y));
            else
                _closed = command == Command.EndPoly;
        }
    }

    private static (Command command, long x, long y) StrokeVertex(Stroke self)
    {
        long x = 0;
        long y = 0;

        var command = Command.LineTo;
        while (command != Command.Stop)
            if (self._status == StrokeStatus.Initial)
            {
                self.Rewind();
            }
            else if (self._status == StrokeStatus.Ready)
            {
                if (self._distanceCount < 2 + (self._closed ? 1 : 0))
                {
                    command = Command.Stop;
                }
                else
                {
                    self._status = self._closed ? StrokeStatus.Outline1 : StrokeStatus.Cap1;
                    command = Command.MoveTo;
                    self._srcVertex = 0;
                    self._outVertexCount = 0;
                }
            }
            else if (self._status == StrokeStatus.Cap1)
            {
                CalcCap(self, self._distances[0], self._distances[1], self._distances[0].Distance);

                self._srcVertex = 1;
                self._previousStatus = StrokeStatus.Outline1;
                self._status = StrokeStatus.OutVertices;
                self._outVertexCount = 0;
            }
            else if (self._status == StrokeStatus.Cap2)
            {
                CalcCap(self, self._distances[self._distanceCount - 1], self._distances[self._distanceCount - 2],
                    self._distances[self._distanceCount - 2].Distance);

                self._previousStatus = StrokeStatus.Outline2;
                self._status = StrokeStatus.OutVertices;
                self._outVertexCount = 0;
            }
            else if (self._status == StrokeStatus.Outline1)
            {
                var join = true;
                if (self._closed)
                {
                    if (self._srcVertex >= self._distanceCount)
                    {
                        self._previousStatus = StrokeStatus.CloseFirst;
                        self._status = StrokeStatus.EndPoly1;
                        join = false;
                    }
                }
                else
                {
                    if (self._srcVertex >= self._distanceCount - 1)
                    {
                        self._status = StrokeStatus.Cap2;
                        join = false;
                    }
                }

                if (join)
                {
                    CalcJoin(self,
                        self.Previous(self._srcVertex),
                        self.Current(self._srcVertex),
                        self.Next(self._srcVertex),
                        self.Previous(self._srcVertex).Distance,
                        self.Current(self._srcVertex).Distance);

                    ++self._srcVertex;
                    self._previousStatus = self._status;
                    self._status = StrokeStatus.OutVertices;
                    self._outVertexCount = 0;
                }
            }
            else if (self._status == StrokeStatus.CloseFirst)
            {
                self._status = StrokeStatus.Outline2;
                command = Command.MoveTo;
            }
            else if (self._status == StrokeStatus.Outline2)
            {
                var join = true;
                if (self._srcVertex <= (!self._closed ? 1 : 0))
                {
                    self._status = StrokeStatus.EndPoly2;
                    self._previousStatus = StrokeStatus.Stop;
                    join = false;
                }

                if (join)
                {
                    --self._srcVertex;

                    CalcJoin(self,
                        self.Next(self._srcVertex),
                        self.Current(self._srcVertex),
                        self.Previous(self._srcVertex),
                        self.Current(self._srcVertex).Distance,
                        self.Previous(self._srcVertex).Distance
                    );

                    self._previousStatus = self._status;
                    self._status = StrokeStatus.OutVertices;
                    self._outVertexCount = 0;
                }
            }
            else if (self._status == StrokeStatus.OutVertices)
            {
                if (self._outVertexCount >= self._outVerticesCount)
                {
                    self._status = self._previousStatus;
                }
                else
                {
                    var c = self._outVertices[self._outVertexCount++];
                    x = c.X;
                    y = c.Y;
                    return (command, c.X, y);
                }
            }
            else if (self._status == StrokeStatus.EndPoly1)
            {
                self._status = self._previousStatus;
                return (Command.EndPoly, x, y);
            }
            else if (self._status == StrokeStatus.EndPoly2)
            {
                self._status = self._previousStatus;
                return (Command.EndPoly, x, y);
            }
            else if (self._status == StrokeStatus.Stop)
            {
                command = Command.Stop;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

        return (command, x, y);
    }

    public static void CalcCap(Stroke self, VertexDistance v0, VertexDistance v1, long len)
    {
        self._outVerticesCount = 0;

        CalcCapArgs a;

        a.Dx1 = Fix64.Div(Fix64.Sub(v1.Y, v0.Y), len);
        a.Dy1 = Fix64.Div(Fix64.Sub(v1.X, v0.X), len);
        a.Dx2 = 0;
        a.Dy2 = 0;

        a.Dx1 = Fix64.Mul(a.Dx1, self._width);
        a.Dy1 = Fix64.Mul(a.Dy1, self._width);

        if (self.LineCap != LineCap.Round)
        {
            if (self.LineCap == LineCap.Square)
            {
                a.Dx2 = a.Dy1 * self._widthSign;
                a.Dy2 = a.Dx1 * self._widthSign;
            }

            self._outVertices[self._outVerticesCount++] = new Vector2(Fix64.Sub(v0.X, Fix64.Sub(a.Dx1, a.Dx2)),
                Fix64.Add(v0.Y, Fix64.Sub(a.Dy1, a.Dy2)));
            self._outVertices[self._outVerticesCount++] = new Vector2(Fix64.Add(v0.X, Fix64.Sub(a.Dx1, a.Dx2)),
                Fix64.Sub(v0.Y, Fix64.Sub(a.Dy1, a.Dy2)));
        }
        else
        {
            a.Da = Fix64.Mul(Trig256.Acos(
                Fix64.Div(self._widthAbs,
                    Fix64.Add(self._widthAbs, Fix64.Div(536870912 /* 0.125 */, Fix64.One)))), Fix64.Two);

            a.N = (int) (Fix64.Div(Fix64.Pi, a.Da) / Fix64.One);

            a.Da = Fix64.Div(Fix64.Pi, (a.N + 1) * Fix64.One);

            self._outVertices[self._outVerticesCount++] =
                new Vector2(Fix64.Sub(v0.X, a.Dx1), Fix64.Add(v0.Y, a.Dy1));

            if (self._widthSign > 0)
            {
                a.A1 = Trig256.Atan2(a.Dy1, -a.Dx1);
                a.A1 = Fix64.Add(a.A1, a.Da);
                for (a.I = 0; a.I < a.N; a.I++)
                {
                    self._outVertices[self._outVerticesCount++] = new Vector2(
                        Fix64.Add(v0.X, Fix64.Mul(Trig256.Cos(a.A1), self._width)),
                        Fix64.Add(v0.Y, Fix64.Mul(Trig256.Sin(a.A1), self._width))
                    );
                    a.A1 += a.Da;
                }
            }
            else
            {
                a.A1 = Trig256.Atan2(-a.Dy1, a.Dx1);
                a.A1 = Fix64.Sub(a.A1, a.Da);
                for (a.I = 0; a.I < a.N; a.I++)
                {
                    self._outVertices[self._outVerticesCount++] = new Vector2(
                        Fix64.Add(v0.X, Fix64.Mul(Trig256.Cos(a.A1), self._width)),
                        Fix64.Add(v0.Y, Fix64.Mul(Trig256.Sin(a.A1), self._width))
                    );

                    a.A1 = Fix64.Sub(a.A1, a.Da);
                }
            }

            self._outVertices[self._outVerticesCount++] = new Vector2(
                Fix64.Add(v0.X, a.Dx1),
                Fix64.Sub(v0.Y, a.Dy1)
            );
        }
    }

    public static void CalcJoin(Stroke self, VertexDistance v0, VertexDistance v1, VertexDistance v2, long len1,
        long len2)
    {
        self._outVerticesCount = 0;

        CalcJoinArgs a;

        a.Dx1 = Fix64.Mul(self._width, Fix64.Div(Fix64.Sub(v1.Y, v0.Y), len1));
        a.Dy1 = Fix64.Mul(self._width, Fix64.Div(Fix64.Sub(v1.X, v0.X), len1));
        a.Dx2 = Fix64.Mul(self._width, Fix64.Div(Fix64.Sub(v2.Y, v1.Y), len2));
        a.Dy2 = Fix64.Mul(self._width, Fix64.Div(Fix64.Sub(v2.X, v1.X), len2));
        a.Cp = MathUtils.CrossProduct(v0.X, v0.Y, v1.X, v1.Y, v2.X, v2.Y);

        if (a.Cp != 0 && a.Cp > 0 == self._width > 0)
        {
            long limit = 0;
            if (self._widthAbs != 0) limit = Fix64.Div(len1 < len2 ? len1 : len2, self._widthAbs);

            if (limit < 4337916928 /* 1.01 */) limit = 4337916928 /* 1.01 */;

            CalcMiter(self,
                new CalcMiterMethodArgs(v0, v1, v2, a.Dx1, a.Dy1, a.Dx2, a.Dy2,
                    LineJoin.MiterRevert,
                    limit, 0));
        }
        else
        {
            a.Dx = Fix64.Div(Fix64.Add(a.Dx1, a.Dx2), Fix64.Two);
            a.Dy = Fix64.Div(Fix64.Add(a.Dy1, a.Dy2), Fix64.Two);
            a.BevelDistance = Trig256.Sqrt(Fix64.Add(Fix64.Mul(a.Dx, a.Dx), Fix64.Mul(a.Dy, a.Dy)));

            if (self.LineJoin is LineJoin.Round or LineJoin.Bevel)
                if (Fix64.Mul(Fix64.One, Fix64.Sub(self._widthAbs, a.BevelDistance)) < self._widthEps)
                {
                    (a.Dx, a.Dy, var intersects) = MathUtils.CalcIntersection(
                        Fix64.Add(v0.X, a.Dx1),
                        Fix64.Sub(v0.Y, a.Dy1),
                        Fix64.Add(v1.X, a.Dx1),
                        Fix64.Sub(v1.Y, a.Dy1),
                        Fix64.Add(v1.X, a.Dx2),
                        Fix64.Sub(v1.Y, a.Dy2),
                        Fix64.Add(v2.X, a.Dx2),
                        Fix64.Sub(v2.Y, a.Dy2));

                    if (intersects)
                        self._outVertices[self._outVerticesCount++] = new Vector2(a.Dx, a.Dy);
                    else
                        self._outVertices[self._outVerticesCount++] =
                            new Vector2(Fix64.Add(v1.X, a.Dx1), Fix64.Sub(v1.Y, a.Dy1));

                    return;
                }

            if (self.LineJoin is LineJoin.Miter or LineJoin.MiterRevert or LineJoin.MiterRound)
            {
                CalcMiter(self,
                    new CalcMiterMethodArgs(
                        v0, v1, v2, a.Dx1, a.Dy1, a.Dx2, a.Dy2,
                        self.LineJoin,
                        17179869184 /* 4 */,
                        a.BevelDistance
                    ));
            }
            else if (self.LineJoin == LineJoin.Round)
            {
                CalcArc(self, new CalcArcArgs(v1.X, v1.Y, a.Dx1, -a.Dy1, a.Dx2, -a.Dy2));
            }
            else if (self.LineJoin == LineJoin.Bevel)
            {
                self._outVertices[self._outVerticesCount++] =
                    new Vector2(Fix64.Add(v1.X, a.Dx1), Fix64.Sub(v1.Y, a.Dy1));
                self._outVertices[self._outVerticesCount++] =
                    new Vector2(Fix64.Add(v1.X, a.Dx2), Fix64.Sub(v1.Y, a.Dy2));
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }


    private static void CalcArc(Stroke self, CalcArcArgs f)
    {
        var a1 = Trig256.Atan2(
            Fix64.Mul(f.Dy1, self._widthSign),
            Fix64.Mul(f.Dx1, self._widthSign)
        );

        var a2 = Trig256.Atan2(
            Fix64.Mul(f.Dy2, self._widthSign),
            Fix64.Mul(f.Dx2, self._widthSign)
        );

        int n;

        var da = Fix64.Mul(
            Trig256.Acos(Fix64.Div(self._widthAbs,
                Fix64.Add(self._widthAbs, Fix64.Div(536870912 /* 0.125 */, Fix64.One)))),
            Fix64.Two);

        self._outVertices[self._outVerticesCount++] = new Vector2(Fix64.Add(f.X, f.Dx1), Fix64.Add(f.Y, f.Dy1));

        if (self._widthSign > 0)
        {
            if (a1 > a2) a2 = Fix64.Add(a2, Fix64.TwoPi);

            var t1 = Fix64.Div(Fix64.Sub(a2, a1), da);
            n = (int) (t1 / Fix64.One);

            da = Fix64.Div(Fix64.Sub(a2, a1), (n + 1) * Fix64.One);
            a1 = Fix64.Add(a1, da);

            for (var i = 0; i < n; i++)
            {
                var vx = Fix64.Add(f.X, Fix64.Mul(Trig256.Cos(a1), self._width));
                var vy = Fix64.Add(f.Y, Fix64.Mul(Trig256.Sin(a1), self._width));
                self._outVertices[self._outVerticesCount++] = new Vector2(vx, vy);
                a1 = Fix64.Add(a1, da);
            }
        }
        else
        {
            if (a1 < a2) a2 = Fix64.Sub(a2, Fix64.TwoPi);

            var t1 = Fix64.Div(Fix64.Sub(a1, a2), da);
            n = (int) (t1 / Fix64.One);

            da = Fix64.Div(Fix64.Sub(a1, a2), (n + 1) * Fix64.One);
            a1 = Fix64.Sub(a1, da);

            for (var i = 0; i < n; i++)
            {
                var vx = Fix64.Add(f.X, Fix64.Mul(Trig256.Cos(a1), self._width));
                var vy = Fix64.Add(f.Y, Fix64.Mul(Trig256.Sin(a1), self._width));
                self._outVertices[self._outVerticesCount++] = new Vector2(vx, vy);
                a1 = Fix64.Sub(a1, da);
            }
        }

        self._outVertices[self._outVerticesCount++] = new Vector2(Fix64.Add(f.X, f.Dx2), Fix64.Add(f.Y, f.Dy2));
    }

    private static void CalcMiter(
        Stroke self,
        CalcMiterMethodArgs f)
    {
        CalcMiterArgs a;

        a.Di = Fix64.One;
        a.Lim = Fix64.Mul(self._widthAbs, f.MiterLimit);
        a.MiterLimitExceeded = true;
        a.IntersectionFailed = true;

        var (xi, yi, intersects) = MathUtils.CalcIntersection(
            Fix64.Add(f.V0.X, f.Dx1),
            Fix64.Sub(f.V0.Y, f.Dy1),
            Fix64.Add(f.V1.X, f.Dx1),
            Fix64.Sub(f.V1.Y, f.Dy1),
            Fix64.Add(f.V1.X, f.Dx2),
            Fix64.Sub(f.V1.Y, f.Dy2),
            Fix64.Add(f.V2.X, f.Dx2),
            Fix64.Sub(f.V2.Y, f.Dy2));

        if (intersects)
        {
            a.Di = MathUtils.CalcDistance(f.V1.X, f.V1.Y, xi, yi);

            if (a.Di <= a.Lim)
            {
                self._outVertices[self._outVerticesCount++] = new Vector2(xi, yi);
                a.MiterLimitExceeded = false;
            }

            a.IntersectionFailed = false;
        }
        else
        {
            var x2 = Fix64.Add(f.V1.X, f.Dx1);
            var y2 = Fix64.Sub(f.V1.Y, f.Dy1);

            if (MathUtils.CrossProduct(f.V0.X, f.V0.Y, f.V1.X, f.V1.Y, x2, y2) < 0 ==
                MathUtils.CrossProduct(f.V1.X, f.V1.Y, f.V2.X, f.V2.Y, x2, y2) < 0)
            {
                self._outVertices[self._outVerticesCount++] =
                    new Vector2(Fix64.Add(f.V1.X, f.Dx1), Fix64.Sub(f.V1.Y, f.Dy1));
                a.MiterLimitExceeded = false;
            }
        }

        if (!a.MiterLimitExceeded) return;

        {
            if (f.Lj == LineJoin.MiterRevert)
            {
                self._outVertices[self._outVerticesCount++] =
                    new Vector2(Fix64.Add(f.V1.X, f.Dx1), Fix64.Sub(f.V1.Y, f.Dy1));
                self._outVertices[self._outVerticesCount++] =
                    new Vector2(Fix64.Add(f.V1.X, f.Dx2), Fix64.Sub(f.V1.Y, f.Dy2));
            }
            else if (f.Lj == LineJoin.MiterRound)
            {
                CalcArc(self, new CalcArcArgs(f.V1.X, f.V1.Y, f.Dx1, -f.Dy1, f.Dx2, -f.Dy2));
            }
            else if (f.Lj == LineJoin.Miter)
            {
            }
            else if (f.Lj == LineJoin.Round)
            {
            }
            else if (f.Lj == LineJoin.Bevel)
            {
            }
            else
            {
                if (a.IntersectionFailed)
                {
                    f.MiterLimit = Fix64.Mul(f.MiterLimit, self._widthSign);

                    self._outVertices[self._outVerticesCount++] = new Vector2(
                        Fix64.Add(f.V1.X, Fix64.Add(f.Dx1, Fix64.Mul(f.Dy1, f.MiterLimit))),
                        Fix64.Sub(f.V1.Y, Fix64.Add(f.Dy1, Fix64.Mul(f.Dx1, f.MiterLimit)))
                    );

                    self._outVertices[self._outVerticesCount++] = new Vector2(
                        Fix64.Add(f.V1.X, Fix64.Sub(f.Dx2, Fix64.Mul(f.Dy2, f.MiterLimit))),
                        Fix64.Sub(f.V1.Y, Fix64.Sub(f.Dy2, Fix64.Mul(f.Dx2, f.MiterLimit)))
                    );
                }
                else
                {
                    var x1 = Fix64.Add(f.V1.X, f.Dx1);
                    var y1 = Fix64.Sub(f.V1.Y, f.Dy1);
                    var x2 = Fix64.Add(f.V1.X, f.Dx2);
                    var y2 = Fix64.Sub(f.V1.Y, f.Dy2);

                    a.Di = Fix64.Div(Fix64.Sub(a.Lim, f.DistanceBevel), Fix64.Sub(a.Di, f.DistanceBevel));

                    self._outVertices[self._outVerticesCount++] = new Vector2(
                        Fix64.Add(x1, Fix64.Mul(Fix64.Sub(xi, x1), a.Di)),
                        Fix64.Add(y1, Fix64.Mul(Fix64.Sub(yi, y1), a.Di))
                    );

                    self._outVertices[self._outVerticesCount++] = new Vector2(
                        Fix64.Add(x2, Fix64.Mul(Fix64.Sub(xi, x2), a.Di)),
                        Fix64.Add(y2, Fix64.Mul(Fix64.Sub(yi, y2), a.Di))
                    );
                }
            }
        }
    }

    internal VertexDistance Previous(int i)
    {
        return _distances[(i + _distanceCount - 1) % _distanceCount];
    }

    internal VertexDistance Current(int i)
    {
        return _distances[i];
    }

    internal VertexDistance Next(int i)
    {
        return _distances[(i + 1) % _distanceCount];
    }

    private void Clear()
    {
        _distanceCount = 0;
        _closed = false;
        _status = StrokeStatus.Initial;
    }

    private void Rewind()
    {
        if (_status == StrokeStatus.Initial)
        {
            Close(_closed);
            if (_distanceCount < 3) _closed = false;
        }

        _status = StrokeStatus.Ready;
        _srcVertex = 0;
        _outVertexCount = 0;
    }

    public void Add(VertexDistance value)
    {
        if (_distanceCount > 1)
            if (!_distances[_distanceCount - 2].IsEqual(_distances[_distanceCount - 1]))
                RemoveLast();
        _distances[_distanceCount++] = value;
    }

    public void ModifyLast(VertexDistance value)
    {
        RemoveLast();
        Add(value);
    }

    public void Close(bool isClosed)
    {
        while (_distanceCount > 1)
        {
            if (_distances[_distanceCount - 2].IsEqual(_distances[_distanceCount - 1])) break;
            var t = _distances[_distanceCount - 1];
            RemoveLast();
            ModifyLast(t);
        }

        if (isClosed)
            while (_distanceCount > 1)
            {
                if (_distances[_distanceCount - 1].IsEqual(_distances[0])) break;
                RemoveLast();
            }
    }

    private void RemoveLast()
    {
        if (_distanceCount != 0) _distanceCount--;
    }

    private struct CalcCapArgs
    {
        public uint VertexCount;
        public long Dx1;
        public long Dy1;
        public long Dx2;
        public long Dy2;
        public long Da;
        public long A1;
        public int I;
        public int N;
    }

    private struct CalcJoinArgs
    {
        public long Dx1;
        public long Dy1;
        public long Dx2;
        public long Dy2;
        public long Cp;
        public long Dx;
        public long Dy;
        public long BevelDistance;
        public bool Intersects;
    }

    private struct CalcArcArgs
    {
        public readonly long X;
        public readonly long Y;
        public readonly long Dx1;
        public readonly long Dy1;
        public readonly long Dx2;
        public readonly long Dy2;

        public CalcArcArgs(long x, long y, long dx1, long dy1, long dx2, long dy2)
        {
            X = x;
            Y = y;
            Dx1 = dx1;
            Dy1 = dy1;
            Dx2 = dx2;
            Dy2 = dy2;
        }
    }

    private struct CalcMiterMethodArgs
    {
        public readonly VertexDistance V0;
        public readonly VertexDistance V1;
        public readonly VertexDistance V2;
        public readonly long Dx1;
        public readonly long Dy1;
        public readonly long Dx2;
        public readonly long Dy2;
        public readonly LineJoin Lj;
        public long MiterLimit;
        public readonly long DistanceBevel;

        public CalcMiterMethodArgs(VertexDistance v0, VertexDistance v1, VertexDistance v2, long dx1, long dy1,
            long dx2,
            long dy2, LineJoin lj, long miterLimit, long distanceBevel)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            Dx1 = dx1;
            Dy1 = dy1;
            Dx2 = dx2;
            Dy2 = dy2;
            Lj = lj;
            MiterLimit = miterLimit;
            DistanceBevel = distanceBevel;
        }
    }

    private struct CalcMiterArgs
    {
        public long Di;
        public long Lim;
        public bool MiterLimitExceeded;
        public bool IntersectionFailed;

        public CalcMiterArgs(long di, long lim, bool miterLimitExceeded, bool intersectionFailed)
        {
            Di = di;
            Lim = lim;
            MiterLimitExceeded = miterLimitExceeded;
            IntersectionFailed = intersectionFailed;
        }
    }
}