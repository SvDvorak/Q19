Shader "Hidden/DownSampling2"
{
	HLSLINCLUDE

		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		Texture2D _MainTex;
		SamplerState point_clamp_sampler;

		uniform int _DownSampling;

		float4 frag (VaryingsDefault i) : SV_Target
		{
			half2 newUV = i.texcoord * _ScreenParams.xy;
			float u = (floor(newUV.x / _DownSampling)*_DownSampling) / _ScreenParams.x + 0.0001f;
			float v = (floor(newUV.y / _DownSampling)*_DownSampling) / _ScreenParams.y + 0.0001f;

			return _MainTex.Sample(point_clamp_sampler, float2(u,v));


			// fixed4 col = tex2D(_MainTex, i.uv);
			// just invert the colors
			// col.rgb = 1 - col.rgb;
			// return col;
		}

	ENDHLSL

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment frag

            ENDHLSL
        }
    }
}
