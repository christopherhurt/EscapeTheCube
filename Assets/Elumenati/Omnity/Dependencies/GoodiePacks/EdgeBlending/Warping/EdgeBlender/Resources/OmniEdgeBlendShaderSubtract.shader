// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Elumenati/EdgeBlendShaderSubtract" {
	Properties {
		_Value("_Value", Float) = .2
	}
	
	Subshader {
		ZTest Always
		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Fog { Mode off }      

		Tags {"Queue" = "Geometry+2" }
		Pass {
			Tags {"Queue" = "Geometry+2" }
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct v2f {
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half4 color : TEXCOORD1;
			};

			v2f vert( appdata_full v ) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.color = v.color;
				return o;
			} 

			half _Value= .2;	

			half4 frag(v2f i) : COLOR {
			return half4 ( 0,0,0,_Value);
				
			}
			ENDCG 	
		}
	}
}