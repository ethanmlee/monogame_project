using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MusicGame;

///A platform intended for collisions from physics bodies.
public class Platform
{
    //scope otherparaeters Type NameOfProperty
    public static float Thickness { get; set; } = 2f;

    public Vector2 Position { get; set; }

    ///Angle of line in radians
    public float Angle { get; set; }
    public float Length { get; set; }
    
    public Platform(Vector2 position, float angle, float length)
    {
        Position = position;
        Angle = angle;
        Length = length;
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        // draw size of Thickness
        spriteBatch.Draw(Game1.Pixel, destinationRectangle: new Rectangle((int)Position.X, (int)Position.Y, 100, 30), Color.White);
        
        int size = (int)Math.Round(Position.X);
        size = 5;
    }
}
