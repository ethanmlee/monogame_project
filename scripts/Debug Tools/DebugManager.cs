using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelOven.Debug
{
    public static class DebugManager
    {

        public static bool DebugMode = false;
        
        private static readonly Queue<IDrawType> DrawCalls = new Queue<IDrawType>();
        private static List<string> _previousCommands = new List<string>();

        public static bool ShowCollisionRectangles = false;
        
        public static void DrawSprite(Vector2 position, Texture2D texture, Color color)
        {
            DrawTypeSprite drawType = new DrawTypeSprite(position, texture, color);
            DrawCalls.Enqueue(drawType);
        }

        public static void DrawText(Vector2 position, SpriteFont spriteFont, string text, Color color)
        {
            DrawTypeText drawType = new DrawTypeText(position, spriteFont, text, color);
            DrawCalls.Enqueue(drawType);
        }
        
        public static void Draw(SpriteBatch spriteBatch)
        {
            while (DrawCalls.Count > 0)
            {
                IDrawType drawType = DrawCalls.Dequeue();
                drawType.Draw(spriteBatch);
            }
        }

        private interface IDrawType
        {
            void Draw(SpriteBatch spriteBatch);
        }

        private struct DrawTypeSprite : IDrawType, IDisposable
        {
            private Vector2 _position;
            private Texture2D _texture;
            private Color _color;
            public DrawTypeSprite(Vector2 position, Texture2D texture, Color color)
            {
                _position = position;
                _texture = texture;
                _color = color;
            }

            void IDrawType.Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(_texture, _position, null, _color, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }

            public void Dispose()
            {
                _texture?.Dispose();
            }
        }

        private struct DrawTypeText : IDrawType, IDisposable
        {
            private Vector2 _position;
            private SpriteFont _font;
            private string _text;
            private Color _color;

            public DrawTypeText(Vector2 position, SpriteFont font, string text, Color color)
            {
                _position = position;
                _font = font;
                _text = text;
                _color = color;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.DrawString(_font, _text, _position, _color, 0, new Vector2(0, 12), 1, SpriteEffects.None, 0f);
            }

            public void Dispose()
            {
            }
        }
    }
}
