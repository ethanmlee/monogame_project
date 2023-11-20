namespace VoxelEngine.Scripts.Systems;

using System;
using Microsoft.Xna.Framework;

public struct Vector3Int : IEquatable<Vector3Int>, IEquatable<Vector3>
{
    private Vector3 _internalVector;

    public int X
    {
        get => (int)_internalVector.X;
        set => _internalVector.X = value;
    }

    public int Y
    {
        get => (int)_internalVector.Y;
        set => _internalVector.Y = value;
    }

    public int Z
    {
        get => (int)_internalVector.Z;
        set => _internalVector.Z = value;
    }

    public Vector3Int(int x, int y, int z)
    {
        _internalVector = new Microsoft.Xna.Framework.Vector3(x, y, z);
    }

    public Vector3Int(Vector3 vector3)
    {
        _internalVector = new Vector3((int)vector3.X, (int)vector3.Y, (int)vector3.Z);
    }

    public static Vector3Int operator +(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3Int operator -(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3Int operator *(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    public static Vector3Int operator /(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }
    
    public static Vector3Int operator %(Vector3Int a, Vector3Int b)
    {
        Vector3Int finalValue = new Vector3Int(a.X % b.X, a.Y % b.Y, a.Z % b.Z);
        if (finalValue.X < 0) finalValue.X += b.X;
        if (finalValue.Y < 0) finalValue.Y += b.Y;
        if (finalValue.Z < 0) finalValue.Z += b.Z;
        return finalValue;
    }

    public static Vector3 operator +(Vector3Int a, Vector3 b)
    {
        return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3 operator -(Vector3Int a, Vector3 b)
    {
        return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3 operator *(Vector3Int a, Vector3 b)
    {
        return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    public static Vector3 operator /(Vector3Int a, Vector3 b)
    {
        return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }

    public static Vector3 operator +(Vector3 a, Vector3Int b)
    {
        return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3 operator -(Vector3 a, Vector3Int b)
    {
        return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3 operator *(Vector3 a, Vector3Int b)
    {
        return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    public static Vector3 operator /(Vector3 a, Vector3Int b)
    {
        return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public override int GetHashCode()
    {
        return _internalVector.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is Vector3Int vector3Int && Equals(vector3Int);
    }

    public bool Equals(Vector3Int other)
    {
        return _internalVector.Equals(other._internalVector);
    }
    
    public bool Equals(Vector3 other)
    {
        return _internalVector.Equals(other);
    }
    
    public static implicit operator Vector3(Vector3Int a)
    {
        return new Vector3(a.X, a.Y, a.Z);
    }
}
