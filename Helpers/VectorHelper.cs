using CounterStrikeSharp.API.Modules.Utils;

namespace Katrox;

public static class VectorHelper
{
    public static Vector ToCSVector(this System.Numerics.Vector3 vector)
    {
        return new Vector(vector.X, vector.Y, vector.Z);
    }

    public static System.Numerics.Vector3 ToVector3(this Vector vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static Vector Change(this Vector vector, Vector other)
    {
        vector.X = other.X;
        vector.Y = other.Y;
        vector.Z = other.Z;
        return vector;
    }

    public static Vector Clone(this Vector vector)
    {
        var vec = new Vector
        {
            X = vector.X,
            Y = vector.Y,
            Z = vector.Z
        };
        return vec;
    }

    public static Vector Add(this Vector vector, Vector other)
    {
        vector.X += other.X;
        vector.Y += other.Y;
        vector.Z += other.Z;
        return vector;
    }

    public static Vector Scale(this Vector vector, float scale)
    {
        vector.X *= scale;
        vector.Y *= scale;
        vector.Z *= scale;
        return vector;
    }

    public static Vector Normalize(this Vector vector)
    {
        var length = vector.Length();
        vector.X /= length;
        vector.Y /= length;
        vector.Z /= length;
        return vector;
    }

    public static float Distance(this Vector vector, Vector other)
    {
        return (float)Math.Sqrt(vector.DistanceSquared(other));
    }

    public static float DistanceSquared(this Vector vector, Vector other)
    {
        return (float)(
            Math.Pow(vector.X - other.X, 2) +
            Math.Pow(vector.Y - other.Y, 2) +
            Math.Pow(vector.Z - other.Z, 2)
        );
    }
}