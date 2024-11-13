Shader "Augmenta/CameraDisplayTexture"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_PaddingColor("Padding Color", Color) = (0, 0, 0, 1)
	}

		CGINCLUDE

#include "UnityCustomRenderTexture.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;

		float4 _PaddingColor;
		float4 _BotCamUV; //x,y = botLeft; z,w = botRight
		float4 _TopCamUV; //x,y = topLeft; z,w = topRight

		float2 GetCamUV(float2 uv) {

			float2 topCamUV = lerp(_TopCamUV.xy, _TopCamUV.zw, uv.x);
			float2 botCamUV = lerp(_BotCamUV.xy, _BotCamUV.zw, uv.x);

			return lerp(botCamUV, topCamUV, uv.y);
		}

		half4 frag(v2f_customrendertexture i) : SV_Target
		{
			float2 uv = i.globalTexcoord;

			float2 camUV = GetCamUV(uv);

			bool isInside;

			isInside = camUV.x >= 0 && camUV.y >= 0 && camUV.x <= 1 && camUV.y <= 1;


			if (!isInside)
			{
				//If not seen by camera, return padding color
				return _PaddingColor;

			} else {
				//Read camera texture
				return tex2D(_MainTex, camUV);
			}
		}

			ENDCG

			SubShader
		{
			Cull Off ZWrite Off ZTest Always
				Pass
			{
				Name "Update"
				CGPROGRAM
				#pragma vertex CustomRenderTextureVertexShader
				#pragma fragment frag
				ENDCG
			}
		}
}
