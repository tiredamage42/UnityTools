Shader "Hidden/LightWave"
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
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _RG;
            float3 _BI;
            fixed4 frag (v2f i) : COLOR {
                fixed3 result = fixed3(
                    tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * _RG.xy).r,
                    tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * _RG.zw).r,
                    tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * _BI.xy).r
                );
                return fixed4(lerp(tex2D(_MainTex, i.uv).rgb, result, _BI.z), 1);
            }
            ENDHLSL
        }
    }
}