
Shader "Hidden/Binary"
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
            float2 _MainTex_TexelSize;

            sampler2D _DitherTex;
            float2 _DitherTex_TexelSize;
            
            half2 _ScaleOpacity;
            half3 _Color0, _Color1;

            half4 frag(v2f i) : SV_Target
            {
                half4 source = tex2D(_MainTex, i.uv);

                // Dither pattern sample
                float2 dither_uv = i.uv * _DitherTex_TexelSize;
                dither_uv /= _MainTex_TexelSize * _ScaleOpacity.x;
                half dither = tex2D(_DitherTex, dither_uv).a + 0.5 / 256;

                // Relative luminance in linear RGB space
            #ifdef UNITY_COLORSPACE_GAMMA
                half rlum = LinearRgbToLuminance(GammaToLinearSpace(saturate(source.rgb)));
            #else
                half rlum = LinearRgbToLuminance(source.rgb);
            #endif

                // Blending
                half3 rgb = rlum < dither ? _Color0 : _Color1;
                return half4(lerp(source.rgb, rgb, _ScaleOpacity.y), source.a);
            }

            ENDHLSL
        }
    }
}
