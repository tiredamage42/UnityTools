Shader "Hidden/Spherical"
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
            uniform float _Radius;

            fixed4 frag (v2f_img i) : COLOR {
                float2 p = i.uv * 2.0 - 1.0;
                float rad = dot(p, p) * _Radius;
                //USE ONE OF THE BELOW FOR DIFFERENT EFFECTS
                float f = sqrt(1.0 - rad * rad);
                //float f = (1.0 - sqrt(1.0 - rad)) / rad;
                float2 nuv;
                nuv.x = (p.x * (f / 2.0)) + 0.5;
                nuv.y = (p.y * f) + 0.5;
                fixed4 main = tex2D(_MainTex, nuv);
                return main;
            }
            ENDHLSL
        }
    }
}
