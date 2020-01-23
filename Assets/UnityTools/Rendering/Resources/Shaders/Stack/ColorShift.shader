
Shader "Hidden/ColorShift" {
    SubShader {
        ZTest Off Cull Off ZWrite Off Blend Off

        HLSLINCLUDE
        #pragma vertex ImgFXVertex
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
        #include "ImgFX.hlsl"
        sampler2D _MainTex;
        fixed4 _MainTex_TexelSize;
        fixed _Amount;

        fixed4 Frag (v2f i, float2 offset) {
            offset *= _MainTex_TexelSize.xy * _Amount;
            float colR = tex2D(_MainTex, i.uv + offset).r;
            float colG = tex2D(_MainTex, i.uv).g;
            float colB = tex2D(_MainTex, i.uv - offset).b;
            return fixed4(colR, colG, colB, 1);
        }
        ENDHLSL
        Pass { 
            HLSLPROGRAM
            half4 frag (v2f i) : COLOR { return Frag (i, 1); }
            ENDHLSL
		}
		Pass { 
            HLSLPROGRAM
            half4 frag (v2f i) : COLOR { return Frag (i, normalize(i.uv - .5)); }
            ENDHLSL
		}
	}
}