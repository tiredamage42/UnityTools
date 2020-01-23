Shader "Hidden/VHS"
{
    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment Fragment
            #pragma fragmentoption ARB_precision_hint_fastest
            
            #include "ImgFX.hlsl"
            sampler2D _MainTex;
            
            float4 _Parameters0; // seed, shake, drift
            #define SEED _Parameters0.x
            #define SHAKE _Parameters0.y
            #define DRIFT_X _Parameters0.z
            #define DRIFT_Y _Parameters0.w
            
            float4 _Parameters1; // jitter, jump
            #define JITTER_X _Parameters1.x
            #define JITTER_Y _Parameters1.y
            #define JUMP_X _Parameters1.z
            #define JUMP_Y _Parameters1.w
            
            float4 Fragment(v2f input) : SV_Target
            {
                float2 uv = input.uv;

                // Texture space position
                float tx = uv.x;
                float ty = uv.y;

                // Jump
                ty = lerp(ty, frac(ty + JUMP_X), JUMP_Y);

                // Jitter
                float jitter = Hash(ty * _ScreenParams.y + SEED) * 2 - 1;
                tx += jitter * (JITTER_X < abs(jitter)) * JITTER_Y;

                // Shake
                tx = frac(tx + (Hash(SEED) - 0.5) * SHAKE);

                // Drift
                float drift = sin(ty * 2 + DRIFT_X) * DRIFT_Y;

                // Source sample
                float4 c1 = tex2D(_MainTex, float2(tx        , ty));
                float4 c2 = tex2D(_MainTex, float2(tx + drift, ty));
                
                return float4(c1.r, c2.g, c1.b, 1);
            }
            ENDHLSL
        }   
    }
}
