#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D SpriteTexture;

sampler2D _DitherPattern;
float _Colors;

float4 _MainTex_TexelSize;
float4 _DitherPattern_TexelSize;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float channelError(float col, float colMin, float colMax)
{
    float range = abs(colMin - colMax);
    float aRange = abs(col - colMin);
    return aRange /range;
}

float ditheredChannel(float error, float2 ditherBlockUV, float ditherSteps)
{
    error = floor(error * ditherSteps) / ditherSteps;
    float2 ditherUV = float2(error, 0);
    ditherUV.x += ditherBlockUV.x;
    ditherUV.y = ditherBlockUV.y;
    return tex2D(_DitherPattern, ditherUV).x;
}

float4 mix(float4 a, float4 b, float amt)
{
    return ((1.0 - amt) * a) + (b * amt);
}

/// YUV/RGB color space calculations

float4 RGBtoYUV(float4 rgba) {
    float4 yuva;
    yuva.r = rgba.r * 0.2126 + 0.7152 * rgba.g + 0.0722 * rgba.b;
    yuva.g = (rgba.b - yuva.r) / 1.8556;
    yuva.b = (rgba.r - yuva.r) / 1.5748;
    yuva.a = rgba.a;
                
    // Adjust to work on GPU
    yuva.gb += 0.5;
                
    return yuva;
}

float4 YUVtoRGB(float4 yuva) {
    yuva.gb -= 0.5;
    return float4(
        yuva.r * 1 + yuva.g * 0 + yuva.b * 1.5748,
        yuva.r * 1 + yuva.g * -0.187324 + yuva.b * -0.468124,
        yuva.r * 1 + yuva.g * 1.8556 + yuva.b * 0,
        yuva.a);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // sample the texture and convert to YUV color space
    float4 col = tex2D(SpriteTexture, input.TextureCoordinates);
    float4 yuv = RGBtoYUV(col);
    
    // Clamp the YUV color to specified color depth (default: 32, 5 bits per channel, as per playstation hardware)
    float4 col1 = floor(yuv * _Colors) / _Colors;
    float4 col2 = ceil(yuv * _Colors) / _Colors;
    
    // Calculate dither texture UV based on the input texture
    float ditherSize = _DitherPattern_TexelSize.w;
    float ditherSteps = _DitherPattern_TexelSize.z/ditherSize;
    
    float2 ditherBlockUV = input.TextureCoordinates;
    ditherBlockUV.x %= (ditherSize / _MainTex_TexelSize.z);
    ditherBlockUV.x /= (ditherSize / _MainTex_TexelSize.z);
    ditherBlockUV.y %= (ditherSize / _MainTex_TexelSize.w);
    ditherBlockUV.y /= (ditherSize / _MainTex_TexelSize.w);
    ditherBlockUV.x /= ditherSteps;
    
    // Dither each channel individually
    yuv.x = lerp(col1.x, col2.x, ditheredChannel(channelError(yuv.x, col1.x, col2.x), ditherBlockUV, ditherSteps));
    yuv.y = lerp(col1.y, col2.y, ditheredChannel(channelError(yuv.y, col1.y, col2.y), ditherBlockUV, ditherSteps));
    yuv.z = lerp(col1.z, col2.z, ditheredChannel(channelError(yuv.z, col1.z, col2.z), ditherBlockUV, ditherSteps));
    
    return YUVtoRGB(yuv);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
