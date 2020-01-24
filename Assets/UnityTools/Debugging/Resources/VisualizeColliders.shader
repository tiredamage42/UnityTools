Shader "Hidden/VisualizeColliders" {
    SubShader {
        Tags { "RenderType"="Transparent" }
        ZWrite Off ZTest Off Blend One One
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            static const fixed4 COLOR = fixed4(0,1,0,.5);
            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
            };
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target {
                return COLOR;
            }
            ENDCG
        }
    }
}
