Shader "Hidden/Outlines"{
    SubShader {    
        Cull Front ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "ImgFX.hlsl"
        sampler2D _MainTex;
        ENDHLSL
        //masking out the inner highglight from the blurred image
        Pass {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            sampler2D _MaskOut;
            // how much alpha we subtract from the inner highlight
            fixed _MaskAlphaSubtractMult;

            fixed4 frag (v2f i) : COLOR  {

                fixed4 mask = tex2D(_MaskOut, i.uv);
                
                fixed4 mask4 = fixed4(mask.a, mask.a, mask.a, mask.a * _MaskAlphaSubtractMult);
                fixed4 blurred = tex2D(_MainTex, i.uv);
                return blurred - mask4;
            }
            ENDHLSL
        }

        //final, add highlights to scene texture (_MainTex)
        Pass {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            
            sampler2D _OverlayHighlights, _OverlayMask, _DepthHighlights;
            
            fixed3 _Intensity_Heaviness_OverlayAlphaHelper;

            fixed4 frag (v2f i) : COLOR  {
                
                fixed intensity = _Intensity_Heaviness_OverlayAlphaHelper.x;
                fixed heaviness = _Intensity_Heaviness_OverlayAlphaHelper.y;
                fixed overlayAlphaHelper = _Intensity_Heaviness_OverlayAlphaHelper.z;

                fixed4 overlayHighlights = tex2D(_OverlayHighlights, i.uv);

                fixed4 depthHighlights = tex2D(_DepthHighlights, i.uv);
                
                // remove any highlights from depth tested pass that overlap into
                // the overlay insides
                
                //maybe just subtract
                depthHighlights.rgba = depthHighlights.rgba * saturate(1.0 - overlayHighlights.a * overlayAlphaHelper);
                                
                //adjust the overlay alpha so it's back ot normal (0 on the inside of the highlight)
                overlayHighlights.a -= tex2D(_OverlayMask, i.uv).a;
                 
                fixed4 allOverlay = saturate(overlayHighlights + depthHighlights);

                fixed interpolator = allOverlay.a * heaviness;
                
                allOverlay = allOverlay * intensity;

                fixed4 scene = tex2D(_MainTex, i.uv);
                // return scene;
                return lerp(scene, allOverlay, interpolator);
            }
            ENDHLSL
        }




        // optimized passes:

        //masking out the inner highglight from the blurred image
        Pass {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            sampler2D _MaskOut;
            fixed4 frag (v2f i) : COLOR  {
                return tex2D(_MainTex, i.uv) - tex2D(_MaskOut, i.uv).a;
            }
            ENDHLSL
        }

        //final, add highlights to scene texture (_MainTex)
        Pass {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            
            sampler2D _OverlayHighlights;
            fixed2 _Intensity_Heaviness;
            
            fixed4 frag (v2f i) : COLOR  {
                fixed4 overlayHighlights = tex2D(_OverlayHighlights, i.uv);                                
                return lerp(tex2D(_MainTex, i.uv), overlayHighlights * _Intensity_Heaviness.x, overlayHighlights.a * _Intensity_Heaviness.y);
            }
            ENDHLSL
        }
    }       
}