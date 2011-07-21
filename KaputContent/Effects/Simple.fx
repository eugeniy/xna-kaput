float4x4 World;
float4x4 View;
float4x4 Projection;

float3 xLightDirection;
float xAmbient;
bool xEnableLighting;
Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

/* Simple Colored */

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Color = input.Color;

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;

    output.Color = input.Color;

    return output;
}

technique Simple
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}


/* Simple Textured */

struct TexVertexShaderOutput
{
    float4 Position     : POSITION;    
    float4 Color        : COLOR0;
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
};

struct TexPixelShaderOutput
{
    float4 Color : COLOR0;
};

TexVertexShaderOutput TexturedVertexShader( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{    
    TexVertexShaderOutput Output = (TexVertexShaderOutput)0;

    float4x4 preViewProjection = mul(View, Projection);
    float4x4 preWorldViewProjection = mul(World, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);    
    Output.TextureCoords = inTexCoords;
    
    float3 Normal = normalize(mul(normalize(inNormal), World));    
    Output.LightingFactor = 1;
    if (xEnableLighting)
        Output.LightingFactor = saturate(dot(Normal, -xLightDirection));
    
    return Output;    
}

TexPixelShaderOutput TexturedPixelShader(TexVertexShaderOutput PSIn)
{
    TexPixelShaderOutput Output = (TexPixelShaderOutput)0;        
    
    Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
    Output.Color.rgb *= saturate(PSIn.LightingFactor + xAmbient);

    return Output;
}

technique Textured
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 TexturedVertexShader();
        PixelShader = compile ps_2_0 TexturedPixelShader();
    }
}
