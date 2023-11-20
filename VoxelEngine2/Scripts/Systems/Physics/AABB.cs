using Microsoft.Xna.Framework;

namespace VoxelEngine2.Scripts.Systems.Physics;

public struct AABB
{
    public Vector3 Center;
    public Vector3 Size;

    public AABB(Vector3 center, Vector3 size)
    {
        Center = center;
        Size = size;
    }
    
    public float Right => (Size.X * 0.5f);
    public float Left => (Size.X * 0.5f);
    public float Top => (Size.Y * 0.5f);
    public float Bottom => (Size.Y * 0.5f);
    public float Front => (Size.Z * 0.5f);
    public float Back =>  (Size.Z * 0.5f);
}