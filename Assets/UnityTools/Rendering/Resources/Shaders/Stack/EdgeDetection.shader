Shader "Hidden/EdgeDetection"
{
    HLSLINCLUDE

        #include "ImgFX.hlsl"

        sampler2D _MainTex;
        sampler2D _CameraDepthNormalsTexture;
        float4 _MainTex_TexelSize;
        float4 _Parameters;
        fixed4 _EdgeColor, _BackgroundColor;
        fixed _Scale, _DepthThreshold, _NormalThreshold, _DepthNormalThreshold, _DepthNormalThresholdScale;
        float4x4 _ClipToView;


struct v2f2 {
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 viewSpaceDir : TEXCOORD1;
};


v2f2 ImgFXVertex2 (float4 vertex : POSITION) {
    v2f2 o;
    StartVertex(vertex, o.pos, o.uv);
    o.viewSpaceDir = mul(_ClipToView, o.pos).xyz;
    return o;
}


        float4 GetPixelValue(in float2 uv) {
            half3 normal;
            float depth;
            DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, uv), depth, normal);
            return fixed4(normal, depth);
        }



        float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}

        fixed4 frag (v2f2 i) : SV_Target
        {
            float halfScaleFloor = floor(_Scale * 0.5);
				float halfScaleCeil = ceil(_Scale * 0.5);

				float2 bottomLeftUV = i.uv - float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleFloor;
				float2 topRightUV = i.uv + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleCeil;  
				float2 bottomRightUV = i.uv + float2(_MainTex_TexelSize.x * halfScaleCeil, -_MainTex_TexelSize.y * halfScaleFloor);
				float2 topLeftUV = i.uv + float2(-_MainTex_TexelSize.x * halfScaleFloor, _MainTex_TexelSize.y * halfScaleCeil);

                float4 normalDepth0 = tex2D(_CameraDepthNormalsTexture, bottomLeftUV);
				float4 normalDepth1 = tex2D(_CameraDepthNormalsTexture, topRightUV);
				float4 normalDepth2 = tex2D(_CameraDepthNormalsTexture, bottomRightUV);
				float4 normalDepth3 = tex2D(_CameraDepthNormalsTexture, topLeftUV);
                
                float3 normal0;
                float depth0;
                DecodeDepthNormal(normalDepth0, depth0, normal0);

                float3 normal1;
                float depth1;
                DecodeDepthNormal(normalDepth1, depth1, normal1);

                float3 normal2;
                float depth2;
                DecodeDepthNormal(normalDepth2, depth2, normal2);

                float3 normal3;
                float depth3;
                DecodeDepthNormal(normalDepth3, depth3, normal3);

                
				// float3 normal0 = tex2D(_CameraNormalsTexture, bottomLeftUV).rgb;
				// float3 normal1 = tex2D(_CameraNormalsTexture, topRightUV).rgb;
				// float3 normal2 = tex2D(_CameraNormalsTexture, bottomRightUV).rgb;
				// float3 normal3 = tex2D(_CameraNormalsTexture, topLeftUV).rgb;

				// float depth0 = tex2D(_CameraDepthTexture, bottomLeftUV).r;
				// float depth1 = tex2D(_CameraDepthTexture, topRightUV).r;
				// float depth2 = tex2D(_CameraDepthTexture, bottomRightUV).r;
				// float depth3 = tex2D(_CameraDepthTexture, topLeftUV).r;

                // float3 normal0 = normalDepth0.rgb;
                // float3 normal1 = normalDepth1.rgb;
				// float3 normal2 = normalDepth2.rgb;
				// float3 normal3 = normalDepth3.rgb;

				// float depth0 = normalDepth0.a;
				// float depth1 = normalDepth1.a;
				// float depth2 = normalDepth2.a;
				// float depth3 = normalDepth3.a;

                // return depth0;


				// Transform the view normal from the 0...1 range to the -1...1 range.
				float3 viewNormal = normal0 * 2 - 1;
				float NdotV = 1 - dot(viewNormal, -i.viewSpaceDir);

				// Return a value in the 0...1 range depending on where NdotV lies 
				// between _DepthNormalThreshold and 1.
				float normalThreshold01 = saturate((NdotV - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
				// Scale the threshold, and add 1 so that it is in the range of 1..._NormalThresholdScale + 1.
				float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;

				float depthThreshold = _DepthThreshold * depth0 * normalThreshold;

				float depthFiniteDifference0 = depth1 - depth0;
				float depthFiniteDifference1 = depth3 - depth2;
				float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;
				edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

				float3 normalFiniteDifference0 = normal1 - normal0;
				float3 normalFiniteDifference1 = normal3 - normal2;
				float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
				edgeNormal = edgeNormal > _NormalThreshold ? 1 : 0;

				float edge = max(edgeDepth, edgeNormal);

				float4 edgeColor = float4(_EdgeColor.rgb, _EdgeColor.a * edge);

				float4 color = tex2D(_MainTex, i.uv);

				return alphaBlend(edgeColor, color);















            fixed4 col = tex2D(_MainTex, i.uv);
            fixed4 orValue = GetPixelValue(i.uv);

            // return fixed4(orValue.xyz, 1);


            float2 offsets[8] = {
                float2(-1, -1),
                float2(-1, 0),
                float2(-1, 1),
                float2(0, -1),
                float2(0, 1),
                float2(1, -1),
                float2(1, 0),
                float2(1, 1)
            };
            fixed4 sampledValue = fixed4(0,0,0,0);
            for(int j = 0; j < 8; j++) {
                sampledValue += GetPixelValue(i.uv + offsets[j] * _MainTex_TexelSize.xy);
            }
            sampledValue /= 8;

            fixed4 bgColor = fixed4(_BackgroundColor.rgb, 1);
            return lerp(lerp(col, bgColor, _BackgroundColor.a), _EdgeColor, step(_Parameters.x, length(orValue - sampledValue)));
        }
        

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
                #pragma vertex ImgFXVertex2
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
            ENDHLSL
        }
    }
}
