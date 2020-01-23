
Shader "Hidden/VolumetricLightDirectional"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			ZTest Off Cull Off ZWrite Off 
			Blend One One, One Zero

			CGPROGRAM
			#pragma vertex vertDir
			#pragma fragment fragDir
			#pragma shader_feature USE_SHADOWS
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "VolumetricLights.cginc"

			// fixed _Intensity;
			float4 _FrustumCorners[4];

			#define SAMPLES 8
			#define EXTINCTION .1
			#define MAXRAYLENGTH 50
			


			struct v2fDir
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 wpos : TEXCOORD1;
#if defined (USE_SHADOWS)
				float4x4 w2s0 : TEXCOORD2; //3/4/5
				float4x4 w2s1 : TEXCOORD6; //7/8/9
				float4x4 w2s2 : TEXCOORD10; //11/12/13
				float4x4 w2s3 : TEXCOORD14; //15/16/17
#endif
			};	

#if defined (USE_SHADOWS)

			sampler2D_float _CachedShadowMap;			
			sampler2D _World2ShadowTex;

			#define TEXEL_SIZE .25

			inline fixed4 GetCascadeWeights_SplitSpheres(float3 wpos) {
				//fromt center...
				float3 fc0 = wpos - unity_ShadowSplitSpheres[0].xyz;
				float3 fc1 = wpos - unity_ShadowSplitSpheres[1].xyz;
				float3 fc2 = wpos - unity_ShadowSplitSpheres[2].xyz;
				float3 fc3 = wpos - unity_ShadowSplitSpheres[3].xyz;
				fixed4 wgts = float4(float4(dot(fc0, fc0), dot(fc1, fc1), dot(fc2, fc2), dot(fc3, fc3)) < unity_ShadowSplitSqRadii);
				wgts.yzw = saturate(wgts.yzw - wgts.xyz);
				return wgts;
			}
				
			float4 W2S (float x, float y) { 
				return tex2Dlod(_World2ShadowTex, float4(TEXEL_SIZE * x, TEXEL_SIZE * y, 0, 0)); 
			}
			void BuildWorld2ShadowMatricesVert (out float4x4 w2s0, out float4x4 w2s1, out float4x4 w2s2, out float4x4 w2s3) {
				float4 _00s = W2S(0, 0); float4 _10s = W2S(1, 0); float4 _20s = W2S(2, 0); float4 _30s = W2S(3, 0);
				float4 _01s = W2S(0, 1); float4 _11s = W2S(1, 1); float4 _21s = W2S(2, 1); float4 _31s = W2S(3, 1);
				float4 _02s = W2S(0, 2); float4 _12s = W2S(1, 2); float4 _22s = W2S(2, 2); float4 _32s = W2S(3, 2);
				float4 _03s = W2S(0, 3); float4 _13s = W2S(1, 3); float4 _23s = W2S(2, 3); float4 _33s = W2S(3, 3);
				w2s0[0][0] = _00s.r; w2s0[1][0] = _10s.r; w2s0[2][0] = _20s.r; w2s0[3][0] = _30s.r;
				w2s0[0][1] = _01s.r; w2s0[1][1] = _11s.r; w2s0[2][1] = _21s.r; w2s0[3][1] = _31s.r;
				w2s0[0][2] = _02s.r; w2s0[1][2] = _12s.r; w2s0[2][2] = _22s.r; w2s0[3][2] = _32s.r;
				w2s0[0][3] = _03s.r; w2s0[1][3] = _13s.r; w2s0[2][3] = _23s.r; w2s0[3][3] = _33s.r;
				w2s1[0][0] = _00s.g; w2s1[1][0] = _10s.g; w2s1[2][0] = _20s.g; w2s1[3][0] = _30s.g;
				w2s1[0][1] = _01s.g; w2s1[1][1] = _11s.g; w2s1[2][1] = _21s.g; w2s1[3][1] = _31s.g;
				w2s1[0][2] = _02s.g; w2s1[1][2] = _12s.g; w2s1[2][2] = _22s.g; w2s1[3][2] = _32s.g;
				w2s1[0][3] = _03s.g; w2s1[1][3] = _13s.g; w2s1[2][3] = _23s.g; w2s1[3][3] = _33s.g;
				w2s2[0][0] = _00s.b; w2s2[1][0] = _10s.b; w2s2[2][0] = _20s.b; w2s2[3][0] = _30s.b;
				w2s2[0][1] = _01s.b; w2s2[1][1] = _11s.b; w2s2[2][1] = _21s.b; w2s2[3][1] = _31s.b;
				w2s2[0][2] = _02s.b; w2s2[1][2] = _12s.b; w2s2[2][2] = _22s.b; w2s2[3][2] = _32s.b;
				w2s2[0][3] = _03s.b; w2s2[1][3] = _13s.b; w2s2[2][3] = _23s.b; w2s2[3][3] = _33s.b;
				w2s3[0][0] = _00s.a; w2s3[1][0] = _10s.a; w2s3[2][0] = _20s.a; w2s3[3][0] = _30s.a;
				w2s3[0][1] = _01s.a; w2s3[1][1] = _11s.a; w2s3[2][1] = _21s.a; w2s3[3][1] = _31s.a;
				w2s3[0][2] = _02s.a; w2s3[1][2] = _12s.a; w2s3[2][2] = _22s.a; w2s3[3][2] = _32s.a;
				w2s3[0][3] = _03s.a; w2s3[1][3] = _13s.a; w2s3[2][3] = _23s.a; w2s3[3][3] = _33s.a;
			}
			

			inline float3 GetCascadeShadowCoord(v2fDir i, float4 pos, fixed4 wgt)
			{
				float3 coord = float3(
					mul(i.w2s0, pos).xyz * wgt[0] + 
					mul(i.w2s1, pos).xyz * wgt[1] + 
					mul(i.w2s2, pos).xyz * wgt[2] + 
					mul(i.w2s3, pos).xyz * wgt[3]
				);

#if defined(UNITY_REVERSED_Z)
				float noCascadeWeights = 1 - dot(wgt, float4(1, 1, 1, 1));
				coord.z += noCascadeWeights;
#endif
				return coord;
			}
