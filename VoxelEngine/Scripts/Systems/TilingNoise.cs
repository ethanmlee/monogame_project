using System;
using LibNoise;
using LibNoise.Primitive;

namespace VoxelEngine.Scripts.Systems;

public static class TilingNoise
{
    private static readonly SimplexPerlin Perlin = new SimplexPerlin(1337, NoiseQuality.Best);
    public static float GetNoiseWorld(int x, int y, float scale)
    {
        return GetNoise(x, y, VoxelData.WorldPlanarSizeChunks * VoxelData.chunkSize.X, VoxelData.WorldPlanarSizeChunks * scale);
    }
    public static float GetNoise(int x, int y, float size, float scale)
    {
        float s = (float)x / size;
        float t = (float)y / size;
        
        // x2 and y1 ~= octave count (aka how detailed it is)
        float x1 = 0, x2 = scale;
        float y1 = 0, y2 = scale;
        float dx = x2 - x1;
        float dy = y2 - y1;

        float nx = x1 + MathF.Cos(s * 2 * MathF.PI) * dx / (2 * MathF.PI);
        float ny = y1 + MathF.Cos(t * 2 * MathF.PI) * dy / (2 * MathF.PI);
        float nz = x1 + MathF.Sin(s * 2 * MathF.PI) * dx / (2 * MathF.PI);
        float nw = y1 + MathF.Sin(t * 2 * MathF.PI) * dy / (2 * MathF.PI);
        
        return Perlin.GetValue(nx, ny, nz, nw); 
    }
}