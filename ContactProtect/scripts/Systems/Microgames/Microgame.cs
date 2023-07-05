using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ContactProtect.MicrogameStructure;

public abstract class Microgame
{
    public abstract MicrogameDetails MicrogameDetails { get; }
    
    public MicroWinState WinState { get; private set; } = MicroWinState.None;

    // TODO: Implement MicrogamePlayer, and pass it into the initializer and store in the microgame
    public Microgame() { }

    public abstract void Initialize();
    public abstract void LoadContent();
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

    public void Win()
    {
        WinState = MicroWinState.Win;
    }

    public void Lose()
    {
        WinState = MicroWinState.Lose;
    }
    
    public virtual void OnTimeTick(int tickIndex) { }

    /// <summary>
    /// Called automatically when Win is called on the microgame. 
    /// </summary>
    /// <remarks>Note, it is NOT called when the microgame ends and OnTimeUp returns true. Only when you call Win()</remarks>
    public virtual void OnWin()
    {
        // TODO: Call system for "Host" to do their win jingle
    }

    /// <summary>
    /// Called automatically when Lose is called on the microgame. 
    /// </summary>
    /// <remarks>Note, it is NOT called when the microgame ends and OnTimeUp returns false. Only when you call Lose()</remarks>
    public virtual void OnLose()
    {
        // TODO: Call system for "Host" to do their lose jingle
    }
    
    /// <summary>
    /// Called when the microgame timer has ended, and returns a bool that represents whether or not the microgame was won or not.
    /// </summary>
    /// <remarks>
    /// Only override if you intend to set the win condition at the very last moment.
    /// </remarks>
    public virtual bool OnTimeUp()
    {
        return WinState == MicroWinState.Win;
    }
}