Shader "Hidden/SinCity"
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
            uniform fixed4 _SelectedColor, _ReplacedColor;

            float2 _Params;
            #define DESATURATION _Params.x
            #define TOLERANCE _Params.y

            fixed4 frag (v2f_img i) : COLOR {
                fixed4 main = tex2D(_MainTex, i.uv);
                fixed3 lum = Luminance(main.rgb) * DESATURATION;
                fixed3 col = _SelectedColor.rgb;
                if (main.r < col.r + TOLERANCE && main.r > col.r - TOLERANCE &&
                    main.g < col.g + TOLERANCE && main.g > col.g - TOLERANCE &&
                    main.b < col.b + TOLERANCE && main.b > col.b - TOLERANCE
                )
                    lum.rgb = _ReplacedColor.rgb;

                return fixed4(lum.rgb, main.a);
            }
            ENDHLSL
        }
    }
}



