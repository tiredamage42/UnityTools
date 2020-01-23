Shader "Hidden/TransferCubeMap"
{
    CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
            
        struct appdata {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };
        struct v2f {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };
        v2f vert (appdata v) {
            v2f o;
            o.vertex = float4(v.vertex.xy, 0.0, 0.5);
            o.uv = v.uv;
            return o;
        }

        samplerCUBE_float _CubeF;
        float GetValueF (float3 cubePos) {
            return texCUBE(_CubeF, cubePos * 2.0 - 1.0).r;
        }

        samplerCUBE _Cube;
        float4 GetValue (float3 cubePos) {
            return texCUBE(_Cube, cubePos * 2.0 - 1.0);
        }


        #define POSX float3(1.0, i.uv.y, 1.0 - i.uv.x)
        #define NEGX float3(0.0, i.uv.y, i.uv.x)

        #define POSY float3(i.uv.x, 1.0, 1.0 - i.uv.y)
        #define NEGY float3(i.uv.x, 0.0, i.uv.y)

        #define POSZ float3(i.uv.x, i.uv.y, 1.0)
        #define NEGZ float3(1.0 - i.uv.x, i.uv.y, 0.0)

        #define OFF 1

        ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        
        Pass {
            CGPROGRAM
            float frag (v2f i) : SV_Target { return GetValueF(POSX); }
            // float frag (v2f i) : SV_Target { return OFF; }
            
            ENDCG
        }
        Pass {
            CGPROGRAM
            float frag (v2f i) : SV_Target { return GetValueF(NEGX); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float frag (v2f i) : SV_Target { return GetValueF(POSY); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float frag (v2f i) : SV_Target { return GetValueF(NEGY); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float frag (v2f i) : SV_Target { return GetValueF(POSZ); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float frag (v2f i) : SV_Target { return GetValueF(NEGZ); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }




        Pass {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target { return GetValue(POSX); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target { return GetValue(NEGX); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target { return GetValue(POSY); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target { return GetValue(NEGY); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target { return GetValue(POSZ); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
        Pass {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target { return GetValue(NEGZ); }
            // float frag (v2f i) : SV_Target { return OFF; }
            ENDCG
        }
    }
}