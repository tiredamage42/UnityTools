Shader "Hidden/FogSkyNoise" {

    SubShader {
        Cull Front
        ZWrite Off
        ZTest Always

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastes
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 vertex : TEXCOORD1;
            };

            v2f vert (float4 vertex : POSITION)
            {
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(vertex);
                OUT.vertex = -vertex;
                return OUT;
            }

            float3 _BGNoiseSpeed;
            float4 _NoiseParams; // FG size, intensity BG size, offset
            sampler3D _NoiseTexture;
            #define BACKGROUND_NOISE_SIZE _NoiseParams.z
            #define BACKGROUND_NOISE_OFFSET _NoiseParams.w

            fixed frag (v2f IN) : SV_Target
            {
                float3 v = IN.vertex.xyz * .5 + .5;
                v.y = 1.0 - v.y;
                v *= BACKGROUND_NOISE_SIZE;
                v += _BGNoiseSpeed * _Time.x;
                return saturate(tex3D(_NoiseTexture, v).a + BACKGROUND_NOISE_OFFSET);
            }
            ENDCG
        }
    }
}