Shader "Custom/Drawing" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_DrawingTexture ("Albedo (RGB)", 2D) = "white" {}
		_Offset ("Offset", Range(0, 1)) = 0.00
		_Silhouette ("Offset", Range(0, 1)) = 0.00
		_LignSensitivity ("LignSensitivity", Range(0, 3)) = 0.00
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _DrawingTexture;
			half4 _Color;
			float _Offset;
			float _Silhouette;
			float _LignSensitivity;

			fixed4 frag (v2f_img i) : SV_Target
			{
				float2 uv = i.uv;

				fixed4 Filter = tex2D(_DrawingTexture, uv);
				float FilterGrayscale = Filter.r * 0.3 + Filter.g * 0.59 + Filter.b * 0.11;

				fixed4 c1 = tex2D(_MainTex, uv);
			    uv.x += _Offset;
			    fixed4 c2 = tex2D(_MainTex, uv);
			    uv.y += _Offset;
			    uv.x -= _Offset;
			    fixed4 c3 = tex2D(_MainTex, uv);
			    uv.y -= _Offset;
			    uv.x -= _Offset;
			    fixed4 c4 = tex2D(_MainTex, uv);
			    uv.y += _Offset;
			    uv.x += _Offset;
			    fixed4 c5 = tex2D(_MainTex, uv);

			    float Gsc1 = c1.r * 0.3 + c1.g * 0.59 + c1.b * 0.11;
  	 			float Gsc2 = c2.r * 0.3 + c2.g * 0.59 + c2.b * 0.11;
   				float Gsc3 = c3.r * 0.3 + c3.g * 0.59 + c3.b * 0.11;
   				float Gsc4 = c4.r * 0.3 + c4.g * 0.59 + c4.b * 0.11;
   				float Gsc5 = c5.r * 0.3 + c5.g * 0.59 + c5.b * 0.11;

   				half4 c = c1;

				float contrast = abs(Gsc1 - Gsc2) + abs(Gsc1 - Gsc3) + abs(Gsc1 - Gsc4) + abs(Gsc1 - Gsc5);
						    
			    if(contrast > _LignSensitivity){
			     	c -= half4(_Silhouette,_Silhouette,_Silhouette,1);   
			    }

			    c = c * FilterGrayscale  + _Color * (1 - FilterGrayscale);

				return c;
			}

		ENDCG
		}
	}
}
