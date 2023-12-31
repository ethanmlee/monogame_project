using System;
using Microsoft.Xna.Framework;

namespace VoxelEngine.Scripts;

public static class Extensions
{
    public static int FloorToInt(this float f)
    {
        return (int)MathF.Floor(f);
    }
    
    public static int RoundToInt(this float f)
    {
        return (int)MathF.Round(f);
    }
    
    public static Vector3 XY(this Vector3 vector)
    {
        return new Vector3(vector.X, vector.Y, 0.0f);
    }

    public static Vector3 XZ(this Vector3 vector)
    {
        return new Vector3(vector.X, 0.0f, vector.Z);
    }

    public static Vector3 YZ(this Vector3 vector)
    {
        return new Vector3(0.0f, vector.Y, vector.Z);
    }
    
    public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
    {
        if (maxLength < 0.0f)
            throw new ArgumentException("maxLength must be non-negative.");

        float sqrMagnitude = vector.LengthSquared();
        if (sqrMagnitude > maxLength * maxLength)
        {
            float scaleFactor = maxLength / (float)Math.Sqrt(sqrMagnitude);
            return new Vector3(vector.X * scaleFactor, vector.Y * scaleFactor, vector.Z * scaleFactor);
        }
        else
        {
            return vector;
        }
    }
}