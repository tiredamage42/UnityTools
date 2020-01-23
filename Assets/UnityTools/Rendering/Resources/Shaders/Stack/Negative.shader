Shader "Hidden/Negative"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "ImgFX.hlsl"
            uniform sampler2D _MainTex;
            fixed4 frag (v2f i) : COLOR {
                fixed4 main = tex2D(_MainTex, i.uv);
                return fixed4((fixed3(1,1,1) - main.rgb), main.a);
            }
            ENDHLSL
        }
    }
}