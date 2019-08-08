Shader "ImageEffect/heartrate"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Fog{Mode off}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			half4 _MainTex_TexelSize; //텍셀이 차지하는 크기
			half2 _BlurCenterPos; //블러의 중심위치
			half _BlurSize; //블러를 위해 이미지가 이동된 거리
			half _Samples; //샘플링 수

			fixed4 frag(v2f_img IN) : SV_Target
			{
				half4 col = half4(0,0,0,1);
				half2 movedTexcoord = IN.uv - _BlurCenterPos;

				for (int i = 0; i < _Samples; i++)
				{
					half Scale = 1.0f - _BlurSize * _MainTex_TexelSize.x * i;
					col.rgb += tex2D(_MainTex, movedTexcoord * Scale + _BlurCenterPos).rgb;
				}
				col.rgb *= 1 / _Samples;
				return col;
			}
			ENDCG
		}
	}
}
