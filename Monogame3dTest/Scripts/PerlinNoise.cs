using System;

public static class PerlinNoiseGenerator
{
    public static float GenerateNoise(float x, float y, float scale)
    {
        int xi = (int)Math.Floor(x / scale);
        int yi = (int)Math.Floor(y / scale);

        float xf = x / scale - xi;
        float yf = y / scale - yi;

        float u = Fade(xf);
        float v = Fade(yf);

        float n00 = Grad(xi, yi, xf, yf);
        float n10 = Grad(xi + 1, yi, xf - 1, yf);
        float n01 = Grad(xi, yi + 1, xf, yf - 1);
        float n11 = Grad(xi + 1, yi + 1, xf - 1, yf - 1);

        float x0 = Lerp(n00, n10, u);
        float x1 = Lerp(n01, n11, u);

        return Lerp(x0, x1, v);
    }

    private static float Grad(int xi, int yi, float xf, float yf)
    {
        float dx = xf;
        float dy = yf;

        // Generate random gradients using a hash function
        int hash = xi * 251 + yi * 631;
        hash = (hash << 13) ^ hash;

        float random = (1.0f - ((hash * (hash * hash * 15731 + 789221) + 1376312589) & 0x7FFFFFFF) / 1073741824.0f);
        float theta = random * 2.0f * MathF.PI;

        float cosTheta = MathF.Cos(theta);
        float sinTheta = MathF.Sin(theta);

        return dx * cosTheta + dy * sinTheta;
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
}