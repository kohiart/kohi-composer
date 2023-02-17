// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public struct Vector2 : IEquatable<Vector2>
{
    public long X { get; set; }
    public long Y { get; set; }

    public Vector2(long x, long y)
    {
        X = x;
        Y = y;
    }

    public static Vector2 operator +(Vector2 left, Vector2 right)
    {
        left.X += right.X;
        left.Y += right.Y;
        return left;
    }

    public static Vector2 operator -(Vector2 left, Vector2 right)
    {
        left.X -= right.X;
        left.Y -= right.Y;
        return left;
    }

    public override int GetHashCode()
    {
        return new {_x = X, _y = Y}.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2 vector2 && Equals(vector2);
    }

    public bool Equals(Vector2 other)
    {
        return
            X == other.X &&
            Y == other.Y;
    }

    public static bool operator ==(Vector2 left, Vector2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2 left, Vector2 right)
    {
        return !(left == right);
    }
}