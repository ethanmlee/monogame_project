using System;

namespace VoxelEngine.Scripts;

public static class Extensions
{
    public static int FloorToInt(this float f)
    {
        return (int)MathF.Floor(f);
    }
    
    public static int RoundToInt(this float f)
    {
        return (int)MathF.Round(f);
    }
}