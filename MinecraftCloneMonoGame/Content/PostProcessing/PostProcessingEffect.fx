sampler TextureSampler : register(s0);
Texture2D  myTex2D;

float _Value;

float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 tex;
	tex = myTex2D.Sample(TextureSampler, texCoord.xy) * .6f;
	tex += myTex2D.Sample(TextureSampler, texCoord.xy + (_Value)) * .4f;
	return tex;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();  
    }
}