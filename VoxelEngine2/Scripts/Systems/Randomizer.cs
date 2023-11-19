using System;

namespace VoxelEngine.Scripts.Systems;

public class Randomizer
{
    private static Random _main;

    public static Random Main => _main ??= new Random();

    public static double Range(double min, double max)
    {
        return (Main.NextDouble() * (max - min) + min);
    }
    
    public static int Range(int min, int max)
    {
        return Main.Next(min, max);
    }
}