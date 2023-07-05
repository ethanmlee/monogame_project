using Microsoft.Xna.Framework;

namespace ContactProtect.MicrogameStructure;

public struct MicrogameDetails
{
    public MicrogameDetails(string name, string description, string caption, MicroDuration duration, bool hostOnWinLose, string musicName, Color backgroundColor, Rectangle frameRectangle)
    {
        Name = name;
        Description = description;
        Caption = caption;
        Duration = duration;
        HostOnWinLose = hostOnWinLose;
        MusicName = musicName;
        BackgroundColor = backgroundColor;
        FrameRectangle = frameRectangle;
    }

    public static MicrogameDetails CreateInstance(string name = "No Name", string description = "No Desc", string caption = "No Caption", MicroDuration duration = MicroDuration.Normal, bool hostOnWinLose = true, string musicName = "", Color backgroundColor = default, Rectangle frameRectangle = new Rectangle())
    {
        return new MicrogameDetails(name, description, caption, duration, hostOnWinLose, musicName, backgroundColor, frameRectangle);
    }

    public string Name { get; }
    public string Description { get; }
    public string Caption { get; }

    public MicroDuration Duration { get; }
    public bool HostOnWinLose { get; }
    public string MusicName { get; }
    public Color BackgroundColor { get; }
    public Rectangle FrameRectangle { get; }
}