using Microsoft.Xna.Framework;

namespace monogame_project;

public class Camera
{
    public Vector2 Position;
    public float Angle;
    public float Zoom;

    public Camera(Vector2 position, float angle, float zoom)
    {
        Position = position;
        Angle = angle;
        Zoom = zoom;
    }

    public Matrix TransformationMatrix => 
        Matrix.Identity *
        Matrix.CreateTranslation(Position.X, Position.Y, 0) * 
        Matrix.CreateRotationZ(Angle) *
        // Matrix.CreateTranslation(Game1.RenderResolution.X * 0.5f, Game1.RenderResolution.Y * 0.5f, 0) * 
        Matrix.CreateScale(Game1.RenderResolution.Y / Zoom);
}