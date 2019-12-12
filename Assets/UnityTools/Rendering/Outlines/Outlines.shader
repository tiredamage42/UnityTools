Shader "Hidden/Outlines"{
    Properties {
        _MainTex("Main Texture", 2D)="black"{}
    }
    SubShader {
        
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
        sampler2D _MainTex;
        fixed4 _MainTex_ST;

        #pragma vertex vert_img
        #pragma fragment frag
        #include "UnityCG.cginc"
        struct appdata
			{
				fixed4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
			};

        struct v2f
			{
				fixed2 uv : TEXCOORD0;
				fixed4 vertex : SV_POSITION;
			};

			v2f vert_img (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				// o.uv = v.uv;
                o.uv = v.uv;// UnityStereoScreenSpaceUVAdjust(v.uv, _MainTex_ST);
				return o;
			}
        ENDCG

        //masking out the inner highglight from the blurred image
        Pass {
            CGPROGRAM
            sampler2D _MaskOut;

            // how much alpha we subtract from the inner highlight
            fixed _MaskAlphaSubtractMult;

            fixed4 frag (v2f i) : COLOR  {

                // invert for OPENGL
                #if UNITY_UV_STARTS_AT_TOP
                    // i.uv.y = 1-i.uv.y;
                #endif

                i.uv = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
                

                fixed4 mask = tex2D(_MaskOut, i.uv);
                fixed4 mask4 = fixed4(mask.a,mask.a,mask.a,mask.a*_MaskAlphaSubtractMult);
                fixed4 blurred = tex2D(_MainTex, i.uv);
                return blurred - mask4;
            }
            ENDCG
        }

        //final, add highlights to scene texture (_MainTex)
        Pass {
            CGPROGRAM
            sampler2D _OverlayHighlights, _OverlayMask, _DepthHighlights;

            
            fixed3 _Intensity_Heaviness_OverlayAlphaHelper;

            fixed4 frag (v2f i) : COLOR  {
                i.uv = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
                

                fixed intensity = _Intensity_Heaviness_OverlayAlphaHelper.x;
                fixed heaviness = _Intensity_Heaviness_OverlayAlphaHelper.y;
                fixed overlayAlphaHelper = _Intensity_Heaviness_OverlayAlphaHelper.z;


                fixed4 overlayHighlights = tex2D(_OverlayHighlights, i.uv);

                
                fixed4 depthHighlights = tex2D(_DepthHighlights, i.uv);
                
                
                // remove any highlights from depth tested pass that overlap into
                // the overlay insides
                
                //maybe just subtract
                // depthHighlights.rgb = depthHighlights.rgb * saturate(1.0 - overlayHighlights.a * overlayAlphaHelper);
                depthHighlights.rgba = depthHighlights.rgba * saturate(1.0 - overlayHighlights.a * overlayAlphaHelper);
                
                // return fixed4(depthHighlights.rgb, 1);

                
                //adjust the overlay alpha so it's back ot normal (0 on the inside of the highlight)
                overlayHighlights.a -= tex2D(_OverlayMask, i.uv).a;
                // return overlayHighlights.a;

                 
                fixed4 allOverlay = saturate(overlayHighlights + depthHighlights);
                // allOverlay.a = saturate(overlayHighlights + depthHighlights);

                fixed interpolator = allOverlay.a * heaviness;
                
                // return depthHighlights;

                
                allOverlay = allOverlay * intensity;
                // return allOverlay;

                
                fixed4 scene = tex2D(_MainTex, i.uv);

                return lerp(scene, allOverlay, interpolator);
            }
            ENDCG
        }
    }       
}