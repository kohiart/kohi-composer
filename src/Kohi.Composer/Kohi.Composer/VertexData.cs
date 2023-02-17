// Copyright (c) Kohi Art Community, Inc.

using System.Diagnostics;

namespace Kohi.Composer;

[DebuggerDisplay("{Command}({Position.X}, {position.Y})")]
public struct VertexData
{
    public Command Command { get; set; }
    public Vector2 Position { get; set; }

    public VertexData(Command command, Vector2 position)
    {
        Command = command;
        Position = position;
    }

    public VertexData(Command command, long x, long y)
        : this(command, new Vector2(x, y))
    {
    }
}