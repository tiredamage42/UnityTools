Shader "Hidden/BlitAdd"  {
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
			#pragma vertex ImgFXVertex
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "../../PostProcessStack/Resources/ImgFX.hlsl"
			sampler2D _MainTex, _Source;
			fixed4 frag(v2f i) : SV_Target {
				float4 main = tex2D(_MainTex, i.uv);
				float4 source = tex2D(_Source, i.uv);
				// return main;
				// return source;
				main.xyz *= source.w;
				main.xyz += source.xyz;
				return (main);
			}
			ENDCG
		}
	}
}