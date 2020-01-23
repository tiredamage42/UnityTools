Shader "Hidden/Streak"
{
    HLSLINCLUDE
    #include "ImgFX.hlsl"
    sampler2D _MainTex, _HighTex;
    float4 _MainTex_TexelSize;
    
    float3 _Params; 
    
    void SampleTap (float2 uv, out float3 c0, out float3 c1, out float3 c2, out float3 c3) {
        const float dx = _MainTex_TexelSize.x * 1.5;
        c0 = tex2D(_MainTex, float2(uv.x - dx, uv.y)) / 2;
        c1 = tex2D(_MainTex, uv) / 4;
        c2 = tex2D(_MainTex, float2(uv.x + dx, uv.y)) / 2;
        c3 = tex2D(_HighTex, uv);
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ImgFXVertex
            #pragma fragment frag_prefilter
            #pragma fragmentoption ARB_precision_hint_fastest
			

            #define SKYBOX_THREASHOLD_VALUE 0.9999
            #define THRESHOLD _Params.x

            sampler2D_float _CameraDepthTexture;
    
            // Prefilter: Shrink horizontally and apply threshold.
            half4 frag_prefilter(v2f i) : SV_Target
            {
                float depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
                if (depth >= SKYBOX_THREASHOLD_VALUE)
                    return 0;

                // Actually this should be 1, but we assume you need more blur...
                const float vscale = 1.5;
                const float dy = _MainTex_TexelSize.y * vscale / 2;

                half3 c0 = tex2D(_MainTex, float2(i.uv.x, i.uv.y - dy));
                half3 c1 = tex2D(_MainTex, float2(i.uv.x, i.uv.y + dy));
                half3 c = (c0 + c1) / 2;

                float br = max(c.r, max(c.g, c.b));
                c *= max(0, br - THRESHOLD) / max(br, 1e-5);

                return half4(c, 1);
            }
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag_down
            #pragma fragmentoption ARB_precision_hint_fastest
			
            half4 frag_down(v2f i) : SV_Target
            {
                // Actually this should be 1, but we assume you need more blur...
                const float hscale = 1.25;
                const float dx = _MainTex_TexelSize.x * hscale;

                half3 c0 = tex2D(_MainTex, float2(i.uv.x - dx * 5, i.uv.y));
                half3 c1 = tex2D(_MainTex, float2(i.uv.x - dx * 3, i.uv.y));
                half3 c2 = tex2D(_MainTex, float2(i.uv.x - dx * 1, i.uv.y));
                half3 c3 = tex2D(_MainTex, float2(i.uv.x + dx * 1, i.uv.y));
                half3 c4 = tex2D(_MainTex, float2(i.uv.x + dx * 3, i.uv.y));
                half3 c5 = tex2D(_MainTex, float2(i.uv.x + dx * 5, i.uv.y));

                // Simple box filter
                half3 c = (c0 + c1 + c2 + c3 + c4 + c5) / 6;

                // return half4((c0 + c1 * 2 + c2 * 3 + c3 * 3 + c4 * 2 + c5) / 12, 1);
                return half4(c, 1);
            }
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag_up
            #pragma fragmentoption ARB_precision_hint_fastest
			

            #define STRETCH _Params.y
    
            half4 frag_up(v2f i) : SV_Target
            {
                float3 c0, c1, c2, c3;
                SampleTap (i.uv, c0, c1, c2, c3);
                return half4(lerp(c3, c0 + c1 + c2, STRETCH), 1);
            }
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag_composite
            #pragma fragmentoption ARB_precision_hint_fastest
			
            #define INTENSITY _Params.z


            float3 _Color;
            half4 frag_composite(v2f i) : SV_Target
            {
                float3 c0, c1, c2, c3;
                SampleTap (i.uv, c0, c1, c2, c3);
        
                half3 cf = (c0 + c1 + c2) * _Color * INTENSITY * 5;
                // return half4(cf, 1);
                return half4(cf + c3, 1);
            }
            ENDHLSL
        }
    }
}