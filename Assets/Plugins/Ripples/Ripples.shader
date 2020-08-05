//https://www.shadertoy.com/view/4dK3Ww
Shader "Unlit/Ripples"
{
	Properties
	{
		_ImageTex("Image Texture", 2D) = "white" {}
		_RipplesTex("Ripples Texture", 2D) = "white" {}
		_TextureSize("Texture Size", vector) = (2048, 2048, 0 , 0)
		_Specular("Specular", float) = 10.0
		_SpecularPower("Specular Power", float) = 32.0
		_FrontSpecular("Front Specular", float) = 10
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			sampler2D _ImageTex;
			sampler2D _RipplesTex;
			float4 _TextureSize;
			float _Specular;
			float _SpecularPower;
			float _FrontSpecular;

			float4 _ImageTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _ImageTex);
				o.screenPos = ComputeScreenPos(o.vertex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			float4 frag(v2f i) : SV_Target{

				float2 q = i.uv; 

				float3 e = float3(float2(1., 1.) / _TextureSize.xy,0.);
				float4 p11 = tex2D(_RipplesTex, q);
				float p10 = tex2D(_RipplesTex, q - e.zy).x;
				float p01 = tex2D(_RipplesTex, q - e.xz).x;
				float p21 = tex2D(_RipplesTex, q + e.xz).x;
				float p12 = tex2D(_RipplesTex, q + e.zy).x;

				// Totally fake displacement and shading:
				float3 grad = normalize(float3(p21 - p01, p12 - p10, 1.));

				float4 c = tex2D(_ImageTex, q + (grad.xy*.35));

				float3 light = normalize(float3(.2, -.5, .7));
				//float diffuse = dot(grad,light);
				float spec = pow(max(0.,-reflect(light,grad).z), _SpecularPower) * _Specular;

				float frontSpec = max(1.0f - p11.z, 0) * _FrontSpecular * spec;

				//return tex2D(_RipplesTex, q);
				return c + spec + frontSpec;
			}
			ENDCG
		}
	}
}
