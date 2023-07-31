#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif


#define HASHSCALE3 float3(.1031, .1030, .0973)
#define UVSCALE 500.0

// Hash without Sine
// Creative Commons Attribution-ShareAlike 4.0 International Public License
// Created by David Hoskins.

///  2d hash based on 2d coordinates.
float2 hash2d(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * HASHSCALE3);
    p3 += dot(p3, p3.yzx+19.19);
    return frac((p3.xx+p3.yz)*p3.zy);
}

void AddStars(out float4 frag_color, float2 uvCoord)
{
    float3 col = 0.0;
    
    for(int i = 0; i < 8; i++)
    {
     
        //calculate a value to multiply UVs by to guarantee square cell shapes.
        const float2 uv_scale = float(i)*1.0 + UVSCALE;
        const float2 uv = uvCoord * uv_scale;
    
        // get random 2d cell noise
        const float2 cell_uvs = floor(uv + float(i * 1199));
        const float2 hash = (hash2d(cell_uvs) * 2.0 - 1.0) * 2.0;
        const float hash_magnitude =(1.0-length(hash));

        // calculate uv cell grid.
        const float2 uv_grid = frac(uv) - 0.5;

        const float radius = clamp(hash_magnitude - 0.5, 0.0, 1.0);
        float radial_gradient = length(uv_grid - hash) / radius;
        radial_gradient = clamp(1.0 - radial_gradient, 0.0, 1.0);
        radial_gradient *= radial_gradient;
    
        col += radial_gradient;
    }
    // Output to screen
    frag_color = float4(col,1.0);
}


matrix _WorldMatrix;
matrix _ViewMatrix;
matrix _ProjectionMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
    float3 Normal : TEXCOORD2;
};

VertexShaderOutput MainVS(const in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(mul(mul(input.Position, _WorldMatrix), _ViewMatrix), _ProjectionMatrix);
    output.UV = input.UV;
    output.Normal = input.Normal;
    
    return output;
}

float4 MainPS(const VertexShaderOutput input) : COLOR
{
    float4 output_color = 0;
    AddStars(output_color, input.UV);
    return output_color;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};