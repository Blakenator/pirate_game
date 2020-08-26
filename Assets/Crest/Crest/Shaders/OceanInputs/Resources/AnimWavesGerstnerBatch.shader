// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// Adds Gerstner waves everywhere. Must be given batch prepared by ShapeGerstnerBatched.cs.
Shader "Hidden/Crest/Inputs/Animated Waves/Gerstner Batch Global"
{
	Properties
	{
	}

	SubShader
	{
		Pass
		{
			Blend One One
			ZWrite Off
			ZTest Always
			Cull Off

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#pragma multi_compile __ CREST_DIRECT_TOWARDS_POINT_INTERNAL

			#include "UnityCG.cginc"

			#include "../../OceanInputsDriven.hlsl"
			#include "../../OceanGlobals.hlsl"
			#include "../../OceanHelpersNew.hlsl"

			sampler2D _LD_Sampler_SeaFloorDepth_0;

			#include "../GerstnerShared.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 worldPosXZ : TEXCOORD0;
				float3 uv_slice : TEXCOORD1;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = float4(input.positionOS.xy, 0.0, 0.5);

#if UNITY_UV_STARTS_AT_TOP // https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
				o.positionCS.y = -o.positionCS.y;
#endif

				float2 worldXZ = UVToWorld(input.uv, _LD_SliceIndex, _LD_Pos_Scale[_LD_SliceIndex], _LD_Params[_LD_SliceIndex]);
				o.worldPosXZ = worldXZ;
				o.uv_slice = float3(input.uv, _LD_SliceIndex);
				return o;
			}

			half4 Frag(Varyings input) : SV_Target
			{
				return ComputeGerstner(input.worldPosXZ, input.uv_slice);
//#endif
			}
			ENDCG
		}
	}
}