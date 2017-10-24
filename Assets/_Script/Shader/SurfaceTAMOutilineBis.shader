Shader "Custom/DiffuseTAMOutlineBis" {
	 Properties {
    	_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
	  	_Layer1 ("Layer 1", 2D) = "white" {}
	  	_Layer2 ("Layer 2", 2D) = "white" {}
	  	_Layer3 ("Layer 3", 2D) = "white" {}
	  	_Layer4 ("Layer 4", 2D) = "white" {}
		_Outline ("Outline", 2D) = "white" {}
	  	_Exposure ("Exposure", float) = 0.0 
		_Tiling("Tiling", float) = 1.0
		_Oscilation("Oscilation", float) = 1.0 

		_OutlineWidth("Outline Width", float) = .005
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
   }

	CGINCLUDE
		#pragma target 5.0
		#include "UnityCG.cginc"
    	#include "AutoLight.cginc"

		/*------------------------------OUTLINE------------------------------*/

		sampler2D _Outline;
		half4 _OutlineColor;
		float _OutlineWidth;

		struct vInOutline
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2fOutline
		{
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
			float4 vertex : SV_POSITION;
		};

		v2fOutline vertOutline (vInOutline v)
		{
			v2fOutline o;

			o.normal = normalize(v.normal);
			v.vertex.xyz += o.normal * _OutlineWidth;

			o.vertex = UnityObjectToClipPos(v.vertex);
			return o;
		}

		fixed4 fragOutline (v2fOutline i) : SV_Target
		{
			//half4 c = tex2D(_Outline, i.uv);
			return _OutlineColor;
		}

		/*------------------------------HATCHING------------------------------*/
		sampler2D _MainTex;
		sampler2D _Layer1;
		sampler2D _Layer2;
		sampler2D _Layer3;
		sampler2D _Layer4;
		
		float4 _Color;
		float4 _LightColor0; 
		
		float _Exposure;
		float _Tiling;
		float _Oscilation;

		struct vIn {
			float4 vertex 		: POSITION;
			float3 normal 		: NORMAL;
			float4 texcoord 	: TEXCOORD0;
		};

		struct v2f
		{
			float4 pos         	: SV_POSITION;
			float2 uv         	: TEXCOORD0;
			float3 lightDir    	: TEXCOORD2;
			float3 normal		: TEXCOORD1;
			half4 lightColor	: half4;
			float light 		: float;
			LIGHTING_COORDS(3,4)                            
		};
		
		v2f vert (vIn v)
		{
			v2f o;
			
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			o.lightDir = normalize(ObjSpaceLightDir(v.vertex));
			o.normal = v.normal;
			o.lightColor = _LightColor0 * saturate(dot(o.normal, o.lightDir));
			o.light = o.lightColor.x;
			TRANSFER_VERTEX_TO_FRAGMENT(o);                 
			return o;
		}

		fixed4 frag(v2f i) : SV_TARGET
		{
			fixed atten = LIGHT_ATTENUATION(i);

			int oscillation = (int)(_Time.x * _Oscilation);
			fixed2 uv = i.uv * _Tiling;
			uv.x += oscillation * 0.8;
			fixed4 albedo = tex2D(_MainTex, i.uv);
			half light = ((albedo.r * 0.3 + albedo.g * 0.59 + albedo.b * 0.11) * i.light * 4) * atten   + _Exposure;
			
			half3 intensity = half3(light,light,light);

			half3 weights0 = saturate(intensity - half3(0,1,2));
			half2 weights1 = saturate(intensity.xy - half2(3,4));
			weights0.x += 0.1;
			weights0.xy -= weights0.yz;
			weights0.z -= weights1.x;
			weights1.x -= weights1.y;

			fixed4 cl1 = tex2D(_Layer1, uv);
			fixed4 cl2 = tex2D(_Layer2, uv);
			fixed4 cl3 = tex2D(_Layer3, uv);
			fixed4 cl4 = tex2D(_Layer4, uv);

			fixed4 c = (cl4 * weights0.x) + (cl3 * weights0.y) + (cl2 * weights0.z) + (cl1 * weights1.x) + (fixed4(1,1,1,1) * weights1.y);

			return c * _Color * (albedo * 0.5f) * (_LightColor0 * atten);
		}
	ENDCG

   	SubShader {
		LOD 100

		Pass
		{
			Tags { "LightMode"="Always" }
			Cull Front

			CGPROGRAM
			#pragma vertex vertOutline
			#pragma fragment fragOutline
			ENDCG
		}

    	Pass 
		{	
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert  
			#pragma fragment frag
			#pragma multi_compile_fwdbase 
			ENDCG
      	}

		Pass 
		{	
			Tags { "LightMode" = "ForwardAdd" }
			Blend One One
	
			CGPROGRAM
			#pragma vertex vert  
			#pragma fragment frag 
			#pragma multi_compile_fwdadd
			ENDCG
		}
   	}
   	Fallback "Diffuse"
}
