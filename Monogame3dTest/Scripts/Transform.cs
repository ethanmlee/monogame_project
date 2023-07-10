using Microsoft.Xna.Framework;

namespace Monogame3dTest.Scripts;

public class Transform
{
    public Transform Parent { get; set; } = null;
    public Vector3 Position { get; set; } = new Vector3();
    public Vector3 EulerAngles { get; set; } = new Vector3();
    public Vector3 Scale { get; set; } = Vector3.One;
    
    public Matrix GetTransformMatrix()
    {
        Matrix startMatrix = Matrix.Identity;
        if (Parent != null) startMatrix = Parent.GetTransformMatrix();
        return startMatrix * Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(EulerAngles.X, EulerAngles.Y, EulerAngles.Z) * Matrix.CreateTranslation(Position);
    }
}