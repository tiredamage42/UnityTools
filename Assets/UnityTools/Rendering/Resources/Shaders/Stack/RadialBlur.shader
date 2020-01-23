
Shader "Hidden/RadialBlur" {
    SubShader {
        ZTest Always Cull Front ZWrite Off Blend Off
		Pass { 
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
            #include "ImgFX.hlsl"
            sampler2D _MainTex;
            fixed4 _RadialBlurParameters; //center, iterations, delta offset

            #define CENTER _RadialBlurParameters.xy
            #define ITERATIONS _RadialBlurParameters.z
            #define DELTA_OFFSET _RadialBlurParameters.w            
            
            half4 frag (v2f i) : COLOR {          
                float2 deltaTextCoord = (i.uv - CENTER) * DELTA_OFFSET;
                float2 coord = i.uv;
                float4 c = float4(0, 0, 0, 0);
                int iSamples = ITERATIONS;
                for (int i = 0; i < iSamples ; i++) {
                    coord -= deltaTextCoord;
                    c += tex2D(_MainTex, coord);
                }
                return saturate(c / ITERATIONS);
            }
            ENDHLSL
		}
        Pass { 
            HLSLPROGRAM
            #pragma vertex vert_radial
            #pragma fragment frag_radial
			#pragma fragmentoption ARB_precision_hint_fastest
            #include "ImgFX.hlsl"
            sampler2D _MainTex;
            fixed4 _RadialBlurParameters; //center, iterations, delta offset
            fixed _BlurRadius4;
            #define CENTER _RadialBlurParameters.xy
            
            struct v2f_radial {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 blurVector : TEXCOORD1;
            };
            
            v2f_radial vert_radial( appdata_img v ) {
                v2f_radial o;
                StartVertex(v.vertex, o.pos, o.uv);
                o.blurVector = (CENTER - o.uv) * _BlurRadius4;
                return o; 
            }
            half4 frag_radial(v2f_radial i) : SV_Target 
            {	
                half4 color = half4(0,0,0,0);

                color += tex2D(_MainTex, i.uv.xy);
                i.uv.xy += i.blurVector; 	

                color += tex2D(_MainTex, i.uv.xy);
                i.uv.xy += i.blurVector; 	

                color += tex2D(_MainTex, i.uv.xy);
                i.uv.xy += i.blurVector; 	

                color += tex2D(_MainTex, i.uv.xy);
                i.uv.xy += i.blurVector; 	

                color += tex2D(_MainTex, i.uv.xy);
                i.uv.xy += i.blurVector; 	

                color += tex2D(_MainTex, i.uv.xy);
                i.uv.xy += i.blurVector; 	

                return color / 6.0;
            }
            
            ENDHLSL
		}
	}
}

