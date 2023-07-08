#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix _WorldViewProjection;
float4 _Color = (1, 1, 1, 1);
sampler2D _MainTex;
int _HasTex;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 texCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, _WorldViewProjection);
    output.texCoord = input.texCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 outputColor = _Color;
    if (_HasTex == 1) 
    {
        outputColor = outputColor * (_HasTex * tex2D(_MainTex, input.texCoord) + (1 - _HasTex));
    }
    
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