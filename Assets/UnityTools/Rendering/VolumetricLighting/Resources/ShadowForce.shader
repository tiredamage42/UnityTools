
Shader "Custom/ShadowForce" {
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGINCLUDE
        #pragma vertex vert_surf
        #pragma fragment frag_surf    
        #include "UnityCG.cginc"
        struct v2f_surf {
            UNITY_POSITION(pos);
        };
        v2f_surf vert_surf (appdata_full v) {
            v2f_surf o;
            o.pos = UnityObjectToClipPos(v.vertex);
            return o;
        }
        ENDCG
        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            ZTest Off ZWrite Off Blend One One
            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase noshadowmask nodynlightmap nolightmap
            #pragma fragmentoption ARB_precision_hint_fastest
            fixed4 frag_surf (v2f_surf IN) : SV_Target {
                return 0;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardAdd" }
            ZTest Off ZWrite Off Blend One One
            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma skip_variants INSTANCING_ON
            #pragma multi_compile_fwdadd_fullshadows noshadowmask nodynlightmap nolightmap
            #pragma fragmentoption ARB_precision_hint_fastest
            #if !defined(INSTANCING_ON)
            fixed4 frag_surf (v2f_surf IN) : SV_Target {
                return 0;
            }
            #endif
            ENDCG
        }
        Pass {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }
            ZTest Off ZWrite Off Blend One One
            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma multi_compile_prepassfinal noshadowmask nodynlightmap nolightmap
            #pragma fragmentoption ARB_precision_hint_fastest
            void frag_surf (v2f_surf IN, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3) {
                outGBuffer0 = outGBuffer1 = outGBuffer2 = outEmission = 0;
            }
            ENDCG
        }
    }
    // FallBack "Diffuse"
}
