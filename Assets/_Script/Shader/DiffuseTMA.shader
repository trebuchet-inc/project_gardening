Shader "Custom/DiffuseTMA" {
	 Properties {
    	_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
	  	_Layer1 ("Layer 1", 2D) = "white" {}
	  	_Layer2 ("Layer 2", 2D) = "white" {}
	  	_Layer3 ("Layer 3", 2D) = "white" {}
	  	_Layer4 ("Layer 4", 2D) = "white" {}
		_Outline ("Outline", 2D) = "white" {}
	  	_Exposure ("Offset", float) = 0.0 
		_Tiling("Tiling", float) = 1.0 

		g_vOutlineColor( "Outline Color", Color ) = ( .5, .5, .5, 1 )
		g_flOutlineWidth( "Outline width", Range ( .001, 0.03 ) ) = .005
		g_flCornerAdjust( "Corner Adjustment", Range( 0, 2 ) ) = .5
   }

	CGINCLUDE
		#pragma target 5.0
		#include "UnityCG.cginc"

		sampler2D _Outline;
		float4 g_vOutlineColor;
		float g_flOutlineWidth;
		float g_flCornerAdjust;

		struct VS_INPUT
		{
			float4 vPositionOs : POSITION;
			float3 vNormalOs : NORMAL;
		};

		struct PS_INPUT
		{
			float4 vPositionOs : TEXCOORD0;
			float3 vNormalOs : TEXCOORD1;
			float4 vPositionPs : SV_POSITION;
		};

		PS_INPUT MainVs( VS_INPUT i )
		{
			PS_INPUT o;
			o.vPositionOs.xyzw = i.vPositionOs.xyzw;
			o.vNormalOs.xyz = i.vNormalOs.xyz;
			o.vPositionPs = UnityObjectToClipPos( i.vPositionOs.xyzw );
			return o;
		}

		PS_INPUT Extrude( PS_INPUT vertex )
		{
			PS_INPUT extruded = vertex;

			// Offset along normal in projection space
			float3 vNormalVs = mul( ( float3x3 )UNITY_MATRIX_IT_MV, vertex.vNormalOs.xyz );
			float2 vOffsetPs = TransformViewToProjection( vNormalVs.xy );
			vOffsetPs.xy = normalize( vOffsetPs.xy );

			// Calculate position
			extruded.vPositionPs = UnityObjectToClipPos( vertex.vPositionOs.xyzw );
			extruded.vPositionPs.xy += vOffsetPs.xy * extruded.vPositionPs.w * g_flOutlineWidth;

			return extruded;
		}

		[maxvertexcount(18)]
		void ExtrudeGs( triangle PS_INPUT inputTriangle[3], inout TriangleStream<PS_INPUT> outputStream )
		{
			float3 a = normalize(inputTriangle[0].vPositionOs.xyz - inputTriangle[1].vPositionOs.xyz);
			float3 b = normalize(inputTriangle[1].vPositionOs.xyz - inputTriangle[2].vPositionOs.xyz);
			float3 c = normalize(inputTriangle[2].vPositionOs.xyz - inputTriangle[0].vPositionOs.xyz);

			inputTriangle[0].vNormalOs = inputTriangle[0].vNormalOs + normalize( a - c) * g_flCornerAdjust;
			inputTriangle[1].vNormalOs = inputTriangle[1].vNormalOs + normalize(-a + b) * g_flCornerAdjust;
			inputTriangle[2].vNormalOs = inputTriangle[2].vNormalOs + normalize(-b + c) * g_flCornerAdjust;

			PS_INPUT extrudedTriangle0 = Extrude( inputTriangle[0] );
			PS_INPUT extrudedTriangle1 = Extrude( inputTriangle[1] );
			PS_INPUT extrudedTriangle2 = Extrude( inputTriangle[2] );

			outputStream.Append( inputTriangle[0] );
			outputStream.Append( extrudedTriangle0 );
			outputStream.Append( inputTriangle[1] );
			outputStream.Append( extrudedTriangle0 );
			outputStream.Append( extrudedTriangle1 );
			outputStream.Append( inputTriangle[1] );

			outputStream.Append( inputTriangle[1] );
			outputStream.Append( extrudedTriangle1 );
			outputStream.Append( extrudedTriangle2 );
			outputStream.Append( inputTriangle[1] );
			outputStream.Append( extrudedTriangle2 );
			outputStream.Append( inputTriangle[2] );

			outputStream.Append( inputTriangle[2] );
			outputStream.Append( extrudedTriangle2 );
			outputStream.Append(inputTriangle[0]);
			outputStream.Append( extrudedTriangle2 );
			outputStream.Append( extrudedTriangle0 );
			outputStream.Append( inputTriangle[0] );
		}

		fixed4 MainPs( PS_INPUT i ) : SV_Target
		{
			return tex2D(_Outline, i.vPositionOs) * g_vOutlineColor;
		}

		fixed4 NullPs( PS_INPUT i ) : SV_Target
		{
			return float4( 1.0, 0.0, 1.0, 1.0 );
		}
	ENDCG

   	SubShader {
		Pass
		{
			Tags { "LightMode" = "Always" }
			ColorMask 0
			Cull Off
			ZWrite Off
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM
			#pragma vertex MainVs
			#pragma fragment NullPs
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "Always" }
			Cull Off
			ZWrite On
			Stencil
			{
				Ref 1
				Comp notequal
				Pass keep
				Fail keep
			}

			CGPROGRAM
			#pragma vertex MainVs
			#pragma geometry ExtrudeGs
			#pragma fragment MainPs
			ENDCG
		}

    	Pass 
		{	
			Tags { "LightMode" = "ForwardBase" } 
	
			CGPROGRAM
	
			#pragma vertex vert  
			#pragma fragment frag 
	
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _Layer1;
			sampler2D _Layer2;
			sampler2D _Layer3;
			sampler2D _Layer4;
			
			float4 _Color;
			float4 _LightColor0; 
			
			float _Exposure;
			float _Tiling;
	
			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
			};

			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float light : float;
			};
	
			vertexOutput vert(vertexInput input) 
			{
				vertexOutput output;
	
				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;
				float3 normalDirection = normalize(mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 lightColor = _LightColor0 * max(0.0, dot(normalDirection, lightDirection));

				output.light = lightColor.r * 0.3 + lightColor.g * 0.59 + lightColor.b * 0.11;
				output.uv = input.texcoord;
				output.pos = UnityObjectToClipPos(input.vertex);
				return output;
			}

			float GetIntensity(float Value, float Begin, float End, bool last = false)
			{
				if(Value > Begin && (Value < End || last))
				{
					float Moy = (End - Begin) * 0.5;
					if(Value - Begin <= Moy)
					{
						return (Value - Begin) * (1/Moy);
					}
					else
					{
						if(!last) return 1 - (Value - (Begin + Moy)) * (1/Moy);
						else return 1;
					}
				}
				return 0;
			}
	
			float4 frag(vertexOutput input) : COLOR
			{
				int oscillation = (int)((_Time.x * 100.0) % 10);
				fixed2 uv = input.uv.xy * _Tiling;
				uv.x += oscillation * 0.8;
				fixed4 albedo = tex2D(_MainTex, input.uv.xy);
				float albedoGS = albedo.r * 0.3 + albedo.g * 0.59 + albedo.b * 0.11;
				float light = input.light * albedoGS;

				float begin = 0.0 + _Exposure;
				float end = 0.4 + _Exposure;

				fixed4 cl1 = tex2D(_Layer1, uv);
				fixed4 cl2 = tex2D(_Layer2, uv);
				fixed4 cl3 = tex2D(_Layer3, uv);
				fixed4 cl4 = tex2D(_Layer4, uv);

				float il4 = GetIntensity(light, begin, end);
				begin += 0.2; 
				end += 0.2;
				float il3 = GetIntensity(light, begin, end);
				begin += 0.2; 
				end += 0.2;
				float il2 = GetIntensity(light, begin, end);
				begin += 0.2; 
				end += 0.2;
				float il1 = GetIntensity(light, begin, end);
				begin += 0.2; 
				end += 0.2;
				float il0 = GetIntensity(light, begin, end, true);

				fixed4 c = (cl1 * il1) + (cl2 * il2) + (cl3 * il3) + (cl4 * il4) + (fixed4(1,1,1,1) * il0);

				return c * _Color;
			}
			ENDCG
      	}
   	}
   	Fallback "Diffuse"
}