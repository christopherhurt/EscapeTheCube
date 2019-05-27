// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Elumenati/EdgeBlendVertexColor" {
	Properties{
		_MainTex("_MainTex", 2D) = "black" { }
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
	}
		SubShader{
			Pass {
				cull off
				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				sampler2D _MainTex;
			float4 _MainTex_ST;
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
					float2  uv : TEXCOORD0;
					float2  uv2 : TEXCOORD1;
				};
				float _Gamma1 = 1;
				float _Gamma2 = 1;
				float _Gamma3 = 1;
				float _Gamma4 = 1;
				float _floor = 0;
				float _floorGamma = 1;
				float _floorOverall = 0;

				float _floorGammaOffset = 0;
				float _floorGammaWidth = 1;

				v2f vert(appdata v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.uv2 = TRANSFORM_TEX(v.texcoord2, _MainTex);
#if UNITY_VERSION >= 540
					float4x4 modelMatrixInverse = unity_WorldToObject;
#else
					// if this shader doesn't compile use this line... the preprocessor works, but the unity shader does this wrong.
					float4x4 modelMatrixInverse = unity_WorldToObject;
#endif
					o.col = v.col;

					o.col.xyz = pow(o.col.xyz ,_Gamma1);
					return o;
				}
				fixed4 frag(v2f i) : COLOR {
					float4 color = i.col;
					color = pow(color ,_Gamma2);

					float4 color2 = tex2D(_MainTex, i.uv);

					color2.a = 1;
					color2.xyz = pow(color2.xyz,_Gamma3) * color;
					color2.xyz = pow(color2.xyz ,_Gamma4);

					// OtherFloor
					//(color.x>=1?1:0); // stairstep function

					float t = saturate((color.x - _floorGammaOffset) / _floorGammaWidth);
					float myFloor = _floor*pow(t ,_floorGamma);  //gamma function
					return (color2 + _floorOverall + myFloor)* tex2D(_MaskTex, i.uv2).xxxx;
				}
				ENDCG
			}
		}
}