using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEngine.Scripts.Systems;

public class OrientationArrows
{
    private GraphicsDevice GraphicsDevice { get; set; }
    private BasicEffect BasicEffect { get; set; }

    public OrientationArrows(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        BasicEffect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            Projection = Game1.ProjectionMatrix,
            View = Game1.ViewMatrix
        };
    }

    public void Draw()
    {
        BasicEffect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            Projection = Game1.ProjectionMatrix,
            View = Game1.ViewMatrix
        };
        
        // Start rendering with the BasicEffect
        BasicEffect.CurrentTechnique.Passes[0].Apply();

        // Draw the arrows
        DrawArrow(Game1.CamPos + Game1.CamRotationMatrix.Forward * 0.5f, Vector3.UnitX, Color.Red);    // X-axis arrow
        DrawArrow(Game1.CamPos + Game1.CamRotationMatrix.Forward * 0.5f, Vector3.UnitY, Color.Green);  // Y-axis arrow
        DrawArrow(Game1.CamPos + Game1.CamRotationMatrix.Forward * 0.5f, Vector3.UnitZ, Color.Blue);   // Z-axis arrow
    }
    
    private void DrawArrow(Vector3 origin, Vector3 direction, Color color)
    {
        // Normalize the direction vector to get the arrow direction
        direction.Normalize();
        direction *= 0.02f;

        // Draw the arrow body (line)
        VertexPositionColor[] vertices =
        {
            new VertexPositionColor(origin, color),
            new VertexPositionColor(origin + direction, color)
        };

        GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);

    }
}