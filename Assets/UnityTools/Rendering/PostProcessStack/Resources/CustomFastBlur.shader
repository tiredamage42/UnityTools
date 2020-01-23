
Shader "Hidden/CustomFastBlur" {
	HLSLINCLUDE
		#include "Blur.hlsl"

		sampler2D _MainTex;
		fixed4 _MainTex_ST;
		
		fixed4 fragBlur8 ( v2f_blur i ) : SV_Target {
			
			fixed2 coords = i.uv - i.offs * 3.0;  
            fixed4 color = 0;
            [unroll] for( int l = 0; l < blurWeightsCount; l++ ) {   
				color += tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(coords, _MainTex_ST)) * blurWeights[l];
				coords += i.offs;
  			}
            return color;
		}
					
	ENDHLSL
	
	SubShader {
	    ZTest Off Cull Off ZWrite Off Blend Off
        
        Pass {
            ZTest Always //Cull Off
            HLSLPROGRAM 
            #pragma vertex vertBlurVertical
            #pragma fragment fragBlur8
			#pragma fragmentoption ARB_precision_hint_fastest
            ENDHLSL 
		}	
        Pass {		
            ZTest Always //Cull Off
            HLSLPROGRAM
            #pragma vertex vertBlurHorizontal
            #pragma fragment fragBlur8
			#pragma fragmentoption ARB_precision_hint_fastest
            ENDHLSL
		}
	}	
	FallBack Off
}

