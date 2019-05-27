// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Elumenati/ViosoShader" {
	Properties{
		_Cam0Tex("Cam0 (RGB)", 2D) = "black" {}
	_Cam1Tex("Cam1 (RGB)", 2D) = "black" {}
	_Cam2Tex("Cam2 (RGB)", 2D) = "black" {}
	_Cam3Tex("Cam3 (RGB)", 2D) = "black" {}
	_Cam4Tex("Cam4 (RGB)", 2D) = "black" {}
	_Cam5Tex("Cam5 (RGB)", 2D) = "black" {}
	_BlendTexture("_BlendTexture", 2D) = "white" {}

	_Gamma1("_Gamma1", Float) = 1
		leftDegrees("leftDegrees", Float) = -90
		rightDegrees("rightDegrees ", Float) = 90
		bottomDegrees("bottomDegrees ", Float) = -37
		topDegrees("topDegrees", Float) = 37
	}
		SubShader{
		Pass{
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
		float2 texcoord : TEXCOORD0;
		float4 col : COLOR0;
	};
	struct v2f {
		float4  pos : SV_POSITION;
		float2  posPixel : COLOR0;
		float2  uv : TEXCOORD6;

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
	uniform sampler2D _BlendTexture;
	uniform float4 _BlendTexture_ST;

	uniform float _Gamma1 = 1;

	uniform	float leftDegrees = -90;
	uniform	float rightDegrees = 90;
	uniform	float bottomDegrees = -37;
	uniform	float topDegrees = 37;

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
		return inProj.w >= 0.0 && all(half4(iny.xy, 1 - iny.xy) >= 0);
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
	}

	v2f vert(appdata v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
#if UNITY_VERSION >= 540
		float4x4 modelMatrixInverse = unity_WorldToObject;
#else
		// if this shader doesn't compile use this line... the preprocessor works, but the unity shader does this wrong.
		float4x4 modelMatrixInverse = unity_WorldToObject;
#endif

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

#if DEBUGXYZ
		o.pos = UnityObjectToClipPos(XYZ);
#endif

		o.Cam0CoordProj = mul(_Cam0Matrix, XYZ);
		o.Cam1CoordProj = mul(_Cam1Matrix, XYZ);
		o.Cam2CoordProj = mul(_Cam2Matrix, XYZ);
		o.Cam3CoordProj = mul(_Cam3Matrix, XYZ);
		o.Cam4CoordProj = mul(_Cam4Matrix, XYZ);
		o.Cam5CoordProj = mul(_Cam5Matrix, XYZ);

		o.posPixel = TRANSFORM_TEX(o.pos, _BlendTexture).xy;

		//		o.posPixel = o.pos;

		return o;
	}

fixed4 frag(v2f i) : COLOR{
	half4 outColor = CalculateColorInit(i.Cam0CoordProj, _Cam0Tex);
	CalculateColor(i.Cam1CoordProj, _Cam1Tex, outColor);
	CalculateColor(i.Cam2CoordProj, _Cam2Tex, outColor);
	CalculateColor(i.Cam3CoordProj, _Cam3Tex, outColor);
	CalculateColor(i.Cam4CoordProj, _Cam4Tex, outColor);
	CalculateColor(i.Cam5CoordProj, _Cam5Tex, outColor);
	// in the case where its omniplayer poster warp....
	//	outColor = tex2D(_Cam0Tex, i.uv.xy)*color;

	float color = tex2D(_BlendTexture, i.posPixel.xy);
	color = pow(color, _Gamma1);

	outColor.xyz *= color.xxx;
	return outColor;
}

ENDCG
		}
	}
}