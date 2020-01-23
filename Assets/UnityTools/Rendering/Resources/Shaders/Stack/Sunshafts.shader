Shader "Hidden/SunShafts" {
	Subshader {
		Cull Front ZWrite Off ZTest Always
		HLSLINCLUDE
		#include "ImgFX.hlsl"
		sampler2D _MainTex, _ColorBuffer;
		sampler2D_float _CameraDepthTexture;

		half _BlurRadius4;
		half4 _SunColor, _SunPosition;
		
		struct v2f_radial {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float2 blurVector : TEXCOORD1;
		};

		half4 fragScreen(v2f i) : SV_Target { 
			fixed4 rays =  saturate(tex2D (_ColorBuffer, i.uv).r * _SunColor);
			// return rays;
			return 1.0 - (1.0 - tex2D (_MainTex, i.uv)) * (1.0 - rays);	
		}

		v2f_radial vert_radial( appdata_img v ) {
			v2f_radial o;
			StartVertex(v.vertex, o.pos, o.uv);
			o.blurVector = (_SunPosition.xy - o.uv) * _BlurRadius4;
			return o; 
		}

		half frag_radial(v2f_radial i) : SV_Target {	
			half color = 0;

			color += tex2D(_MainTex, i.uv.xy).r;
			i.uv.xy += i.blurVector; 	

			color += tex2D(_MainTex, i.uv.xy).r;
			i.uv.xy += i.blurVector; 	

			color += tex2D(_MainTex, i.uv.xy).r;
			i.uv.xy += i.blurVector; 	

			color += tex2D(_MainTex, i.uv.xy).r;
			i.uv.xy += i.blurVector; 	

			color += tex2D(_MainTex, i.uv.xy).r;
			i.uv.xy += i.blurVector; 	

			color += tex2D(_MainTex, i.uv.xy).r;
			i.uv.xy += i.blurVector; 	

			return color / 6.0;
		}	

		half frag_depth (v2f i) : SV_Target {
			float depthSample = Linear01Depth (SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy));
			if (depthSample <= .99)
				return 0;

			//max radius
			fixed sunAmount = saturate (_SunPosition.w - length (_SunPosition.xy - i.uv.xy));
			return dot(tex2D (_MainTex, i.uv).rgb, half3(1,1,1)) * sunAmount;
			// return sunAmount * 2;
		}
		ENDHLSL

		Pass {
			HLSLPROGRAM
			#pragma vertex ImgFXVertex
			#pragma fragment fragScreen
			#pragma fragmentoption ARB_precision_hint_fastest        
			ENDHLSL
		}
		Pass {
			HLSLPROGRAM
			#pragma vertex vert_radial
			#pragma fragment frag_radial
			#pragma fragmentoption ARB_precision_hint_fastest        
			ENDHLSL
		}
		Pass {
			HLSLPROGRAM
			#pragma vertex ImgFXVertex
			#pragma fragment frag_depth   
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDHLSL
		}
	}
}