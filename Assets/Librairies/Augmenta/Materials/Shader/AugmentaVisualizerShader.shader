Shader "Unlit/AugmentaVisualizerShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PointColor("Point color", Color) = (0,0,0,0)
		_CenterColor("Center color", Color) = (0,0,0,0)
		_BackgroundColor("Background color", Color) = (0,0,0,0)
		_Transparency("Transparency", Range(0.0, 1.0)) = 1.0
		_SquareSize ("SquareSize", Range(0.01, 0.15)) = 0.1
		_CenterSize ("CenterSize", Range(0.001, 0.1)) = 0.05
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
     
		ZWrite Off
		ZTest Always     
		Blend SrcAlpha OneMinusSrcAlpha 
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			uniform float4 AugmentaPoints[50]; 
			float4 _PointColor;
			float4 _CenterColor;
			float4 _BackgroundColor;
			float _Transparency;

			float _SquareSize;
			float _CenterSize;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _BackgroundColor;

				for(int j=0 ; j<50 ; j++) {
					if(AugmentaPoints[j].x == 0 && AugmentaPoints[j].y == 0)
						break;

					if(i.uv.x < AugmentaPoints[j].x + _SquareSize && i.uv.x > AugmentaPoints[j].x - _SquareSize) {
						if(i.uv.y < AugmentaPoints[j].y + _SquareSize && i.uv.y > AugmentaPoints[j].y - _SquareSize) {
							col = _PointColor;
							//Check center
							if(i.uv.x < AugmentaPoints[j].x + _CenterSize && i.uv.x > AugmentaPoints[j].x - _CenterSize) {
								if(i.uv.y < AugmentaPoints[j].y + _CenterSize && i.uv.y > AugmentaPoints[j].y - _CenterSize) {
									col = _CenterColor;
								}
							}
						}
					}
				}
				col.a = _Transparency;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
