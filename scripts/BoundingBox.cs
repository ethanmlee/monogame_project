using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PixelOven.Debug;

namespace monogame_project;

public class BoundingBox
{
    public Entity Entity;
    public Vector2 Position => Entity.Position;
    public Vector2 Offset;
    public Vector2 Size;

    public Vector2 PosOffset => Position + Offset;
    
    public Color DebugColor = Color.Red;
    public int Width => (int)Size.X;
    public int Height => (int)Size.Y;
    
    public BoundingBox(Entity entity, Vector2 offset, Vector2 size)
    {
        Entity = entity;
        Offset = offset;
        Size = size;
    }

    public int Top => (int)PosOffset.Y;

    public int Bottom => (int)PosOffset.Y + Height - 1;

    public int Left => (int)PosOffset.X;

    public int Right => (int)PosOffset.X + Width - 1;

    public Vector2 Center => new Vector2(PosOffset.X + Width / 2f, PosOffset.Y + Height / 2f);

    public Vector2 BottomCenter => new Vector2(PosOffset.X + Width / 2f, Bottom);

    public Rectangle Bounds => new Rectangle(x:(int)PosOffset.X, y:(int)PosOffset.Y, width:(int)Width, height:(int)Height);
    
    private static readonly Dictionary<string, Texture2D> RectangleTextures = new Dictionary<string, Texture2D>();
    private Texture2D _rectangleTexture;
    private Texture2D RectangleTexture
    {
        get
        {
            if (_rectangleTexture != null && Width == _rectangleTexture.Width && Height == _rectangleTexture.Height)
                return _rectangleTexture;
                
            string sizeKey = string.Concat(Width, "x", Height);
            if (RectangleTextures.ContainsKey(sizeKey))
            {
                _rectangleTexture = RectangleTextures[sizeKey];
            }
            else
            {
                List<Color> colors = new List<Color>();
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                        {
                            colors.Add(Color.White);
                        }
                        else
                        {
                            colors.Add(Color.Transparent);
                        }
                    }
                }
                _rectangleTexture?.Dispose();
                _rectangleTexture = new Texture2D(Game1.Graphics.GraphicsDevice, Width, Height);
                _rectangleTexture.SetData(colors.ToArray());
                RectangleTextures.Add(sizeKey, _rectangleTexture);
            }

            return _rectangleTexture;
        }
    }
    
    public bool IsIntersecting(BoundingBox other)
    {
        return (PosOffset.X < other.PosOffset.X + other.Width &&
                PosOffset.X + Width > other.PosOffset.X &&
                PosOffset.Y < other.PosOffset.Y + other.Height &&
                PosOffset.Y + Height > other.PosOffset.Y);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!DebugManager.ShowCollisionRectangles) return;
        DebugManager.DrawSprite(PosOffset, RectangleTexture, DebugColor);
        DebugManager.DrawText(PosOffset, Game1.ContentManager.Load<SpriteFont>("Fonts/m3x6"), Width + "x" + Height, Color.White);
        // spriteBatch.Draw(RectangleTexture, new Vector2(PosOffset.X, PosOffset.Y), null, DebugColor, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
    }
}