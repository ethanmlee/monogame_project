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
float _VertWobbleAmount = 0.0;

float _LightIntensity = 0.9;

struct VertexShaderInput
{
    float4 Position : SV_Position;
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

    float4 worldPosPreOffset = mul(input.Position, _WorldMatrix);
    float4 pos = input.Position + _VertWobbleAmount * float4(
        nrand(worldPosPreOffset.yz) - 0.5, 
        nrand(worldPosPreOffset.xz) - 0.5, 
        nrand(worldPosPreOffset.xy) - 0.5, 
        0
    );

    output.Position = mul(mul(mul(pos, _WorldMatrix), _ViewMatrix), _ProjectionMatrix);
    output.WorldPos = mul(pos, _WorldMatrix);
    output.Color = input.Color;
    output.Normal = mul(input.Normal, _WorldMatrix);
    
    output.Position.y -= 0.0001 * DistanceCurve(
    output.WorldPos, 
    inverse(_ViewMatrix)[3].xyz, 
    15, 
    0, 
    2
    );
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 outputColor = input.Color;
    outputColor = DistanceLighting(outputColor, input.WorldPos, inverse(_ViewMatrix)[3].xyz, 1920, _LightIntensity, 0.01);
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