
Shader "Hidden/VolumetricLightPoint"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE
		#define POINT
		#include "VolumetricLightsSpotPoint.cginc"
		ENDCG

		// pass 0 - point light, camera inside
		Pass
		{
			ZTest Off Cull Front ZWrite Off Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragPointInside
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma shader_feature USE_SHADOWS
			#pragma shader_feature USE_COOKIE
			ENDCG
		}
		// pass 2 - point light, camera outside
		Pass
		{
			ZTest Always Cull Back ZWrite Off Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragPointOutside
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma shader_feature USE_SHADOWS
			#pragma shader_feature USE_COOKIE

			
			fixed4 fragPointOutside(v2f i) : SV_Target
			{
				float2 uv = i.uv.xy / i.uv.w;

				float3 rayDir = (i.wpos - _WorldSpaceCameraPos);
				float rayLength = length(rayDir);

				rayDir /= rayLength;

				float3 lightToCamera = _WorldSpaceCameraPos - _LightPos;

				float b = dot(rayDir, lightToCamera);
				float c = dot(lightToCamera, lightToCamera) - (LIGHT_RANGE * LIGHT_RANGE);

				float d = sqrt((b*b) - c);
				float start = -b - d;
				float end = -b + d;

				end = min(end, GetProjectedDepth (uv, rayDir));

				float3 rayStart = _WorldSpaceCameraPos + rayDir * start;
				rayLength = end - start;

				return RayMarch(i.pos.xy, rayStart, rayDir, rayLength);
			}
			ENDCG
		}
					
	}
}