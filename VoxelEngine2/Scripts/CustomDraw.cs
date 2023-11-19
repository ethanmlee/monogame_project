using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEngine.Scripts;

public static class CustomDraw
{
    private static BasicEffect _basicEffect;
    private static VertexPositionColor[] _gridVertices;
    private static bool _isInitialized = false;
    
    public static void DrawGrid(GraphicsDevice graphicsDevice, int gridSize, float cellSize, Matrix world)
    {
        if (!_isInitialized)
        {
            _basicEffect = new BasicEffect(graphicsDevice);
            _basicEffect.VertexColorEnabled = true;

            Color color = new Color(1f, 1f, 1f, 0.1f) * 0.1f;
            
            // Create grid vertices
            _gridVertices = new VertexPositionColor[(gridSize + 1) * 4];
            for (int i = 0; i <= gridSize; i++)
            {
                float position = i * cellSize;

                // Horizontal lines
                _gridVertices[i * 4] = new VertexPositionColor(new Vector3(0, position, 0), color);
                _gridVertices[i * 4 + 1] = new VertexPositionColor(new Vector3(gridSize * cellSize, position, 0), color);

                // Vertical lines
                _gridVertices[i * 4 + 2] = new VertexPositionColor(new Vector3(position, 0, 0), color);
                _gridVertices[i * 4 + 3] = new VertexPositionColor(new Vector3(position, gridSize * cellSize, 0), color);
            }
            
            _isInitialized = true;
        }

        _basicEffect.World = world;
        _basicEffect.View = Game1.ViewMatrix;
        _basicEffect.Projection = Game1.ProjectionMatrix;

        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _gridVertices, 0, (_gridVertices.Length / 2));
        }
    }
    
    public static void DrawQuad(GraphicsDevice graphicsDevice, Texture2D texture, Matrix worldMatrix, float width, float height, Vector2 pixelTopLeft, Vector2 pixelBottomRight)
    {
        Vector2 uvTopLeft = pixelTopLeft / texture.Bounds.Size.ToVector2();
        Vector2 uvBottomRight = pixelBottomRight / texture.Bounds.Size.ToVector2();
        // Define the vertices of the quad
        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(-width / 2, -height / 2, 0), uvTopLeft),
            new VertexPositionTexture(new Vector3(-width / 2, height / 2, 0), new Vector2(uvTopLeft.X, uvBottomRight.Y)),
            new VertexPositionTexture(new Vector3(width / 2, -height / 2, 0), new Vector2(uvBottomRight.X, uvTopLeft.Y)),
            new VertexPositionTexture(new Vector3(width / 2, height / 2, 0), uvBottomRight)
        };

        // Create the vertex buffer
        VertexBuffer vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices);

        // Set the vertex buffer and primitive type
        graphicsDevice.SetVertexBuffer(vertexBuffer);
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

        // Set up basic effect parameters
        var basicEffect = new BasicEffect(graphicsDevice);
        basicEffect.TextureEnabled = true;
        basicEffect.Texture = texture;
        basicEffect.World = worldMatrix;
        basicEffect.View = Game1.ViewMatrix;
        basicEffect.Projection = Game1.ProjectionMatrix;

        // Apply the basic effect
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            // Draw the quad
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }
}
