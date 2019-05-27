// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Elumenati/ViosoShaderFlat" {
	Properties{
		_Cam0Tex("Cam0 (RGB)", 2D) = "black" {}

		_BlendTexture("_BlendTexture (R)", 2D) = "white" {}

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

	};

	uniform sampler2D _Cam0Tex;
	uniform sampler2D _BlendTexture;
	uniform float4 _BlendTexture_ST;
	uniform float4 _Cam0Tex_ST;

	uniform float _Gamma1 = 1;


	v2f vert(appdata v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _Cam0Tex);
#if UNITY_VERSION >= 540
		float4x4 modelMatrixInverse = unity_WorldToObject;
#else
		// if this shader doesn't compile use this line... the preprocessor works, but the unity shader does this wrong.
		float4x4 modelMatrixInverse = unity_WorldToObject;
#endif
		o.posPixel = TRANSFORM_TEX(o.pos, _BlendTexture).xy;
		return o;
	}

fixed4 frag(v2f i) : COLOR{

	// in the case where its omniplayer poster warp....
	float4 outColor = tex2D(_Cam0Tex, i.uv.xy);

	float color = tex2D(_BlendTexture, i.posPixel.xy);
	color = pow(color, _Gamma1);

	outColor.xyz *= color.xxx;
	return outColor;
}

ENDCG
		}
	}
}