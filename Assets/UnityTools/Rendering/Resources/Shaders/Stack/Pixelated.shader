Shader "Hidden/Pixelated"
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
            uniform float4 _MainTex_TexelSize;
            float2 _PixSize;
            fixed4 frag (v2f_img i) : COLOR {
                float2 d = _PixSize * _MainTex_TexelSize.xy;
                return tex2D(_MainTex, float2(d.x * floor(i.uv.x / d.x), d.y * floor(i.uv.y / d.y)));
            }
            ENDHLSL
        }
    }
}
