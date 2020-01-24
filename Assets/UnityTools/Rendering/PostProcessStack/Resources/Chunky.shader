
Shader "Hidden/Chunky"
{
    SubShader
    {
        Cull Back ZWrite Off ZTest Off Blend Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "ImgFX.hlsl"
            sampler2D _MainTex;
            sampler2D _SprTex; 
            float4 _BlockCountSize; 
            
			fixed4 frag(v2f i) : SV_Target
			{
                float2 blockPos = floor(i.uv * _BlockCountSize.xy);
				float2 blockCenter = blockPos * _BlockCountSize.zw + _BlockCountSize.zw * 0.5;

				float4 tex = tex2D(_MainTex, blockCenter);// - del;
				float grayscale = dot(tex.rgb, float3(0.3, 0.59, 0.11));
				grayscale = clamp(grayscale, 0.0, 1.0);

                float dx = floor(grayscale * 16.0);

				float2 sprPos = i.uv;
				sprPos -= blockPos * _BlockCountSize.zw;
				sprPos.x /= 16;
				sprPos *= _BlockCountSize.xy;
				sprPos.x += 1.0 / 16.0 * dx;

				return tex2D(_SprTex, sprPos);
			}
            ENDHLSL
        }
    }
}
