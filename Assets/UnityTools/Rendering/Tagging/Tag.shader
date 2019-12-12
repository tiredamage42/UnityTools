Shader "Hidden/Tagged"
{
	// Properties
	// {
	// 	_Color( "Color", Color ) = ( 1, 1, 1, 1 )
	// 	_FlashRimPower ("Rim Power", Range(.01, 3)) = 1
	// }
	
	SubShader
	{
		 
		Pass
		{       
			Lighting Off

            Blend One OneMinusSrcAlpha
            
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            
			#include "UnityCG.cginc" 
			
			#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
				#define UNITY_FOG_LERP_COLOR(col, fogCol, fogFac) col.rgb = lerp(fogCol, col.rgb, fogFac)				
				#define APPLY_FOG(coord, col) UNITY_FOG_LERP_COLOR(col, unity_FogColor.rgb, coord)
				#define CALCULATE_FOG(clipPos, outFogFactor) UNITY_CALC_FOG_FACTOR(clipPos.z); outFogFactor = saturate(unityFogFactor);
			#else
				#define APPLY_FOG(coord,col)
				#define CALCULATE_FOG(clipPos, outFogFactor)
			#endif

			#define INITIALIZE_FRAGMENT_IN(INTYPE, FRAG_IN_NAME, VERTEX_IN) \
				INTYPE FRAG_IN_NAME = (INTYPE)0; \
				UNITY_SETUP_INSTANCE_ID(VERTEX_IN); \
				UNITY_TRANSFER_INSTANCE_ID(VERTEX_IN, FRAG_IN_NAME); \
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(FRAG_IN_NAME); 

			fixed4 _Color;
			fixed2 _FlashRimPower;
            
			struct v2f {
				UNITY_POSITION(pos);
				float4 wPos : TEXCOORD0;
				float3 wNorm : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID 
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// vertex shader
			v2f vert (appdata_full v) {
				INITIALIZE_FRAGMENT_IN(v2f, o, v)      
				o.pos = UnityObjectToClipPos(v.vertex);
				o.wPos.xyz = mul(unity_ObjectToWorld, v.vertex);
                o.wNorm = UnityObjectToWorldNormal(v.normal); 
				CALCULATE_FOG(o.pos, o.wPos.w)				
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(i);
                fixed4 color = _Color * _FlashRimPower.x * pow(1.0 - saturate(dot(normalize(_WorldSpaceCameraPos.xyz - i.wPos.xyz), i.wNorm)), _FlashRimPower.y);
                APPLY_FOG(i.wPos.w, color); 
				return color;
			}
			ENDCG
		}
	}
}