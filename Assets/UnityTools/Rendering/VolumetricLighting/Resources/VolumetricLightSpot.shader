
Shader "Hidden/VolumetricLightSpot"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE
		#define SPOT
		#include "VolumetricLightsSpotPoint.cginc"
		ENDCG
		
		// pass 1 - spot light, camera inside
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

		// pass 3 - spot light, camera outside
		Pass
		{
			ZTest Always Cull Back ZWrite Off Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragSpotOutside
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma shader_feature USE_SHADOWS
			#pragma shader_feature USE_COOKIE

			#define COS_ANGLE  _AttenuationParams.z
			#define PLANE_D  _AttenuationParams.w

			float4 _ConeAxis;
			
			float2 RayConeIntersect(float3 f3ConeApex, float3 f3ConeAxis, float fCosAngle, float3 f3RayStart, float3 f3RayDir)
			{
				float inf = 10000;
				f3RayStart -= f3ConeApex;
				float a = dot(f3RayDir, f3ConeAxis);
				float b = dot(f3RayDir, f3RayDir);
				float c = dot(f3RayStart, f3ConeAxis);
				float d = dot(f3RayStart, f3RayDir);
				float e = dot(f3RayStart, f3RayStart);

				fCosAngle *= fCosAngle;
				float A = a*a - b*fCosAngle;
				float B = 2 * (c*a - d*fCosAngle);
				float C = c*c - e*fCosAngle;
				float D = B*B - 4 * A*C;

				if (D > 0)
				{
					D = sqrt(D);
					float2 t = (-B + sign(A)*float2(-D, +D)) / (2 * A);
					bool2 b2IsCorrect = c + a * t > 0 && t > 0;
					t = t * b2IsCorrect + !b2IsCorrect * (inf);
					return t;
				}
				else // no intersection
					return inf;
			}
			float RayPlaneIntersect(float3 planeNormal, float planeD, float3 rayOrigin, float3 rayDir)
			{
				float NdotD = dot(planeNormal, rayDir);
				float NdotO = dot(planeNormal, rayOrigin);
				float t = -(NdotO + planeD) / NdotD;
				if (t < 0)
					t = 100000;
				return t;
			}

			fixed4 fragSpotOutside(v2f i) : SV_Target
			{
				float2 uv = i.uv.xy / i.uv.w;

				float3 rayDir = (i.wpos - _WorldSpaceCameraPos);
				float rayLength = length(rayDir);

				rayDir /= rayLength;

				// inside cone
				float3 r1 = i.wpos + rayDir * 0.001;

				// plane intersection
				float planeCoord = RayPlaneIntersect(_ConeAxis, PLANE_D, r1, rayDir);

				// ray cone intersection
				float2 lineCoords = RayConeIntersect(_LightPos, _ConeAxis, COS_ANGLE, r1, rayDir);

				float z = (GetProjectedDepth (uv, rayDir) - rayLength);
				rayLength = min(min(planeCoord, min(lineCoords.x, lineCoords.y)), z);
				
				return RayMarch(i.pos.xy, i.wpos, rayDir, rayLength);
			}
			ENDCG
		}		
	}
}