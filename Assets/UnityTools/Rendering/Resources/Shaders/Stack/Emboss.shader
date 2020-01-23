Shader "Hidden/Emboss"
{
    HLSLINCLUDE

        #include "ImgFX.hlsl"

        uniform sampler2D _MainTex;
    uniform float4 _MainTex_TexelSize;


    fixed4 frag (v2f_img i) : COLOR {
        fixed4 main = tex2D(_MainTex, i.uv);
        main.rgb -= tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy).rgb * 2.0;
        main.rgb += tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy).rgb * 2.0;
        main.rgb = (main.r + main.g + main.b) / 3.0;
        return main;
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
                #pragma vertex ImgFXVertex
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
            ENDHLSL
        }
    }
}
