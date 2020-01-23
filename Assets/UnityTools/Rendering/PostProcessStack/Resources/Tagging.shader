Shader "Hidden/Tagging"{
    SubShader {    
        Cull Front ZWrite Off ZTest Always
        Pass {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "ImgFX.hlsl"
            sampler2D _MainTex, _OverlayMask;
                
            fixed4 frag (v2f i) : COLOR  {
                fixed4 allOverlay = tex2D(_OverlayMask, i.uv);
                return lerp(tex2D(_MainTex, i.uv), allOverlay, allOverlay.a);
            }
            ENDHLSL
        }
    }       
}