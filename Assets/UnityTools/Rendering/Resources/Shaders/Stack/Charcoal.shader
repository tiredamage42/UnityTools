
Shader "Hidden/Charcoal"
{
    HLSLINCLUDE

        #include "ImgFX.hlsl"
        
        uniform sampler2D _MainTex;
        uniform float4 _MainTex_TexelSize;
        fixed4 _CharcoalParams; // rgb color, a = neg/pos

        fixed4 frag (v2f i) : COLOR {
            half xr = _MainTex_TexelSize.x * 2;
            half yr = _MainTex_TexelSize.y * 2;
            fixed4 main = tex2D(_MainTex, i.uv);
            fixed3 c0 = tex2D(_MainTex, i.uv + float2(xr, 0)).rgb;
            fixed3 c1 = tex2D(_MainTex, i.uv + float2(0, yr)).rgb;
            half f = 0;
            f += abs(main.r - c0.r);
            f += abs(main.g - c0.g);
            f += abs(main.b - c0.b);
            f += abs(main.r - c1.r);
            f += abs(main.g - c1.g);
            f += abs(main.b - c1.b);
            f = saturate(f);

            fixed4 nColor = 1 - f;
            nColor = (1 - nColor) * _CharcoalParams;
            nColor.a = main.a;
            
            fixed4 c = (1 - f) + _CharcoalParams * f;
            c.a = main.a;

            return lerp(nColor, c, _CharcoalParams.a);
            // main.rgb = (1 - f) + _CharcoalParams.rgb * f;
            // return 1 - main;
        }
        
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
                #pragma vertex ImgFXVertex
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
            ENDHLSL
        }
    }
}
