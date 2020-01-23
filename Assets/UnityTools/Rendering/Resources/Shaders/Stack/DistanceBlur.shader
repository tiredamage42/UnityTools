
Shader "Hidden/Custom/DistanceBlur" 
{
    HLSLINCLUDE

        #include "Blur.hlsl"

        sampler2D _MainTex;
		sampler2D _UnblurredMap, _CameraDepthTexture;
        fixed4 _MainTex_ST;
        half4 _DistBlurParams; // (blur start, fade range, fade steepness, max distance)
        float _SkyboxBleed; //.5f
		
        half BuildDepthBlurMap ( v2f i ) : SV_Target
		{			
            half d = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
            if (d >= _DistBlurParams.w) 
                return -1;

            return pow(smoothstep(0, 1, (d - _DistBlurParams.x) / _DistBlurParams.y), _DistBlurParams.z);
		}

        half BlurDepthBlurMap ( v2f_blur i ) : SV_Target
		{            
            fixed2 coords = i.uv - i.offs * 3.0;  
			fixed color = 0;
  			[unroll] for( int l = 0; l < blurWeightsCount; l++ )  
  			{   
				color += max(tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(coords, _MainTex_ST)).r, 0) * blurWeights[l];
				coords += i.offs;
  			}

            fixed unblurredMap = tex2D(_UnblurredMap, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST)).r;            
            
            // if we're blurring against the skybox, adjust color so higher values "bleed" into skybox
            // so there isnt a stark blur contrast between the blurred object and the unblurred skybox
            if (unblurredMap < 0) 
                return pow(color, _SkyboxBleed);
            
            // for lower depth values (unblurred), favor lower values, so blur doesnt bleed onto objects
            // that are in focus

            // for higher depth values (in blur area), favor higher values, so blur bleeds out
            // to prevent stark contrast between a blurred object with an unblurred background (e.g. skybox)
            return lerp(min(color, unblurredMap), max(color, unblurredMap), unblurredMap);
		}
            
					
	ENDHLSL

    
	SubShader {
        ZTest Off Cull Off ZWrite Off Blend Off
		// build depth blur map
        Pass { 
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment BuildDepthBlurMap
			#pragma fragmentoption ARB_precision_hint_fastest
            ENDHLSL
		}
        // blur depth blur map vertical
        Pass {
            HLSLPROGRAM 
            #pragma vertex vertBlurVertical
            #pragma fragment BlurDepthBlurMap
			#pragma fragmentoption ARB_precision_hint_fastest
            ENDHLSL 
		}	
        // blur depth blur map horizontal
        Pass {		
            HLSLPROGRAM
            #pragma vertex vertBlurHorizontal
            #pragma fragment BlurDepthBlurMap
			#pragma fragmentoption ARB_precision_hint_fastest
            ENDHLSL
		}	

        // add distance blur to source
		Pass { 
            HLSLPROGRAM
                #pragma vertex ImgFXVertex
                #pragma fragment AddDistanceBlur
                #pragma fragmentoption ARB_precision_hint_fastest
                
                #pragma shader_feature DEBUG_VISUAL
                
                sampler2D _DepthBlurMap, _BlurredSource;
                
				half4 AddDistanceBlur (v2f i) : SV_Target {
					half t = tex2D(_DepthBlurMap, i.uv).r;
					
#if defined(DEBUG_VISUAL)
                    if (t <= 0) return fixed4(0,0,1,1);
                    if (t >= 1) return fixed4(1,0,0,1);
                    return t;
#else

                    half4 source = tex2D(_MainTex, i.uv);
					half4 blurred = tex2D(_BlurredSource, i.uv);

					half3 color = lerp(source.rgb, blurred.rgb, saturate(t));
    				return half4( color, source.a );
#endif
				}
			ENDHLSL
		}
	}
}