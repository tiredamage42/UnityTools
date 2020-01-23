Shader "Hidden/Fog"
{
    SubShader {
        Cull Front ZWrite Off ZTest Always
        Pass {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "ImgFX.hlsl"
            float4 _FogParams; // fog start, fog end, color start, color end
            float4 _Params2; // fog steep, color steep, skybox affect, intensity

            float4 _HeightParams; // height, height fade, height start, height end
            float4 _HeightParams2;
            
            #define HEIGHT _HeightParams.x
            #define HEIGHT_FADE _HeightParams.y
            #define HEIGHT_START _HeightParams.z
            #define HEIGHT_END _HeightParams.w

            #define HEIGHT_NOISE_INTENSITY _HeightParams2.x
            
            fixed4 _Color0, _Color1;
            float4x4 _ViewProjInv;
            
            float4 _NoiseParams; // FG size, intensity BG size, offset
            float3 _FGNoiseSpeed;
            
            #define FOG_START _FogParams.x
            #define FOG_END _FogParams.y
            #define FOG_STEEPNESS _Params2.x

            #define COLOR_START _FogParams.z
            #define COLOR_END _FogParams.w
            #define COLOR_STEEPNESS _Params2.y

            #define SKYBOX_AFFECT _Params2.z
            #define FOG_INTENSITY _Params2.w

            #define FOREGROUND_NOISE_SIZE _NoiseParams.x
            #define FOREGROUND_NOISE_INTENSITY _NoiseParams.y
            
            #define SKYBOX_THREASHOLD_VALUE 0.9999

            sampler2D _MainTex, _CameraDepthTexture, _SkyNoise;
            sampler3D _NoiseTexture;

            half ComputeLerp(half z, half start, half end, half steepness) {
                return pow(saturate((z - start) / (end - start)), steepness);
            }
            float3 GetWorldPositionFromDepth( float depth, float2 uv ) {    
                float4 D = mul(_ViewProjInv, float4(uv * 2 - 1, depth, 1.0));
                return D.xyz / D.w;
            }
            float4 frag(v2f i) : SV_Target {
                half4 scene = tex2D(_MainTex, i.uv);
                float sampledDepth = tex2D(_CameraDepthTexture, i.uv).r;
                float depth = Linear01Depth(sampledDepth);
                
                float skyNoise = tex2D(_SkyNoise, i.uv).r;
                fixed4 bgColor = lerp(_Color0, _Color1, skyNoise);

                if (depth >= SKYBOX_THREASHOLD_VALUE)
                    return lerp(scene, bgColor, SKYBOX_AFFECT * FOG_INTENSITY);
                    
                float3 wPos = GetWorldPositionFromDepth(sampledDepth, i.uv);

                float height = wPos.y;
                
                wPos *= FOREGROUND_NOISE_SIZE;
                wPos += _FGNoiseSpeed * _Time.x;

                float fgNoiseA = (tex3D(_NoiseTexture, wPos).a * 2 - 1); 
                float fgNoise = fgNoiseA * FOREGROUND_NOISE_INTENSITY;
                
                float dist = LinearEyeDepth(sampledDepth) + fgNoise;
                
                half fog = ComputeLerp(dist, FOG_START, FOG_END, FOG_STEEPNESS);
            
                fixed heightT = 1.0 - saturate(((height + fgNoiseA * HEIGHT_NOISE_INTENSITY) - HEIGHT) / HEIGHT_FADE);
                // heightT = min(heightT, .9);

                half fogH = ComputeLerp(dist, HEIGHT_START, HEIGHT_END, 1);
                heightT = lerp(0, heightT, fogH);

                fog = max(fog, heightT);
                // fog = saturate(fog + heightT);
                
                half col = ComputeLerp(dist, COLOR_START, COLOR_END, COLOR_STEEPNESS);
                fixed4 fogColor = lerp(_Color0, bgColor, col);

                return lerp(scene, fogColor, fog * FOG_INTENSITY);
            }
            ENDHLSL
        }
    }
}