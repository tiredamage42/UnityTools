Shader "Hidden/BilateralBlur"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Off Blend Off

		CGINCLUDE
        // #define FULL_RES_BLUR_KERNEL_SIZE 7
        // #define HALF_RES_BLUR_KERNEL_SIZE 5
        // #define QUARTER_RES_BLUR_KERNEL_SIZE 6

		#define KERNEL_SIZE 7
        #define BLUR_DEPTH_FACTOR 0.5
        #define GAUSS_BLUR_DEVIATION 1.5        

		#define PI 3.1415927

		#include "../../PostProcessStack/Resources/ImgFX.hlsl"

        UNITY_DECLARE_TEX2D(_CameraDepthTexture);        
        UNITY_DECLARE_TEX2D(_MainTex);

		float GaussianWeight(float offset, float deviation) {
			float weight = 1.0 / sqrt(2.0 * PI * deviation * deviation);
			weight *= exp(-(offset * offset) / (2.0 * deviation * deviation));
			return weight;
		}

		void Iteration (const float deviation, inout float4 color, inout float weightSum, float centerDepth, v2f input, int2 direction, Texture2D depth, SamplerState depthSampler, const int i) {
		
			float2 offset = (direction * i);
			float4 sampleColor = _MainTex.Sample(sampler_MainTex, input.uv, offset);
			float sampleDepth = (LinearEyeDepth(depth.Sample(depthSampler, input.uv, offset)));

			float depthDiff = abs(centerDepth - sampleDepth);
			float dFactor = depthDiff * BLUR_DEPTH_FACTOR;
			float w = exp(-(dFactor * dFactor));
			
			// gaussian weight is computed from constants only -> will be computed in compile time
			float weight = GaussianWeight(i, deviation) * w;

			color += weight * sampleColor;
			weightSum += weight;
		}

		float4 BilateralBlur(v2f input, int2 direction, Texture2D depth, SamplerState depthSampler, const int kernelRadius) {
			const float deviation = kernelRadius / GAUSS_BLUR_DEVIATION; // make it really strong

			float2 uv = input.uv;
			float4 centerColor = _MainTex.Sample(sampler_MainTex, uv);
			
			float4 color = centerColor;
			float centerDepth = (LinearEyeDepth(depth.Sample(depthSampler, uv)));

			float weightSum = 0;

			// gaussian weight is computed from constants only -> will be computed in compile time
            float weight = GaussianWeight(0, deviation);
			color *= weight;
			weightSum += weight;
						
			[unroll] for (int i = -kernelRadius; i < 0; i += 1) {
				Iteration (deviation, color, weightSum, centerDepth, input, direction, depth, depthSampler, i);
			}
			[unroll] for (i = 1; i <= kernelRadius; i += 1) {
				Iteration (deviation, color, weightSum, centerDepth, input, direction, depth, depthSampler, i);
			}
			color /= weightSum;
			return color;
		}

		ENDCG
		Pass {
			CGPROGRAM
			#pragma vertex ImgFXVertex
            #pragma fragment horizontalFrag
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 horizontalFrag(v2f input) : SV_Target {
				return BilateralBlur(input, int2(1, 0), _CameraDepthTexture, sampler_CameraDepthTexture, KERNEL_SIZE);
			}
			ENDCG
		}
		Pass {
			CGPROGRAM
			#pragma vertex ImgFXVertex
            #pragma fragment verticalFrag
            #pragma fragmentoption ARB_precision_hint_fastest
			fixed4 verticalFrag(v2f input) : SV_Target {
				return BilateralBlur(input, int2(0, 1), _CameraDepthTexture, sampler_CameraDepthTexture, KERNEL_SIZE);
			}
			ENDCG
		}
	}
}