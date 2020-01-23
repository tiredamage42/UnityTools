Shader "Hidden/EdgeDetect"
{
	HLSLINCLUDE

        #include "ImgFX.hlsl"

		sampler2D _CameraDepthNormalsTexture;
        sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		

        half4 _Params;
        #define DEPTH_SENSITIVITY _Params.x
        #define NORMAL_SENSITIVITY _Params.y
        #define SAMPLE_DISTANCE _Params.z
        #define BACKGROUND_FADE _Params.w
        //Settings
		half4 _BgColor;
        
        
		
		struct Varyings
		{
			float4 vertex : SV_POSITION;
			float2 texcoord[5] : TEXCOORD0;
		};

        int CheckNormal (half2 centerNormal, half4 theSample) {
            // difference in normals
			// do not bother decoding normals - there's no need here
			half2 diff = abs(centerNormal - theSample.xy) * NORMAL_SENSITIVITY;
			int isSameNormal = (diff.x + diff.y) * NORMAL_SENSITIVITY < 0.1;
            return isSameNormal;
        }

		//--------------------------------------------------------------------------------------------------------------------------------

		inline half CheckSame (half2 centerNormal, float centerDepth, half4 theSample)
		{
            int isSameNormal = CheckNormal (centerNormal, theSample);
			
            // difference in depth
			float sampleDepth = DecodeFloatRG (theSample.zw);
			float zdiff = abs(centerDepth-sampleDepth);
			// scale the required threshold by the distance
			int isSameDepth = zdiff * DEPTH_SENSITIVITY < 0.09 * centerDepth;
			
			// return:
			// 1 - if normals and depth are similar enough
			// 0 - otherwise
			return isSameNormal * isSameDepth ? 1.0 : 0.0;
		}

		//--------------------------------------------------------------------------------------------------------------------------------

		Varyings VertRobert(float4 vertex : POSITION)
        {
			Varyings o;
            StartVertex(vertex, o.vertex, o.texcoord[0]);
            float2 texelSize = _MainTex_TexelSize.xy * SAMPLE_DISTANCE;
            o.texcoord[1] = o.texcoord[0] + texelSize * half2(1,1);
			o.texcoord[2] = o.texcoord[0] + texelSize * half2(-1,-1);
			o.texcoord[3] = o.texcoord[0] + texelSize * half2(-1,1);
			o.texcoord[4] = o.texcoord[0] + texelSize * half2(1,-1);
			return o;
		}
		float4 FragRobert(Varyings i) : SV_Target
		{
            half4 color = tex2D(_MainTex, i.texcoord[0]);

			half4 sample1 = tex2D(_CameraDepthNormalsTexture, i.texcoord[1].xy);
			half4 sample2 = tex2D(_CameraDepthNormalsTexture, i.texcoord[2].xy);
			half4 sample3 = tex2D(_CameraDepthNormalsTexture, i.texcoord[3].xy);
			half4 sample4 = tex2D(_CameraDepthNormalsTexture, i.texcoord[4].xy);

			half edge = 1.0;
			edge *= CheckSame(sample1.xy, DecodeFloatRG(sample1.zw), sample2);
			edge *= CheckSame(sample3.xy, DecodeFloatRG(sample3.zw), sample4);

			return edge * lerp(color, _BgColor, BACKGROUND_FADE);
		}



	ENDHLSL
	
	Subshader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			HLSLPROGRAM
				#pragma vertex VertRobert
				#pragma fragment FragRobert
			ENDHLSL
		}
	}

	Fallback off
}