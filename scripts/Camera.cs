using Microsoft.Xna.Framework;

namespace monogame_project;

public class Camera
{
    public Vector2 Position;
    public float Angle;
    public float Zoom;
    
    public Vector2 Origin => new Vector2(Game1.RenderResolution.X / 2, Game1.RenderResolution.Y / 2);

    public Camera(Vector2 position, float angle, float zoom)
    {
        Position = position;
        Angle = angle;
        Zoom = zoom;
    }

    public Matrix TransformationMatrix => 
        Matrix.Identity *
        Matrix.CreateTranslation(-Position.X, -Position.Y, 0) * 
        Matrix.CreateRotationZ(Angle) *
        Matrix.CreateScale(Game1.RenderResolution.Y / Zoom) * 
        Matrix.CreateTranslation(Origin.X, Origin.Y, 0);
}