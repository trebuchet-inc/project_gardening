// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BilboardLeaf"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		LOD 100 
		Stencil
		{
			Ref 10
			Comp Always
			pass replace
			Fail replace
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct vIn
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			half4 _Color;
			
			v2f vert (vIn v)
			{
				v2f o;
				
				o.pos = UnityObjectToClipPos(v.vertex);

				// o.pos = mul(UNITY_MATRIX_P, 
              	// 		mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
              	// 		+ float4(v.vertex.x, v.vertex.y, 0.0, 0.0));

				//o.pos =  mul(UNITY_MATRIX_P,mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + v.vertex);

				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float gs = (col.r * 0.3 + col.g * 0.59 + col.b * 0.11);

				col = (1 - gs) * _Color;
				col.a = 1- gs;
				
				return col;
			}
			ENDCG
		}
	}
}
