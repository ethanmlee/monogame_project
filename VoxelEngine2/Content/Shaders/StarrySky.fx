#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Function to generate a random float between 0 and 1
float random(float2 seed)
{
    return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
}

float3 lerpColors(float t)
{
    const float3 color1 = float3(1.0f, 0.462f, 0.349f);  // ff7659
    const float3 color2 = float3(1.0f, 0.8f, 0.435f);    // ffcc6f
    const float3 color3 = float3(1.0f, 0.941f, 0.588f);  // fff094
    const float3 color4 = float3(1.0f, 1.0f, 1.0f);      // ffffff
    const float3 color5 = float3(0.478f, 0.729f, 1.0f);  // 7abbff

    // Ensure t is within [0, 1]
    t = saturate(t);
    t = lerp(t, 0.75, random(t) * 0.35);

    // Use smoothstep to interpolate between colors
    float3 result;
    if (t < 0.25f)
    {
        result = lerp(color1, color2, smoothstep(0.0f, 0.25f, t));
    }
    else if (t < 0.5f)
    {
        result = lerp(color2, color3, smoothstep(0.25f, 0.5f, t));
    }
    else if (t < 0.75f)
    {
        result = lerp(color3, color4, smoothstep(0.5f, 0.75f, t));
    }
    else
    {
        result = lerp(color4, color5, smoothstep(0.75f, 1.0f, t));
    }

    return result;
}



#define HASHSCALE3 float3(.1031, .1031, .1031)
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
    
    for(int i = 0; i < 3; i++)
    {
     
        //calculate a value to multiply UVs by to guarantee square cell shapes.
        const float2 uv_scale = float(i)*1.0 + UVSCALE;
        const float2 uv = uvCoord * uv_scale;
    
        // get random 2d cell noise
        const float2 cell_uvs = floor(uv + float(i * 1199));
        const float2 hash = (hash2d(cell_uvs) * 2.0 - 1.0);
        const float hash_magnitude =(1.0-length(hash));

        // calculate uv cell grid.
        const float2 uv_grid = frac(uv) - 0.5;

        const float radius = clamp(hash_magnitude - 0.5, 0.0, 1.0);
        float radial_gradient = length(uv_grid - hash) / radius;
        radial_gradient = clamp(1.0 - radial_gradient, 0.0, 1.0);
        radial_gradient = pow(radial_gradient, 3);

        // Add random brightness to each star
        col += radial_gradient * (hash_magnitude) * lerpColors(hash2d(uvCoord + float(i)));
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