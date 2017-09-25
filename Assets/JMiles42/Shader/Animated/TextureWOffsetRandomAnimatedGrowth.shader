﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "JMiles42/Animated/Random/TextureWOffsetRandomAnimatedGrowth"
{
	Properties
	{
		_Colour ("Colour",color)=(1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Offset ("Offset",Range(-0.1,0.1))=0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Colour;
			float _Offset;
			
			v2f vert (appdata v)
			{
				v2f o;
				v.vertex.xyz += v.normal.xyz * _Offset * sin( _Time.y * v.vertex.x * v.vertex.z / v.vertex.y);
				//v.vertex.xyz += v.normal.xyz * _Offset * sin(_Time.y);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
