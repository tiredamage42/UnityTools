Shader "Hidden/LensFlare"
{
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

        CGINCLUDE
        #pragma vertex ImgFXVertex
        #pragma fragment frag
        #include "ImgFX.hlsl"
        sampler2D _MainTex; 
        static const float2 CENTER = float2(.5, .5); 
        ENDCG

		Pass
		{
			CGPROGRAM

            sampler2D _LensColor;
            float4 _MainTex_TexelSize;

            float3 _GhostParameters;
            #define NUM_GHOSTS _GhostParameters.x
            #define GHOST_DISPLACE _GhostParameters.y
            #define GHOST_THRESHOLD _GhostParameters.z
            
            float3 _HaloParameters;
            #define HALO_RADIUS _HaloParameters.x
            #define HALO_THICKNESS _HaloParameters.y
            #define HALO_THRESHOLD _HaloParameters.z

            float3 SampleMain (float2 uv) {
                return tex2D(_MainTex, uv).rgb;
            }
            // Cubic window; map [0, _radius] in [1, 0] as a cubic falloff from _center.
            float Window_Cubic(float _x, float _center, float _radius) {
                _x = min(abs(_x - _center) / _radius, 1.0);
                return 1.0 - _x * _x * (3.0 - 2.0 * _x);
            }

            float3 ApplyThreshold(float3 _rgb, float _threshold) {
                return max(_rgb - _threshold, 0);
            }


            // float _Falloff;
            // fixed4 frag(v2f i) : SV_Target
            // {
            //     float lCenter = length(center);

            //     float2 texcoord = -i.uv + float2(1, 1);
            //     // ghost vector to image centre:
            //     float2 ghostVec = (center - texcoord) * _GhostDisplace;

            //     // sample ghosts:  
            //     fixed4 result = 0;
            //     for (int i = 0; i < _NumGhost; ++i) { 
            //         float2 offset = (texcoord + ghostVec * float(i));
            //         float weight = length(center - offset) / lCenter;
            //         weight = pow(1.0 - weight, _Falloff);
            //         result += tex2D(_MainTex, offset) * weight;
            //     }

            //     // result = saturate(result);
            //     result *= tex2D(_LensColor, float2((length(center - texcoord) / lCenter) * 2 + .1, 0));
            //     return result;
            // }

            float3 SampleGhosts(float2 _uv)
            {
                float3 ret = 0;
                float2 ghostVec = (CENTER - _uv) * GHOST_DISPLACE;
                for (int i = 0; i < NUM_GHOSTS; ++i) {
                    // sample scene color
                    float2 suv = frac(_uv + ghostVec * i);
                    float3 s = ApplyThreshold(SampleMain(suv), GHOST_THRESHOLD);
                    float distanceToCenter = distance(suv, CENTER);

                    // fade towards center, and towards edges...
                    float weight = smoothstep(0, 1, distanceToCenter * 25); 
                    weight *= smoothstep(1, 0, distanceToCenter * 6); 
                    s *= weight;
                    ret = saturate(ret + s);
                }
                // add color
                ret *= tex2D(_LensColor, float2(frac(distance(_uv, CENTER)) + .5, 0)).rgb;
                return ret;
            }
            
            
            // fixed4 frag(v2f i) : SV_Target
            // {                
            //     float2 ghostVec = i.uv - .5;
            //     float2 haloVec = normalize(ghostVec) * -_HaloRadius;
            //     float weight = length(float2(0.5, 0.5) - frac(i.uv + haloVec)) / length(float2(0.5, 0.5));
            //     weight = pow(1.0 - weight, _HaloFalloff);
            //     fixed4 col = tex2D(_MainTex, i.uv + haloVec) * weight;
            //     col = saturate(col - _HaloSub);
            //     return col;
            // }

            float3 SampleHalo(float2 _uv)
            {
                float _aspectRatio = _ScreenParams.y / _ScreenParams.x;

                float2 haloVec = CENTER - _uv;
                
                haloVec.x /= _aspectRatio;
                haloVec = normalize(haloVec);
                haloVec.x *= _aspectRatio;
                float2 wuv = (_uv - float2(0.5, 0.0)) / float2(_aspectRatio, 1.0) + float2(0.5, 0.0);
                float haloWeight = distance(wuv, CENTER);
                
                haloVec *= HALO_RADIUS;
                haloWeight = Window_Cubic(haloWeight, HALO_RADIUS, HALO_THICKNESS);

                float2 uv = _uv + haloVec;
                return ApplyThreshold(SampleMain(uv) * saturate(length((uv - CENTER) * 100)), HALO_THRESHOLD) * haloWeight;
            }

        
			fixed4 frag (v2f i) : SV_Target
			{
                float2 uv = 1.0 - i.uv; // flip the texture coordinates
                return fixed4(SampleGhosts(uv) + SampleHalo(uv), 1);
			}
			ENDCG
		}
        Pass
        {
            CGPROGRAM
            sampler2D _LensArtifacts, _LensDirt, _LensStar; 
            float3 _CamFwd;

            static const float IRIS_FREQUENCY = 1;
            static const float IRIS_OFFSET = 0.5;
            static const float DIRT_POWER = 4;

            float IrisNoise (float2 uv) {
                float2 centerVec = uv - CENTER;
                float d = length(centerVec);
                float radial = acos(centerVec.x / d) * IRIS_FREQUENCY;

                float o = _CamFwd.x + _CamFwd.y + _CamFwd.z;
                // rotate in the opposite direction at a different rate
                float2 uv1 = float2(radial + o, 0.0);
                float2 uv2 = float2(radial - o * 0.35, 0.0);
                
                float star = (tex2D(_LensStar, uv1).r * tex2D(_LensStar, uv2).r) + IRIS_OFFSET;
                star = saturate(star + (1.0 - smoothstep(0.0, 0.3, d))); // fade the starburst towwards the center
                return star;                
            }

            fixed4 frag (v2f i) : SV_Target {
                float4 dirt = tex2D(_LensDirt, i.uv) * DIRT_POWER;
                // return dirt;
                float star = IrisNoise (i.uv);
                float4 artifacts = tex2D(_LensArtifacts, i.uv);
                float4 flares = artifacts * saturate(dirt * star); // maybe (dirt * star)
                // return flares;

                return tex2D(_MainTex, i.uv) + flares;
            }
            ENDCG
        }
	}
}