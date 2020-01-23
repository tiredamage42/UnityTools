Shader "Hidden/TransferWorld2Shadow"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        struct appdata {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };
        struct v2f {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
        };
        v2f vert (appdata v) {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }
        ENDCG
        Pass {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target {
                int2 u = floor(i.uv * 4);
                u.x = min(3, u.x);
                u.y = min(3, u.y);
                return float4(
                    unity_WorldToShadow[0][u.x][u.y],
                    unity_WorldToShadow[1][u.x][u.y],
                    unity_WorldToShadow[2][u.x][u.y],
                    unity_WorldToShadow[3][u.x][u.y]
                );
            }
            ENDCG
        }
        // Pass {
        //     CGPROGRAM
        //     float4 frag (v2f i) : SV_Target {
        //         int uvI = min(3, floor(i.uv.x * 4));
        //         return float4 (unity_ShadowSplitSpheres[uvI].xyz, unity_ShadowSplitSqRadii[uvI]);
        //     }
        //     ENDCG
        // }
    }
}