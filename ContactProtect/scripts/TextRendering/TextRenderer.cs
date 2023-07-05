using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ContactProtect.TextRendering;

public class TextRenderer
{
    public Texture2D Source { get; private set; }
    public string CharList { get; private set; }
    public Vector2 CharSize { get; private set; }
    public List<Rectangle> Atlas { get; private set; } = new List<Rectangle>();

    public static Dictionary<string, Texture2D> CachedFontTextures { get; private set; } =
        new Dictionary<string, Texture2D>();

    public const string CharListAlpha = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; 
    
    public TextRenderer(string sourceTexturePath, string charList = CharListAlpha, Vector2? charSize = null)
    {
        if (CachedFontTextures.TryGetValue(sourceTexturePath, out var cachedTex))
        {
            Source = cachedTex;
        }
        else
        {
            Source = Game1.ContentManager.Load<Texture2D>(sourceTexturePath);
            CachedFontTextures[sourceTexturePath] = Source;
        }

        CharList = charList;

        // If charSize was passed in
        if (charSize.HasValue)
        {
            CharSize = charSize.Value;
        }
        else
        {
            CharSize = new Vector2(100, 100);
        }

        for (int y = 0; y < Source.Height; y += (int)CharSize.Y)
        for (int x = 0; x < Source.Width; x += (int)CharSize.X)
        {
            Atlas.Add(new Rectangle(x, y, (int)CharSize.X, (int)CharSize.Y));
        }
    }

    public void Draw(string text, float height, SpriteBatch spriteBatch)
    {
        // Loop through all characters, finding the right rectangle
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            int charIndexInAtlas = CharList.IndexOf(c);
            Vector2 sizeToScale = Game1.SizeToScale(Vector2.One * CharSize.Y, Vector2.One * height);
            Vector2 scaledCharSize = CharSize * sizeToScale;
            spriteBatch.Draw(Source, Vector2.UnitX * i * scaledCharSize.X, Atlas[charIndexInAtlas], Color.White, 0f, Vector2.Zero, sizeToScale, SpriteEffects.None, 0);
        }
    }
}