#ifndef IMGFXHLSL
#define IMGFXHLSL

#include "UnityCG.cginc"

// half4 _MainTex_TexelSize;


float Hash(uint s)
{
    s = s ^ 2747636419u;
    s = s * 2654435769u;
    s = s ^ (s >> 16);
    s = s * 2654435769u;
    s = s ^ (s >> 16);
    s = s * 2654435769u;
    return float(s) * rcp(4294967296.0); // 2^-32
}


float3 HsvToRgb(float3 c)
{
    const float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}


// Hue, Saturation, Value
// Ranges:
//  Hue [0.0, 1.0]
//  Sat [0.0, 1.0]
//  Lum [0.0, HALF_MAX]
float3 RgbToHsv(float3 c)
{
    const float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    const float e = 1.0e-4;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 PositivePow (float3 base, float3 power) {
    return pow(abs(base), power);
}

float3 LinearToSRGB(float3 c)
{
    float3 sRGBLo = c * 12.92;
    float3 sRGBHi = (PositivePow(c, float3(1.0/2.4, 1.0/2.4, 1.0/2.4)) * 1.055) - 0.055;
    float3 sRGB   = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    return sRGB;
}
float3 SRGBToLinear(float3 c)
{
    float3 linearRGBLo  = c / 12.92;
    float3 linearRGBHi  = PositivePow((c + 0.055) / 1.055, float3(2.4, 2.4, 2.4));
    float3 linearRGB    = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
}

        
struct v2f {
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

void StartVertex (float4 vertex, out float4 pos, out float2 uv) {
    pos = float4(vertex.xy, 0.0, 1.0);
    uv = (vertex.xy + 1.0) * 0.5;
    #if UNITY_UV_STARTS_AT_TOP
    // if (_MainTex_TexelSize.y < 0)
        uv.y = 1.0 - uv.y;
	
    #endif
}

v2f ImgFXVertex (float4 vertex : POSITION) {
    v2f o;
    StartVertex(vertex, o.pos, o.uv);
    return o;
}


	

#endif // IMGFXHLSL
