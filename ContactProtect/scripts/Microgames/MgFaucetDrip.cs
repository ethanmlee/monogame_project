using ContactProtect.MicrogameStructure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ContactProtect.scripts.Microgames;

public class MgFaucetDrip : Microgame
{
    public override MicrogameDetails MicrogameDetails => new MicrogameDetails(
        "Faucet Drip",
        "Help the little water droplet fall out of the faucet!", 
        "Drip!", 
        MicroDuration.Normal, 
        true, 
        "",
        Color.Aquamarine, 
        Game1.OpenSpace);

    private Texture2D _pixel;
    
    public override void Initialize()
    {
        
    }

    public override void LoadContent()
    {
        _pixel = Game1.ContentManager.Load<Texture2D>("Textures/Pixel");
    }

    public override void Update(GameTime gameTime)
    {
        
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game1.Graphics.GraphicsDevice.Clear(Color.Blue);
        spriteBatch.Draw(_pixel, Vector2.One * 64, Color.Red);
    }
}