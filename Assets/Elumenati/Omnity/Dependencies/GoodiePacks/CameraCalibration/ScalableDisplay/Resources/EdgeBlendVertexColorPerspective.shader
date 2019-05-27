// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Elumenati/EdgeBlendVertexColorPerspective" {
	Properties{
		_Cam0Tex("Cam0 (RGB)", 2D) = "black" {}
	_Cam1Tex("Cam1 (RGB)", 2D) = "black" {}
	_Cam2Tex("Cam2 (RGB)", 2D) = "black" {}
	_Cam3Tex("Cam3 (RGB)", 2D) = "black" {}
	_Cam4Tex("Cam4 (RGB)", 2D) = "black" {}
	_Cam5Tex("Cam5 (RGB)", 2D) = "black" {}

	/*
#if Stereo

	_Cam0rTex("Cam0r (RGB)", 2D) = "black" {}
	_Cam1rTex("Cam1r (RGB)", 2D) = "black" {}
	_Cam2rTex("Cam2r (RGB)", 2D) = "black" {}
	_Cam3rTex("Cam3r (RGB)", 2D) = "black" {}
	_Cam4rTex("Cam4r (RGB)", 2D) = "black" {}
	_Cam5rTex("Cam5r (RGB)", 2D) = "black" {}
#endif
*/
		_MaskTex("_MaskTex", 2D) = "white" { }
		_Gamma1("_Gamma1", Float) = 1
		_Gamma2("_Gamma2", Float) = 1
		_Gamma3("_Gamma3", Float) = 1
		_Gamma4("_Gamma4", Float) = 1

		_floor("_floor", Float) = 0
		_floorOverall("_floorOverall", Float) = 0
		_floorGamma("_floorGamma", Float) = 1

		_floorGammaOffset("_floorGammaOffset", Float) = 0
		_floorGammaWidth("_floorGammaWidth", Float) = 1
		leftDegrees("leftDegrees", Float) = -90
		rightDegrees("rightDegrees ", Float) = 90
		bottomDegrees("bottomDegrees ", Float) = -37
		topDegrees("topDegrees", Float) = 37
	}
		SubShader{
			Pass {
				cull off
				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

			sampler2D _MaskTex;
			float4 _MaskTex_ST;
				struct appdata {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 texcoord : TEXCOORD0;
					float2 texcoord2 : TEXCOORD1;
					float4 col : COLOR0;
				};
				struct v2f {
					float4  pos : SV_POSITION;
					float4 col : COLOR0;
					float2  uv2 : TEXCOORD6;

#if DEBUGUV
					float4 UVraw : TEXCOORD7;
#endif

					half4 Cam0CoordProj : TEXCOORD0;
					half4 Cam1CoordProj : TEXCOORD1;
					half4 Cam2CoordProj : TEXCOORD2;
					half4 Cam3CoordProj : TEXCOORD3;
					half4 Cam4CoordProj : TEXCOORD4;
					half4 Cam5CoordProj : TEXCOORD5;
				};

				uniform half4x4 _Cam0Matrix;
				uniform half4x4 _Cam1Matrix;
				uniform half4x4 _Cam2Matrix;
				uniform half4x4 _Cam3Matrix;
				uniform half4x4 _Cam4Matrix;
				uniform half4x4 _Cam5Matrix;

				uniform sampler2D _Cam0Tex;
				uniform sampler2D _Cam1Tex;
				uniform sampler2D _Cam2Tex;
				uniform sampler2D _Cam3Tex;
				uniform sampler2D _Cam4Tex;
				uniform sampler2D _Cam5Tex;
#if Stereo

				uniform sampler2D _Cam0rTex;
				uniform sampler2D _Cam1rTex;
				uniform sampler2D _Cam2rTex;
				uniform sampler2D _Cam3rTex;
				uniform sampler2D _Cam4rTex;
				uniform sampler2D _Cam5rTex;
#endif

				float _Gamma1 = 1;
				float _Gamma2 = 1;
				float _Gamma3 = 1;
				float _Gamma4 = 1;
				float _floor = 0;
				float _floorGamma = 1;
				float _floorOverall = 0;

				float _floorGammaOffset = 0;
				float _floorGammaWidth = 1;

				float leftDegrees = -90;
				float rightDegrees = 90;
				float bottomDegrees = -37;
				float topDegrees = 37;

				float3 lerp(float3 a, float3 b, float T)
				{
					if (T < 0) {
						return a;
					}
					else if (T >= 1) {
						return b;
					}
 else {
  return a + T*(b - a);
}
}

float remap(float value, float low1, float high1, float low2, float high2) {
	return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
}

bool TestCoordinateInside01FAST(half4 inProj, half2 iny)
{
	//return true;
	return inProj.w >= 0.0 && all(half4(iny.xy, 1 - iny.xy) >= 0);
	//	return (inProj.w >= 0.0 &&iny.x >= 0.0 && iny.x <= 1.0 && iny.y >= 0.0 && iny.y <= 1.0);
}

half4 CalculateColorInit(half4 TextCoordProj, sampler2D _CamTex)
{
	half2 CamTexCoords = TextCoordProj.xy / TextCoordProj.w;
	return half4(tex2D(_CamTex, CamTexCoords.xy).xyz, 1)* TestCoordinateInside01FAST(TextCoordProj, CamTexCoords);
}

void CalculateColor(half4 TextCoordProj, sampler2D _CamTex, inout half4 inColor)
{
	half2 CamTexCoords = TextCoordProj.xy / TextCoordProj.w;

	inColor = TestCoordinateInside01FAST(TextCoordProj, CamTexCoords) ? half4(tex2D(_CamTex, CamTexCoords.xy).xyz, 1) : inColor;
	//inColor = TestCoordinateInside01FAST(TextCoordProj, CamTexCoords) ? half4(CamTexCoords.xy,0, 1) : inColor;
  }

  v2f vert(appdata v) {
	  v2f o;
	  o.pos = UnityObjectToClipPos(v.vertex);
	  //					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
						  o.uv2 = TRANSFORM_TEX(v.texcoord2, _MaskTex);
	  #if UNITY_VERSION >= 540
						  float4x4 modelMatrixInverse = unity_WorldToObject;
	  #else
						  // if this shader doesn't compile use this line... the preprocessor works, but the unity shader does this wrong.
						  float4x4 modelMatrixInverse = unity_WorldToObject;
	  #endif
						  o.col = v.col;

						  o.col.xyz = pow(o.col.xyz ,_Gamma1);

						  float2 UV_unit = v.texcoord;

						  UV_unit.x = 1 - UV_unit.x;

						  float pi = 3.14;

						  float Deg2Rad = 0.0174533;

						  //#define DEBUGXYZ 1
											  UV_unit.x = lerp((-leftDegrees + 180) / 360, (-rightDegrees + 180) / 360, 1 - UV_unit.x) - .25;
											  UV_unit.y = lerp((-bottomDegrees + 90) / 180, (-topDegrees + 90) / 180, UV_unit.y);

											  float	theta = UV_unit.x * 2 * pi;
											  float	phi = UV_unit.y *pi;

											  float4 XYZ;
											  // note that the handle will be off since the bounding box is not updated...
											  XYZ.x = sin(phi) * cos(theta);
											  XYZ.z = sin(phi) * sin(theta);
											  XYZ.y = cos(phi);
											  XYZ.w = 1;

						  #if DEBUGUV
											  o.UVraw = XYZ;
											  o.UVraw.zw = UV_unit.xy;
						  #endif

						  #if DEBUGXYZ
											  o.pos = UnityObjectToClipPos(XYZ);
						  #endif

											  o.Cam0CoordProj = mul(_Cam0Matrix, XYZ);
											  o.Cam1CoordProj = mul(_Cam1Matrix, XYZ);
											  o.Cam2CoordProj = mul(_Cam2Matrix, XYZ);
											  o.Cam3CoordProj = mul(_Cam3Matrix, XYZ);
											  o.Cam4CoordProj = mul(_Cam4Matrix, XYZ);
											  o.Cam5CoordProj = mul(_Cam5Matrix, XYZ);

											  return o;
										  }

										  fixed4 frag(v2f i) : COLOR {
											  float4 color = i.col;
											  color = pow(color ,_Gamma2);

											  half4 outColor = CalculateColorInit(i.Cam0CoordProj, _Cam0Tex);
											  CalculateColor(i.Cam1CoordProj, _Cam1Tex, outColor);
											  CalculateColor(i.Cam2CoordProj, _Cam2Tex, outColor);
											  CalculateColor(i.Cam3CoordProj, _Cam3Tex, outColor);
											  CalculateColor(i.Cam4CoordProj, _Cam4Tex, outColor);
											  CalculateColor(i.Cam5CoordProj, _Cam5Tex, outColor);

#if Stereo

											  half4 outColorR = CalculateColorInit(i.Cam0CoordProj, _Cam0rTex);
											  CalculateColor(i.Cam1CoordProj, _Cam1rTex, outColorR);
											  CalculateColor(i.Cam2CoordProj, _Cam2rTex, outColorR);
											  CalculateColor(i.Cam3CoordProj, _Cam3rTex, outColorR);
											  CalculateColor(i.Cam4CoordProj, _Cam4rTex, outColorR);
											  CalculateColor(i.Cam5CoordProj, _Cam5rTex, outColorR);
											  outColor = outColor *.5 + outColorR*.5;
#endif

						  #if DEBUGUV
											  outColor = half4(i.UVraw.zw, 0, 1);
											  //					outColor = half4(i.UVraw.xyz, 1);
											  #endif
											  //					outColor = outColor* tex2D(_MaskTex, i.uv2).xxxx;

																  color = pow(color, _Gamma2);
																  outColor = outColor* color.xxxx;

																  return outColor;
															  }

															  ENDCG
														  }
		}
}