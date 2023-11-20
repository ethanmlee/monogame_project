using System;
using Microsoft.Xna.Framework;

namespace VoxelEngine.Scripts;

public static class Vector3Utils
{
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