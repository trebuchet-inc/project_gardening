Shader "Custom/SurfaceTAM" {
	Properties {
    	_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	  	_Layer1 ("Layer 1", 2D) = "white" {}
	  	_Layer2 ("Layer 2", 2D) = "white" {}
	  	_Layer3 ("Layer 3", 2D) = "white" {}
	  	_Layer4 ("Layer 4", 2D) = "white" {}
		_ColorWeight ("Color Weight", float) = 0.0 
		_Tiling("Tiling", float) = 1.0
		_Oscilation("Oscilation", float) = 1.0 
	}
	SubShader {
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _Layer1;
			sampler2D _Layer2;
			sampler2D _Layer3;
			sampler2D _Layer4;

			static const float PI = 3.14159;

			half4 _Color;
			float _ColorWeight;
			float _Tiling;
			float _Oscilation;

			fixed4 frag (v2f_img i) : SV_Target
			{
				half4 albedo = tex2D(_MainTex, i.uv);
				//int oscillation = (int)(_Time.x * _Oscilation);

				half2 uv = i.uv * _Tiling;

				//uv.x += oscillation * 0.8;
				//uv.y += oscillation * 0.5;
				uv -= _Tiling * 0.5f;
				float si = sin ( _Oscilation + PI);
            	float co = cos ( _Oscilation + PI);
            	float2x2 rotationMatrix = float2x2( co, -si, si, co);
				uv = mul(uv, rotationMatrix);
				uv += _Tiling * 0.5f;
				

				half light = (albedo.r * 0.3 + albedo.g * 0.59 + albedo.b * 0.11) * 4;
				half3 intensity = half3(light,light,light);

				half3 weights0 = saturate(intensity - half3(0,1,2));
				half2 weights1 = saturate(intensity.xy - half2(3,4));
				weights0.x += 0.1;
				weights0.xy -= weights0.yz;
				weights0.z -= weights1.x;
				weights1.x -= weights1.y;

				half4 cl1 = tex2D(_Layer1, uv);
				half4 cl2 = tex2D(_Layer2, uv);
				half4 cl3 = tex2D(_Layer3, uv);
				half4 cl4 = tex2D(_Layer4, uv);

				fixed4 c = (cl4 * weights0.x) + (cl3 * weights0.y) + (cl2 * weights0.z) + (cl1 * weights1.x) + (fixed4(1,1,1,1) * weights1.y);
				albedo = _Color * (1 - _ColorWeight) + albedo * _ColorWeight;
				return albedo * (1 - Luminance(c)) + fixed4(1,1,1,1) * Luminance(c);
			}
			ENDCG
		}
	}
}