#endif

			float LightAttenDir(v2fDir i, float3 wpos)
			{
				float atten = 1;
#if defined (USE_SHADOWS)
				float3 samplePos = GetCascadeShadowCoord(i, float4(wpos, 1), GetCascadeWeights_SplitSpheres(wpos));
				atten = (tex2D(_CachedShadowMap, samplePos.xy).r < samplePos.z) ? SHADOW_OFF : SHADOW_POWER;
#endif
				return atten;
			}

				
			float4 RayMarchDirectional(v2fDir v2fDIR, float2 screenPos, float3 rayStart, float3 rayDir, float rayLength)
			{
				float offset = GetDitherOffset(screenPos);
    
				float stepSize = rayLength / SAMPLES;
				float3 step = rayDir * stepSize;
				float3 wPos = rayStart + step * offset;
				
				float vlight = 0;
				float extinction = 0;
				
				float extinct = EXTINCTION * stepSize;

				[unroll] for (int i = 0; i < SAMPLES; ++i)
				{
					float density = GetDensity(wPos, _NoiseData.y);
					float atten = LightAttenDir(v2fDIR, wPos);

					extinction += extinct * density * saturate(atten);
					
					vlight += stepSize * density * exp(-extinction) * atten;
					vlight = max(0, vlight);
					
					wPos += step;				
				}

				vlight *= k_mieg;
				
				return float4 ((_LightColor.rgb * _LightColor.w * max(0, vlight)), (exp(-extinction)));
			}

			struct VSInput
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			

						
			v2fDir vertDir(VSInput i)
			{
				v2fDir o;

				o.pos = UnityObjectToClipPos(i.vertex);
				o.uv = i.uv;

				// reconstruct id from uv
				o.wpos = _FrustumCorners[o.uv.x + o.uv.y * 2];

#if defined (USE_SHADOWS)
				BuildWorld2ShadowMatricesVert ( o.w2s0, o.w2s1, o.w2s2, o.w2s3 );
#endif				
				return o;
			}

			
			fixed4 fragDir(v2fDir i) : SV_Target
			{	
				float linearDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
				
				float3 rayDir = i.wpos - _WorldSpaceCameraPos;
				rayDir *= linearDepth;
				
				float rayLength = length(rayDir);
				rayDir /= rayLength;

				rayLength = min(rayLength, MAXRAYLENGTH);
				
				float4 color = RayMarchDirectional(i, i.pos.xy, _WorldSpaceCameraPos, rayDir, rayLength);
				
				if (linearDepth > 0.999999)
					color.w = .5;
				
				color = lerp(fixed4(0,0,0,1), color, min(1, _LightColor.w));
				
				return color;
			}
			ENDCG
		}
	}
}