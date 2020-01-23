Shader "Hidden/Outlined" {
    SubShader {			            
        Pass {
            Lighting Off
            ZWrite Off
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
            struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 dPos : TEXCOORD1;
			};

            static const float FADE_DIST = 3;
            fixed _MaxDistance;
            fixed4 _OutColor;
            
            v2f vert (float4 vertex : POSITION)            
            { 
                v2f o;
                o.vertex = UnityObjectToClipPos ( vertex );
                o.dPos = UnityObjectToViewPos ( vertex ).xyz;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { 
                float startFade = _MaxDistance - FADE_DIST;
                float fade = 1.0 - saturate((length(i.dPos) - startFade) / FADE_DIST);
                return _OutColor * fade;
            }
            ENDCG

        }
    }
}
