Shader "Hidden/Sharpen"
{
    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment Fragment
            #include "ImgFX.hlsl"
            sampler2D _MainTex;
            float2 _MainTex_TexelSize;    
            float _Intensity;

            float4 SampleInput(float2 uv, float2 coord)
            {
                return tex2D(_MainTex, saturate(uv + _MainTex_TexelSize.xy * coord));
            }

            float4 Fragment(v2f i) : SV_Target
            {
                float4 c0 = SampleInput(i.uv, float2(-1, -1));
                float4 c1 = SampleInput(i.uv, float2( 0, -1));
                float4 c2 = SampleInput(i.uv, float2( 1, -1));

                float4 c3 = SampleInput(i.uv, float2(-1, 0));
                float4 c4 = SampleInput(i.uv, float2( 0, 0));
                float4 c5 = SampleInput(i.uv, float2( 1, 0));

                float4 c6 = SampleInput(i.uv, float2(-1, 1));
                float4 c7 = SampleInput(i.uv, float2( 0, 1));
                float4 c8 = SampleInput(i.uv, float2( 1, 1));

                return c4 - (c0 + c1 + c2 + c3 - 8 * c4 + c5 + c6 + c7 + c8) * _Intensity;
            }
            ENDHLSL
        }
    }
}
