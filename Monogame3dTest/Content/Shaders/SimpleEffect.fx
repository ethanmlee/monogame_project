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
float4 _Color = {1, 1, 1, 1};
sampler2D _MainTex;
int _HasTex;
int _IsUnlit;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 WorldPos : TEXCOORD1;
    float3 Normal : TEXCOORD2;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    // output.Position = input.Position * _WorldMatrix * _ViewMatrix * _ProjectionMatrix;
    output.Position = mul(mul(mul(input.Position, _WorldMatrix), _ViewMatrix), _ProjectionMatrix);
    output.WorldPos = mul(input.Position, _WorldMatrix);
    output.Normal = mul(input.Normal, _WorldMatrix);
    output.UV = input.UV;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 outputColor = _Color;
    if (_HasTex == 1) 
    {
        outputColor = outputColor * (_HasTex * tex2D(_MainTex, input.UV) + (1 - _HasTex));
    }

    outputColor = _IsUnlit ? outputColor : DistanceLighting(outputColor, input.WorldPos, float3(0, 1, 0));

    
    // outputColor -= abs(input.Normal.x) * 0.02;
    
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