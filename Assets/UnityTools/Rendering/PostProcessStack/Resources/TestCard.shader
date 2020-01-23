Shader "Hidden/TestCard"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment Fragment
            #include "ImgFX.hlsl"

            sampler2D _MainTex;
            
            float4 _Parameters; // opacity, scale, area.xy

            #define AREA _Parameters.zw
            #define SCALE _Parameters.y
            #define OPACITY _Parameters.x

            float3 TestPattern(float2 uv)
            {
                half2 area = AREA;
                float scale = SCALE;
                
                float2 p0 = (uv - 0.5) * _ScreenParams.xy; // Position (pixel)
                float2 p1 = p0 * scale;                  // Position (half grid)
                float2 p2 = p1 / 2 - 0.5;                // Position (grid)


                // Crosshair and grid lines
                half2 ch = abs(p0);
                half2 grid = (1 - abs(frac(p2) - 0.5) * 2) / scale;
                half c1 = min(min(ch.x, ch.y), min(grid.x, grid.y)) < 1 ? 1 : 0.5;

                // Outer area checker
                half2 checker = frac(floor(p2) / 2) * 2;
                if (any(abs(p1) > area)) 
                    c1 = abs(checker.x - checker.y);

                half corner = sqrt(8) - length(abs(p1) - area + 4); // Corner circles
                half circle = 12 - length(p1);                      // Big center circle
                half mask = saturate(circle / scale);               // Center circls mask

                // Grayscale bars
                half bar1 = saturate(p1.y < 5 ? floor(p1.x / 4 + 3) / 5 : p1.x / 16 + 0.5);
                
                if (abs(5 - p1.y) < 4 * mask) 
                    c1 = bar1;

                // Basic color bars
                half3 bar2 = HsvToRgb(float3((p1.y > -5 ? floor(p1.x / 4) / 6 : p1.x / 16) + 0.5, 1, 1));
                float3 rgb = abs(-5 - p1.y) < 4 * mask ? bar2 : saturate(c1);

                // Circle lines
                return lerp(rgb, 1, saturate(1.5 - abs(max(circle, corner)) / scale));
            }

            float4 Fragment(v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv);
                // Blend the test pattern in sRGB.
                // c.rgb = LinearToSRGB(c.rgb);
                c.rgb = lerp(c.rgb, TestPattern(i.uv), OPACITY);
                // c.rgb = SRGBToLinear(c.rgb);
                return c;
            }
            ENDHLSL
        }
    }
}
