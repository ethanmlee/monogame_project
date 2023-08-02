// Standard defines
#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

#include "LightingCode.hlsl"

matrix _WorldMatrix;
matrix _ViewMatrix;
matrix _ProjectionMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 WorldPos : TEXCOORD1;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    // output.Position = input.Position * _WorldMatrix * _ViewMatrix * _ProjectionMatrix;
    output.Position = mul(mul(mul(input.Position, _WorldMatrix), _ViewMatrix), _ProjectionMatrix);
    output.WorldPos = mul(input.Position, _WorldMatrix);
    output.Color = input.Color;
    output.Normal = mul(input.Normal, _WorldMatrix);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 outputColor = input.Color;
    outputColor = DistanceLighting(outputColor, input.WorldPos, inverse(_ViewMatrix)[3].xyz, 192, 0.5, 0.1);
    outputColor.a = 1;
    return saturate(outputColor);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};