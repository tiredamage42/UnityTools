Shader "Hidden/Pixelated"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Off Blend Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "ImgFX.hlsl"
            sampler2D _MainTex;
            float4 _BlockCountSize;			
            fixed4 frag (v2f i) : COLOR {
                return tex2D(_MainTex, floor(i.uv * _BlockCountSize.xy) * _BlockCountSize.zw + _BlockCountSize.zw * 0.5);
            }
            ENDHLSL
        }
    }
}
