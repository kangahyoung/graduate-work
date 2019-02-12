Shader "Custom/PhantomShader"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		[perrenderdata]_MainTex("Base (RGB)", 2D) = "white" {}
		_PhantomColor("Phantom Color", Color) = (1,1,1,1)
		_PhantomPower("Phantom Power", Float) = 1
			//_Cutoff ("Alpha cutoff", Range (0,1)) = 0.0

	}

		SubShader
		{
			//Pass: character 
			stencil
			{
			  ref 18
			  comp always

			  pass replace
			//zfail incrsat
			//fail   incrsat 
		  }
		  Tags { "Queue" = "Geometry+1" "RenderType" = "Opaque" }

		  CGPROGRAM
		  #pragma surface surf BlinnPhong 
			//alphatest:_Cutoff

			uniform float4 _Color;
			uniform float4 _Indicator;
			uniform sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
				float3 viewDir;
			};

			void surf(Input IN, inout SurfaceOutput o)
			{
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;
			}
			ENDCG

				// Pass: Phantom
				stencil
				{
				  ref 18
				  comp notequal

				  pass replace
				  zfail incrsat
				}
			  ZWrite On
			  ZTest greater
				//Blend DstColor Zero
				Blend SrcAlpha OneMinusSrcAlpha
				//Alphatest lequal 1
		  //     
				CGPROGRAM
				#include "UnityCG.cginc"
				#pragma surface surf BlinnPhong
				uniform float4 _Color;
				uniform fixed  _PhantomPower;
				uniform fixed4 _PhantomColor;
				uniform sampler2D _MainTex;
				//uniform sampler2D _IndicatorTex;
				//fixed4 _IndicatorTex_ST;

				struct Input
				{
					float2 uv_MainTex;
					float3 viewDir;
					float3 worldNormal;
				};

				void surf(Input IN, inout SurfaceOutput o)
				{

					  half rim = 1.0 - saturate(dot(normalize(IN.viewDir), IN.worldNormal));
					  fixed3 rim_final = _PhantomColor.rgb * pow(rim, _PhantomPower);
					  //
					  //TRANSFORM_TEX( _IndicatorTex, IN.uv_MainTex )
						o.Albedo = rim_final.rgb; //tex2D (  _IndicatorTex, IN.uv_MainTex.xy * _IndicatorTex_ST.xy + _IndicatorTex_ST.zw).rgb ;//* _Indicator;
						o.Alpha = rim * _PhantomColor.a;
				  }
				  ENDCG

					  /*
					 // Pass: CG SHADER
						   Pass
							 {

							   Tags { "LightMode" = "Always" }
							   AlphaTest Greater [_Cutoff]
							   ZWrite Off
							   ZTest Greater

								 CGPROGRAM
								 #pragma vertex vert
								 #pragma fragment frag
								 #pragma fragmentoption ARB_precision_hint_fastest
								 #include "UnityCG.cginc"

							   sampler2D _MainTex;
							   float4 _MainTex_ST;
							   uniform float4 _Indicator;

								 struct v2f
								 {
									 float4 pos          : POSITION;
									 float2 uv           : TEXCOORD1;
								 };

								 v2f vert (appdata_full v)
								 {
									 v2f o;
									 o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
									 o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
									 return o;
								 }

								 half4 frag( v2f i ) : COLOR
								 {
								   half4 texcol = tex2D (_MainTex, i.uv);
									   return texcol * _Indicator;
								 }
								   ENDCG
							 }
					 */
					 // Pass: Fixed pipeline
					 //        Pass
					 //      {          
					 //          Tags { "LightMode" = "Always" }
					 //          AlphaTest Greater [_Cutoff]
					 //          ZWrite Off
					 //          ZTest Greater
					 // 
					 //          SetTexture [_MainTex]
					 //          {
					 //              constantColor [_Indicator]
					 //              //combine constant, texture
					 //              combine constant* texture
					 //          }
					 //      }

		}



			Fallback "Diffuse", 0

}