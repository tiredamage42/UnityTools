Shader "Hidden/Glitch"
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

            sampler2D _MainTex;
            
            fixed4 _DisplacementAmount;
            float4 _Params0, _Params1;
            #define CHROMATIC_ABERRATION _Params0.xy
            
            #define STRIPES_AMT_R _Params0.z
            #define STRIPES_AMT_L _Params0.w

            #define STRIPES_FILL_R _Params1.x
            #define STRIPES_FILL_L _Params1.y

            #define WAVE_FREQ _Params1.z
            
            #define GLITCH _Params1.w

            float rand(float2 co){
                return frac(sin( dot(co ,float2(12.9898,78.233))) * 43758.5453 );
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 displAmount = 0;
                fixed2 chromAberrAmount = 0;

                float rightStripesFill = 0;
                float leftStripesFill = 0;

                float fGlitch = frac(GLITCH);
                
                //Glitch control
                if (fGlitch < 0.8) {
                    rightStripesFill = lerp(0, STRIPES_FILL_R, fGlitch * 2);
                    leftStripesFill = lerp(0, STRIPES_FILL_L, fGlitch * 2);
                }
                if (fGlitch < 0.5) 
                    chromAberrAmount = lerp(fixed2(0, 0), CHROMATIC_ABERRATION, fGlitch * 2);
                if (fGlitch < 0.33) 
                    displAmount = lerp(fixed4(0,0,0,0), _DisplacementAmount, fGlitch * 3);

                //Stripes section
                float stripesRight = floor(i.uv.y * STRIPES_AMT_R);
                stripesRight = step(rightStripesFill, rand(float2(stripesRight, stripesRight)));

                float stripesLeft = floor(i.uv.y * STRIPES_AMT_L);
                stripesLeft = step(leftStripesFill, rand(float2(stripesLeft, stripesLeft)));
                
                //Displacement section
                fixed2 wavyDispl = lerp(fixed2(1,0), fixed2(0,1), (sin(i.uv.y * WAVE_FREQ) + 1) / 2);
                fixed2 displUV = (displAmount.xy * stripesRight) - (displAmount.xy * stripesLeft);
                displUV += (displAmount.zw * wavyDispl.r) - (displAmount.zw * wavyDispl.g);
                
                //Chromatic aberration section
                float chromR = tex2D(_MainTex, i.uv + displUV + chromAberrAmount).r;
                float chromG = tex2D(_MainTex, i.uv + displUV).g;
                float chromB = tex2D(_MainTex, i.uv + displUV - chromAberrAmount).b;
                    
                return fixed4(chromR, chromG, chromB, 1);
            }
            ENDHLSL
        }
    }
}
