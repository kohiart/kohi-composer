// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public struct Matrix
{
    public long Sx { get; set; }
    public long Shy { get; set; }
    public long Shx { get; set; }
    public long Sy { get; set; }
    public long Tx { get; set; }
    public long Ty { get; set; }

    public Matrix(Matrix copy)
    {
        Sx = copy.Sx;
        Shy = copy.Shy;
        Shx = copy.Shx;
        Sy = copy.Sy;
        Tx = copy.Tx;
        Ty = copy.Ty;
    }

    public Matrix(long v0, long v1, long v2,
        long v3, long v4, long v5)
    {
        Sx = v0;
        Shy = v1;
        Shx = v2;
        Sy = v3;
        Tx = v4;
        Ty = v5;
    }

    public static Matrix NewIdentity()
    {
        var newAffine = new Matrix();
        newAffine.Sx = Fix64.One;
        newAffine.Shy = 0;
        newAffine.Shx = 0;
        newAffine.Sy = Fix64.One;
        newAffine.Tx = 0;
        newAffine.Ty = 0;

        return newAffine;
    }

    public static Matrix NewRotation(long radians)
    {
        var v0 = Trig256.Cos(radians);
        var v1 = Trig256.Sin(radians);
        var v2 = -Trig256.Sin(radians);
        var v3 = Trig256.Cos(radians);

        return new Matrix(v0, v1, v2, v3, 0, 0);
    }

    public static Matrix NewScale(long scale)
    {
        return new Matrix(scale, 0, 0, scale, 0, 0);
    }

    public static Matrix NewScale(long scaleX, long scaleY)
    {
        return new Matrix(scaleX, 0, 0, scaleY, 0, 0);
    }

    public static Matrix NewTranslation(long x, long y)
    {
        return new Matrix(Fix64.One, 0, 0, Fix64.One, x, y);
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {
        return Mul(new Matrix(a), b);
    }

    public static Matrix operator +(Matrix a, Vector2 b)
    {
        var temp = new Matrix(a);
        temp.Tx += b.X;
        temp.Ty += b.Y;
        return temp;
    }

    public void Transform(ref long x, ref long y)
    {
        var tmp = x;

        var t1 = Fix64.Mul(tmp, Sx);
        var t2 = Fix64.Mul(y, Shx);
        x = Fix64.Add(t1, Fix64.Add(t2, Tx));

        var t3 = Fix64.Mul(tmp, Shy);
        var t4 = Fix64.Mul(y, Sy);
        y = Fix64.Add(t3, Fix64.Add(t4, Ty));
    }

    public Vector2 Transform(Vector2 value)
    {
        var x = value.X;
        var y = value.Y;
        Transform(ref x, ref y);
        value.X = x;
        value.Y = y;
        return value;
    }

    public void Invert()
    {
        var d = Fix64.Div(Fix64.One, Fix64.Sub(Fix64.Mul(Sx, Sy), Fix64.Mul(Shy, Shx)));
        var t0 = Fix64.Mul(Sy, d);

        Sy = Fix64.Mul(Sx, d);
        Shy = Fix64.Mul(-Shy, d);
        Shx = Fix64.Mul(-Shx, d);

        var t1 = Fix64.Sub(Fix64.Mul(-Tx, t0), Fix64.Mul(Ty, Shx));
        var t2 = Fix64.Sub(Fix64.Mul(-Tx, Shy), Fix64.Mul(Ty, Sy));

        Ty = t2;
        Sx = t0;
        Tx = t1;
    }

    public bool IsIdentity()
    {
        return IsEqual(Sx, Fix64.One, MathUtils.Epsilon) &&
               IsEqual(Shy, 0, MathUtils.Epsilon) &&
               IsEqual(Shx, 0, MathUtils.Epsilon) &&
               IsEqual(Sy, Fix64.One, MathUtils.Epsilon) &&
               IsEqual(Tx, 0, MathUtils.Epsilon) &&
               IsEqual(Ty, 0, MathUtils.Epsilon);
    }

    public static bool IsEqual(long v1, long v2, long epsilon)
    {
        return MathExtensions.Abs(Fix64.Sub(v1, v2)) <= epsilon;
    }

    public static Matrix Mul(Matrix self, Matrix other)
    {
        var t0 = Fix64.Add(Fix64.Mul(self.Sx, other.Sx), Fix64.Mul(self.Shy, other.Shx));
        var t1 = Fix64.Add(Fix64.Mul(self.Shx, other.Sx), Fix64.Mul(self.Sy, other.Shx));
        var t2 = Fix64.Add(Fix64.Mul(self.Tx, other.Sx), Fix64.Add(Fix64.Mul(self.Ty, other.Shx), other.Tx));
        var t3 = Fix64.Add(Fix64.Mul(self.Sx, other.Shy), Fix64.Mul(self.Shy, other.Sy));
        var t4 = Fix64.Add(Fix64.Mul(self.Shx, other.Shy), Fix64.Mul(self.Sy, other.Sy));
        var t5 = Fix64.Add(Fix64.Mul(self.Tx, other.Shy), Fix64.Add(Fix64.Mul(self.Ty, other.Sy), other.Ty));

        self.Shy = t3;
        self.Sy = t4;
        self.Ty = t5;
        self.Sx = t0;
        self.Shx = t1;
        self.Tx = t2;

        return self;
    }
}