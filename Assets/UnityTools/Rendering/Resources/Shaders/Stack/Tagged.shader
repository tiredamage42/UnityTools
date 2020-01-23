Shader "Hidden/Tagged" {
    SubShader {		
        Blend One OneMinusSrcAlpha
        Lighting Off
        ZWrite Off
        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
        #include "UnityCG.cginc"
        
        struct appdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
        };
        
        struct v2f {
            float4 vertex : SV_POSITION;
            float3 wPos : TEXCOORD0;
            float3 wNorm : TEXCOORD1;
        };
        
        static const float FADE_DIST = 3;
        fixed _MaxDistance;
        fixed4 _Color;

        v2f vert (appdata v) { 
            v2f o;
            o.vertex = UnityObjectToClipPos ( v.vertex );
            o.wPos = mul( unity_ObjectToWorld, v.vertex );
            o.wNorm = UnityObjectToWorldNormal( v.normal ); 
            return o;
        }
        fixed4 FRAG(v2f i, float2 flashRimPower) { 
            float3 dir = _WorldSpaceCameraPos.xyz - i.wPos.xyz;
            float distance = length(dir);
            dir /= distance;
            fixed4 color = _Color * flashRimPower.x * pow(1.0 - saturate(dot(dir, i.wNorm)), flashRimPower.y);
            float startFade = _MaxDistance - FADE_DIST;
            float distFade = 1.0 - saturate((distance - startFade) / FADE_DIST);
            return color * distFade;
        }
        ENDCG	            

        Pass {
            CGPROGRAM			
            fixed _RimPower;
            fixed4 frag(v2f i) : SV_Target { 
                return FRAG(i, float2(1, _RimPower));
            }
            ENDCG
        }
        Pass {
            CGPROGRAM		
            float4 _FlashParams;
            #define FLASH_SPEED _FlashParams.x
            #define FLASH_STEEP _FlashParams.y
            #define FLASH_POWER_RANGE_X _FlashParams.z
            #define FLASH_POWER_RANGE_Y _FlashParams.w
            
            float2 CalcFlashPower () {
                float2 fp = 0;
                fp.x = pow(saturate(sin(_Time.y * FLASH_SPEED)), FLASH_STEEP);
                fp.y = lerp(FLASH_POWER_RANGE_X, FLASH_POWER_RANGE_Y, fp.x);
                return fp;
            }	

            fixed4 frag(v2f i) : SV_Target { 
                return FRAG(i, CalcFlashPower());
            }            
            ENDCG
        }
    }
}
