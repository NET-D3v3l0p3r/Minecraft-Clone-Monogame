#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_3
	#define PS_SHADERMODEL ps_4_0_level_9_3
#endif

//-----------------------------------------------------------------------------
// InstancedModel.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//TODO EXTRACT TEXTURE FROM TEXTURECOORD

// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;

// This sample uses a simple Lambert lighting model.
float3 LightDirection = normalize(float3(-1, -1, -1));
float3 DiffuseLight = 1.25;
float3 AmbientLight = 0.35;

Texture Texture0;
sampler Sampler0 = sampler_state { texture = <Texture0>; magfilter = POINT; minfilter = LINEAR; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

Texture Texture1;
sampler Sampler1 = sampler_state { texture = <Texture1>; magfilter = POINT; minfilter = LINEAR; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

Texture Texture2;
sampler Sampler2 = sampler_state { texture = <Texture2>; magfilter = POINT; minfilter = LINEAR; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

Texture Texture3;
sampler Sampler3 = sampler_state { texture = <Texture3>; magfilter = POINT; minfilter = LINEAR; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

Texture Texture4;
sampler Sampler4 = sampler_state { texture = <Texture4>; magfilter = POINT; minfilter = LINEAR; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

Texture Texture5;
sampler Sampler5 = sampler_state { texture = <Texture5>; magfilter = POINT; minfilter = LINEAR; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

Texture Texture6;
sampler Sampler6 = sampler_state { texture = <Texture6>; magfilter = POINT; minfilter = LINEAR; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

Texture Texture7;
sampler Sampler7 = sampler_state { texture = <Texture7>; magfilter = POINT; minfilter = LINEAR; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

Texture Texture8;
sampler Sampler8 = sampler_state { texture = <Texture8> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture Texture9;
sampler Sampler9 = sampler_state { texture = <Texture9> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture Texture10;
sampler Sampler10 = sampler_state { texture = <Texture10> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture Texture11;
sampler Sampler11 = sampler_state { texture = <Texture11> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture Texture12;
sampler Sampler12 = sampler_state { texture = <Texture12> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture Texture13;
sampler Sampler13 = sampler_state { texture = <Texture13> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture Texture14;
sampler Sampler14 = sampler_state { texture = <Texture14> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture Texture15;
sampler Sampler15 = sampler_state { texture = <Texture15> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};


float     FogEnabled;
float     FogStart;
float     FogEnd;
float3    FogColor;


uniform const float3    DiffuseColor = 1;
uniform const float     Alpha = 1;
uniform const float3    EmissiveColor = 0;
uniform const float3    SpecularColor = 1;
uniform const float     SpecularPower = 1;
uniform const float3    EyePosition;


struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
	float2 Index : TEXCOORD1;
	float4x4 Instance : BLENDWEIGHT0;
};


struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD0;
	float2 IndexCoord : TEXCOORD1;
	float4  Specular    : COLOR2;
};

float ComputeFogFactor(float d)
{
	return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogEnabled;
}

VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;

	// Apply the world and camera matrices to compute the output position.
	float4 worldPosition = mul(input.Position, instanceTransform);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
    //input.Unnormal();
	// Compute lighting, using a simple Lambert model.
	float4 worldNormal = mul(float4(input.Normal.xyz,0), instanceTransform);
	float diffuseAmount = max(-dot(worldNormal, float4(LightDirection.xyz, 0)), 0);
	float3 lightingResult = saturate(diffuseAmount * DiffuseLight + AmbientLight);

	output.Color = float4(lightingResult.rgb + EmissiveColor, 1);
	output.Specular = float4(0, 0, 0, ComputeFogFactor(length(float4(EyePosition.xyz,0) - worldPosition)));

	// Copy across the input texture coordinate.
	output.TextureCoordinate = input.TextureCoordinate;
	output.IndexCoord = input.Index;
	return output;
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input)
{
	return VertexShaderCommon(input, mul(World, transpose(input.Instance)));
}

// Both techniques share this same pixel shader.
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 tex = float4(0,0,0,0);
	//2010 2015

	if (input.IndexCoord.x > -1 && input.IndexCoord.x < 1)  //01
		tex = tex2D(Sampler0, input.TextureCoordinate);
	if (input.IndexCoord.x > 1 && input.IndexCoord.x <  3)   //02
		tex = tex2D(Sampler1, input.TextureCoordinate);
	if (input.IndexCoord.x > 3 && input.IndexCoord.x <  5)   //04
		tex = tex2D(Sampler2, input.TextureCoordinate);
	if (input.IndexCoord.x > 5 && input.IndexCoord.x <  7)   //06
		tex = tex2D(Sampler3, input.TextureCoordinate);
	if (input.IndexCoord.x > 7 && input.IndexCoord.x <  9)   //08
		tex = tex2D(Sampler4, input.TextureCoordinate);
	if (input.IndexCoord.x > 9 && input.IndexCoord.x <  11)  //10
		tex = tex2D(Sampler5, input.TextureCoordinate);
	if (input.IndexCoord.x > 11 && input.IndexCoord.x < 13) //12
		tex = tex2D(Sampler6, input.TextureCoordinate);
	if (input.IndexCoord.x > 13 && input.IndexCoord.x < 15) //14
		tex = tex2D(Sampler7, input.TextureCoordinate);
	if (input.IndexCoord.x > 15 && input.IndexCoord.x < 17) //16
		tex = tex2D(Sampler8, input.TextureCoordinate);
	if (input.IndexCoord.x > 17 && input.IndexCoord.x < 19) //18
		tex = tex2D(Sampler9, input.TextureCoordinate);
	if (input.IndexCoord.x > 19 && input.IndexCoord.x < 21) //20
		tex = tex2D(Sampler10, input.TextureCoordinate);
	if (input.IndexCoord.x > 21 && input.IndexCoord.x < 23) //22
		tex = tex2D(Sampler11, input.TextureCoordinate);
	if (input.IndexCoord.x > 23 && input.IndexCoord.x < 25) //24
		tex = tex2D(Sampler12, input.TextureCoordinate);
	if (input.IndexCoord.x > 25 && input.IndexCoord.x < 27) //26
		tex = tex2D(Sampler13, input.TextureCoordinate);
	if (input.IndexCoord.x > 27 && input.IndexCoord.x < 29) //28
		tex = tex2D(Sampler14, input.TextureCoordinate);
	if (input.IndexCoord.x > 29 && input.IndexCoord.x < 31) //30
		tex = tex2D(Sampler15, input.TextureCoordinate);

	if (input.IndexCoord.x > 5 && input.IndexCoord.x < 7)
		input.Color = float4(input.Color.rgb, 0.981f);
	tex *= input.Color + float4(input.Specular.rgb, 0);

	tex.rgb = lerp(tex.rgb, FogColor, input.Specular.w);

	return tex;

}

technique HardwareInstancing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL HardwareInstancingVertexShader();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};