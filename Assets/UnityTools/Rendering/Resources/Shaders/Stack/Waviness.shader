
Shader "Hidden/Waviness" {
    SubShader {
        ZTest Off Cull Off ZWrite Off Blend Off
		Pass { 
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
            #include "ImgFX.hlsl"
            sampler2D _MainTex, _DisplTex;
            fixed4 _Parameters; // amount, speed, inverse size

            #define INVERSE_SIZE _Parameters.w
            #define AMOUNT _Parameters.x
            #define SPEED _Parameters.yz
            
            half4 frag (v2f i) : COLOR {
                return tex2D(_MainTex, i.uv + ((tex2D(_DisplTex, i.uv * INVERSE_SIZE + _Time.x * SPEED).xy * 2) - 1) * AMOUNT);
            }
            ENDHLSL
		}
	}
}