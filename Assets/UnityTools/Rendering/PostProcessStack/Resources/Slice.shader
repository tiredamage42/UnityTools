Shader "Hidden/Slice"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment Fragment
            #pragma fragmentoption ARB_precision_hint_fastest
            
            #include "ImgFX.hlsl"
            sampler2D _MainTex;

            float4 _Parameters;

            #define DIR_X _Parameters.x
            #define DIR_Y _Parameters.y
            #define DIR _Parameters.xy
            
            #define DISPLACE _Parameters.z
            #define ROWS _Parameters.w

            uint _Seed;

            float4 Fragment(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float param = dot(uv - 0.5, float2(-DIR_Y, DIR_X) * float2((float)_ScreenParams.x / _ScreenParams.y, 1));
                float delta = Hash(_Seed + (uint)((param + 10) * ROWS + 0.5)) - 0.5;
                uv += DIR * delta * DISPLACE * float2((float)_ScreenParams.y / _ScreenParams.x, 1);
                return tex2D(_MainTex, uv);
            }
            ENDHLSL
        }
    }
}