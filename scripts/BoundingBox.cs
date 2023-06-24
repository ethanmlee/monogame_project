using Microsoft.Xna.Framework;

namespace monogame_project;

public class BoundingBox
{
    public Vector2 Position;
    public Vector2 Offset;
    public Vector2 Size;

    public Vector2 PosOffset => Position + Offset;

    public BoundingBox(Vector2 offset, Vector2 size)
    {
        Position = Vector2.Zero;
        Offset = offset;
        Size = size;
    }
    
    public bool IsIntersecting(BoundingBox other)
    {
        return (PosOffset.X < other.PosOffset.X + other.Size.X &&
                PosOffset.X + Size.X > other.PosOffset.X &&
                PosOffset.Y < other.PosOffset.Y + other.Size.Y &&
                PosOffset.Y + Size.Y > other.PosOffset.Y);
    }
}