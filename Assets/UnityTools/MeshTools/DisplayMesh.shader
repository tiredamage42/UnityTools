Shader "Hidden/DisplayMesh"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
    
        Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma shader_feature UVS
			
			#include "UnityCG.cginc"
			static const float _WireThickness = 500; // 100
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2g
			{
				float4 pos : SV_POSITION;
				float3 color : TEXCOORD0;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float3 color : TEXCOORD0;
				float4 dist : TEXCOORD2;
			};
			
			v2g vert (appdata v)
			{
				v2g o;
				o.pos = UnityObjectToClipPos(v.vertex);
				#if defined (UVS)
				o.color = fixed3(v.uv, 0);
				#else
				o.color = v.normal * .5 + .5;
				#endif
				return o;
			}

			void Vert (inout TriangleStream<g2f> triangleStream, v2g i, float3 dist) {
				g2f o;
				o.color = i.color;
				o.pos = i.pos;
				o.dist.xyz = dist * o.pos.w * _WireThickness;
				o.dist.w = 1.0 / o.pos.w;
				triangleStream.Append(o);
			}

			[maxvertexcount(3)]
			void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
			{
				float2 p0 = i[0].pos.xy / i[0].pos.w;
				float2 p1 = i[1].pos.xy / i[1].pos.w;
				float2 p2 = i[2].pos.xy / i[2].pos.w;

				float2 edge0 = p2 - p1;
				float2 edge1 = p2 - p0;
				float2 edge2 = p1 - p0;

				// To find the distance to the opposite edge, we take the
				// formula for finding the area of a triangle Area = Base/2 * Height, 
				// and solve for the Height = (Area * 2)/Base.
				// We can get the area of a triangle by taking its cross product
				// divided by 2.  However we can avoid dividing our area/base by 2
				// since our cross product will already be double our area.
				float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
				
				Vert (triangleStream, i[0], float3( (area / length(edge0)), 0.0, 0.0));
				Vert (triangleStream, i[1], float3( 0.0, (area / length(edge1)), 0.0));
				Vert (triangleStream, i[2], float3( 0.0, 0.0, (area / length(edge2))));
			}

			fixed4 frag (g2f i) : SV_Target
			{
				float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];
				return minDistanceToEdge > 0.9 ? fixed4(i.color, 1) : 1;
			}
			ENDCG
		}
    }
}