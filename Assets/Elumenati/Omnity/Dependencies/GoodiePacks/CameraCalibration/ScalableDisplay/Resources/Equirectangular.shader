// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Elumenati/Omnity/Equirectangular/Source"
{
	Properties
	{
		_Cam0Tex("Cam0 (RGB)", 2D) = "black" {}
		_Cam1Tex("Cam1 (RGB)", 2D) = "black" {}
		_Cam2Tex("Cam2 (RGB)", 2D) = "black" {}
		_Cam3Tex("Cam3 (RGB)", 2D) = "black" {}
		_Cam4Tex("Cam4 (RGB)", 2D) = "black" {}
		_Cam5Tex("Cam5 (RGB)", 2D) = "black" {}

		_yOffset("YOffset", float) = 0.0
		_xScale("xScale", float) = 1.0
		_yScale("yScale", float) = 1.0
		_scale("_scale",vector) = (1,1,1,1)

		_zScale("ZScale", float) = 2.0 // 2.0 for directX 1.0 for opengl
		_zShift("ZShift", float) = 1.0 // 1.0 for directX 0.0 for opengl
		_LensZoomToHeight("_LensZoomToHeight", float) = 1.0 // for 0, the fisheye is the width of the screen, 1 the fisheye is the height of the screen(fulldome)
		_AspectRatio("AspectRatio", float) = 1.0
	}

		SubShader
		{
			Tags { "Queue" = "Geometry" }
			Pass
			{
				Cull Off // beware of rendering artifacts from the back side coming through
				AlphaTest Greater .5
				CGPROGRAM
				#pragma target 3.0
			// remember to comment out d3d11 for unity 3.5
			#pragma only_renderers d3d9 d3d11 OpenGL
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
					half4 pos 			 : SV_POSITION;
					half4 Cam0CoordProj : TEXCOORD0;
					half4 Cam1CoordProj : TEXCOORD1;
					half4 Cam2CoordProj : TEXCOORD2;
					half4 Cam3CoordProj : TEXCOORD3;
					half4 Cam4CoordProj : TEXCOORD4;
					half4 Cam5CoordProj : TEXCOORD5;
					half radius  		 : TEXCOORD6;
			};

			uniform half4x4 _Cam0Matrix;
			uniform half4x4 _Cam1Matrix;
			uniform half4x4 _Cam2Matrix;
			uniform half4x4 _Cam3Matrix;
			uniform half4x4 _Cam4Matrix;
			uniform half4x4 _Cam5Matrix;

			uniform half4x4 _MVPNoScaleMatrix;

			uniform half _yOffset;
			uniform half _AspectRatio;
			uniform half _xScale;
			uniform half _yScale;
			uniform half _zScale;
			uniform half _zShift;
			uniform half _LensZoomToHeight;
			uniform half3 _scale;
			uniform half _yShift = 0;// this is a decoy varible 0 for !theta 1 for f-theta

bool TestCoordinateInside01FAST(half4 inProj,half2 iny)
{
	//return true;
		return inProj.w >= 0.0 && all(half4(iny.xy, 1 - iny.xy) >= 0);
		//	return (inProj.w >= 0.0 &&iny.x >= 0.0 && iny.x <= 1.0 && iny.y >= 0.0 && iny.y <= 1.0);
		}

		half4 CalculateColorInit(half4 TextCoordProj, sampler2D _CamTex)
		{
			half2 CamTexCoords = TextCoordProj.xy / TextCoordProj.w;
			return half4(tex2D(_CamTex,CamTexCoords.xy).xyz,1)* TestCoordinateInside01FAST(TextCoordProj,CamTexCoords);
		}

		void CalculateColor(half4 TextCoordProj, sampler2D _CamTex, inout half4 inColor)
		{
			half2 CamTexCoords = TextCoordProj.xy / TextCoordProj.w;

			inColor = TestCoordinateInside01FAST(TextCoordProj,CamTexCoords) ? half4(tex2D(_CamTex,CamTexCoords.xy).xyz,1) : inColor;
		}

					v2f vert(appdata_base v)
					{
							v2f o;
							v.vertex.xyz = v.vertex.xyz * _scale;
							half4 target = UnityObjectToClipPos(v.vertex);
							if (_LensZoomToHeight < 0) {
									o.radius = 0;
									o.pos = target;
							}
		 else {
		  o.radius = 1;
		  o.pos = UnityObjectToClipPos( half4(v.texcoord.xy * 2 - 1 , 0 , 1));
	  }
	  o.Cam0CoordProj = mul(_Cam0Matrix, v.vertex);
	  o.Cam1CoordProj = mul(_Cam1Matrix, v.vertex);
	  o.Cam2CoordProj = mul(_Cam2Matrix, v.vertex);
	  o.Cam3CoordProj = mul(_Cam3Matrix, v.vertex);
	  o.Cam4CoordProj = mul(_Cam4Matrix, v.vertex);
	  o.Cam5CoordProj = mul(_Cam5Matrix, v.vertex);
	  return o;
}

half EaseIntPoint(half value)
{
	half val = clamp((value - .5) *2.0 ,0,1);
	val *= .345;
	return cos(val *3.1415);
}

		uniform sampler2D _Cam0Tex;
		uniform sampler2D _Cam1Tex;
		uniform sampler2D _Cam2Tex;
		uniform sampler2D _Cam3Tex;
		uniform sampler2D _Cam4Tex;
		uniform sampler2D _Cam5Tex;
		half4 frag(v2f i) : COLOR
		{
			half4 outColor = CalculateColorInit(i.Cam0CoordProj,_Cam0Tex);
			CalculateColor(i.Cam1CoordProj,_Cam1Tex,outColor);
			CalculateColor(i.Cam2CoordProj,_Cam2Tex,outColor);
			CalculateColor(i.Cam3CoordProj,_Cam3Tex,outColor);
			CalculateColor(i.Cam4CoordProj,_Cam4Tex,outColor);
			CalculateColor(i.Cam5CoordProj,_Cam5Tex,outColor);
			return outColor;
		}

	ENDCG
	}
		}
}