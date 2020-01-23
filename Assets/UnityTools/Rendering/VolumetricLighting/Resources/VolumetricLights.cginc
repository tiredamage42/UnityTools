
#ifndef VOLUMETRICLIGHTS_INCLUDED
#define VOLUMETRICLIGHTS_INCLUDED

#include "UnityCG.cginc"
#include "UnityDeferredLibrary.cginc"

// #define REVERSE_SHADOW

#if defined(REVERSE_SHADOW)
    #define SHADOW_POWER 2.0
    #define SHADOW_OFF 0
#else
    #define SHADOW_POWER -2.0
    #define SHADOW_OFF 1.0
#endif

sampler2D _DitherTexture;
#define DITHER 8.0
static const float2 DITHER_UV = float2(0.5 / DITHER, 0.5 / DITHER);
float GetDitherOffset (float2 screenPos) {
    return tex2D(_DitherTexture, fmod(floor(screenPos), DITHER) / DITHER + DITHER_UV).w;
}

sampler3D _NoiseTexture;
// x: scale, y: dir scale, z: intensity offset, w: post steepness
float4 _NoiseData;
//w unused
float4 _NoiseVelocity;
float GetDensity(float3 wpos, float noiseSize) {
    // return pow(saturate(tex3D(_NoiseTexture, wpos * noiseSize + _NoiseVelocity.xyz).a + _NoiseData.z), _NoiseData.w);
    return saturate((tex3D(_NoiseTexture, wpos * noiseSize + _NoiseVelocity.xyz).a * 2 - 1) + _NoiseData.z);//.z), _NoiseData.w);
}        

#define PI 3.14159265359
static const float k_mieg = 1.0 / (4.0 * PI);

#endif // UNITY_CG_INCLUDED
