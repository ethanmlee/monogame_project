namespace VoxelEngine.Scripts;

using System;

public class SeamlessSimplexNoise
{
    private int tileSize;
    private int seed;

    public SeamlessSimplexNoise(int tileSize, int seed)
    {
        this.tileSize = tileSize;
        this.seed = seed;
    }

    public double GetData(float x, float y, float scale)
    {
        // Noise range
        float x1 = 0, x2 = 2;
        float y1 = 0, y2 = 2;
        float dx = x2 - x1;
        float dy = y2 - y1;

        // Sample noise at smaller intervals
        float s = x / tileSize;
        float t = y / tileSize;

        // Calculate our 4D coordinates
        float nx = x1 + MathF.Cos(s * 2 * MathF.PI) * (dx / (2 * MathF.PI));
        float ny = y1 + MathF.Cos(t * 2 * MathF.PI) * (dy / (2 * MathF.PI));
        float nz = x1 + MathF.Sin(s * 2 * MathF.PI) * (dx / (2 * MathF.PI));
        float nw = y1 + MathF.Sin(t * 2 * MathF.PI) * (dy / (2 * MathF.PI));
        
        // Apply scale
        float scaleBase = (tileSize / 128f) * scale;
        nx *= scaleBase;
        ny *= scaleBase;
        nz *= scaleBase;
        nw *= scaleBase;

        double heightValue = OpenSimplex2S.Noise4_ImproveXYZ_ImproveXZ(seed, nx, ny, nz, nw);

        return heightValue;
    }
}

