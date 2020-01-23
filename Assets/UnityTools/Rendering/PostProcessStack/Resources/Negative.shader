Shader "Hidden/Negative"
{
    SubShader
    {
        Cull Front ZWrite Off ZTest Off Blend Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "ImgFX.hlsl"
            sampler2D _MainTex;
            fixed4 frag (v2f i) : SV_Target {
                fixed4 main = tex2D(_MainTex, i.uv);
                return fixed4((fixed3(1,1,1) - main.rgb), main.a);
            }
            ENDHLSL
        }
    }
}