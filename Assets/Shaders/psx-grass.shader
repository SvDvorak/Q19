// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// USING CUTOUT AND NOT PARTIAL TRANSPARANCY

Shader "psx/grass" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_WindSpeed("Wind speed", float) = 0
		_SwayStrength("Sway strength", float) = 0
		_GrassLayer("Grass layer", float) = 0
	}
		SubShader{
			Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
			LOD 200

			Lighting Off

			Pass{
				CGPROGRAM

					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"
					#include "ClassicNoise2D.hlsl"

						struct v2f
						{
							fixed4 pos : SV_POSITION;
							half4 color : COLOR0;
							half4 colorFog : COLOR1;
							float2 uv_MainTex : TEXCOORD0;
							half3 normal :TEXCOORD1;
						};

						float4 _MainTex_ST;
						float _SwayStrength;
						float _WindSpeed;
						float _GrassLayer;
						uniform half4 unity_FogStart;
						uniform half4 unity_FogEnd;
						static const int _SnapResWidth = 320;
						static const int _SnapResHeight = 180;

						float noice(float2 pos, float offset) {
							return cnoise(pos + float2(offset, offset));
						}

						float2 uv_noise(float2 original_uv) : TEXCOORD0 {
						    float val = _Time * 2 * _WindSpeed;
							// Shift time to create uneveness in the wind speed
							float offsetX = val + sin(val * 0.25) * 0.2 + sin(val * 2) * 0.1 + sin(val * 4 + 0.6) * 0.05;
							float offsetY = val + sin(val * 0.2 + 1) * 0.19 + sin(val * 4) * 0.11 + sin(val * 8 + 3) * 0.05;
							// Create perlin noise in different frequencies
							float valueX =
								noice(original_uv * 0.3, offsetX) * noice(original_uv * 0.3, offsetX) * 0.5 +
								noice(original_uv * 2, offsetX) * 0.2 +
								noice(original_uv * 4, offsetX) * 0.07;
							float valueY =
								noice(original_uv * 0.3, offsetY + 0.1) * noice(original_uv * 0.3, offsetY + 0.1) * 0.5 +
								noice(original_uv * 2, offsetY + 0.1) * 0.2 +
								noice(original_uv * 4, offsetX + 0.1) * 0.07;
							return float2(valueX, valueY) * _SwayStrength * _GrassLayer;
						}

						v2f vert(appdata_full v)
						{
							v2f o;

							//Vertex snapping
							float4 snapToPixel = UnityObjectToClipPos(v.vertex);
							float4 vertex = snapToPixel;
							vertex.xyz = snapToPixel.xyz / snapToPixel.w;
							vertex.x = floor(_SnapResWidth * vertex.x) / _SnapResWidth;
							vertex.y = floor(_SnapResHeight * vertex.y) / _SnapResHeight;
							vertex.xyz *= snapToPixel.w;
							o.pos = vertex;

							//Vertex lighting 
							o.color = v.color*UNITY_LIGHTMODEL_AMBIENT;

							float distance = length(mul(UNITY_MATRIX_MV,v.vertex));

							o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);

							o.normal = distance + (vertex.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2;

							//Fog
							float4 fogColor = unity_FogColor;

							float fogDensity = (unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart);
							o.normal.g = fogDensity;
							o.normal.b = 1;

							o.colorFog = fogColor;
							o.colorFog.a = clamp(fogDensity,0,1);

							//Cut out polygons
							if (distance > unity_FogStart.z + unity_FogColor.a * 255)
							{
								o.pos.w = 0;
							}


							return o;
						}

						sampler2D _MainTex;

						float4 frag(v2f IN) : COLOR
						{

							// UV texture shifting
							float2 uv_mod = uv_noise(IN.uv_MainTex);
							half4 c = tex2D(_MainTex, IN.uv_MainTex + uv_mod)*IN.color;
							//half4 c = half4(uv_mod.x, uv_mod.x, uv_mod.x, 1);
							half4 color = c*(IN.colorFog.a);

							const float alphaCut = 0.1; // AFFECTS DRAW DISTANCE TOO??
							clip(color.a - alphaCut);

							color.rgb += IN.colorFog.rgb*(1 - IN.colorFog.a);
							return color;
						}
				ENDCG
			}
	}
}