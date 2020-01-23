
#ifndef VOLUMETRICLIGHTSSPOTPOINT_INCLUDED
#define VOLUMETRICLIGHTSSPOTPOINT_INCLUDED


#include "VolumetricLights.cginc"


// x: 1 - g^2, y: 1 + g^2, z: 2*g, (mie g)
// w light range
float4 _VolumetricLight;

float MieScattering(float cosAngle)
{
    return k_mieg * (_VolumetricLight.x / (pow(_VolumetricLight.y - _VolumetricLight.z * cosAngle, 1.5)));    			
}

#define LIGHT_RANGE _VolumetricLight.w

float3 _CameraForward;
		
struct v2f {
    float4 pos : SV_POSITION;
    float4 uv : TEXCOORD0;
    float3 wpos : TEXCOORD1;
};

struct appdata {
    float4 vertex : POSITION;
};

v2f vert(appdata v) {
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = ComputeScreenPos(o.pos);
    o.wpos = mul(unity_ObjectToWorld, v.vertex);
    return o;
}

#if defined (USE_COOKIE)
    float4x4 _MyLightMatrix0;
#endif

    
float4 _AttenuationParams;
#define INNER_ATTENUATION_LIMIT  _AttenuationParams.x
#define ATTENUATION_RANGE  _AttenuationParams.y

float GetAttenuation (float distance) 
{
    return 1.0 - saturate((distance - INNER_ATTENUATION_LIMIT) / ATTENUATION_RANGE);
}


#if defined(POINT)
    
#if defined (USE_SHADOWS)
    samplerCUBE_float _CachedShadowMap;
#endif

    float LightAtten(float3 wpos, float3 toLight, float toLightL)
    {
        float atten = GetAttenuation(toLightL);
            
#if defined (USE_COOKIE)
        atten *= texCUBEbias(_LightTexture0, float4(mul(_MyLightMatrix0, half4(wpos, 1)).xyz, -8)).w;
#endif 

#if defined (USE_SHADOWS)
        float3 av = abs(toLight);
        float znear = .2; //shadow near plane
        float projx = LIGHT_RANGE / (znear - LIGHT_RANGE); 
        float projy = (znear * LIGHT_RANGE) / (znear - LIGHT_RANGE);
        float mydist = -projx + projy / max(max(av.x, av.y), av.z); 
#if defined(UNITY_REVERSED_Z)
        mydist = 1.0 - mydist; 
#endif
        atten *= (SAMPLE_DEPTH_CUBE_TEXTURE(_CachedShadowMap, toLight) < mydist) ? SHADOW_OFF : SHADOW_POWER;
#endif 
        return atten;
    }

#elif defined(SPOT)

#if defined (USE_SHADOWS)
    float4x4 _MyWorld2Shadow;
    sampler2D_float _CachedShadowMap;
#endif

    float LightAtten(float3 wpos, float3 toLight, float toLightL) 
    {
        float atten = GetAttenuation(toLightL);

#if defined (USE_COOKIE)
        float4 uvCookie = mul(_MyLightMatrix0, float4(wpos, 1));
        // negative bias because http://aras-p.info/blog/2010/01/07/screenspace-vs-mip-mapping/
        atten *= tex2Dbias(_LightTexture0, float4(uvCookie.xy / uvCookie.w, 0, -8)).w;
        atten *= uvCookie.w < 0;
#endif

#if defined (USE_SHADOWS)
        float4 coords = mul(_MyWorld2Shadow, float4(wpos, 1));
        atten *= (SAMPLE_DEPTH_TEXTURE_PROJ(_CachedShadowMap, UNITY_PROJ_COORD(coords)) < ((coords).z/(coords).w)) ? SHADOW_OFF : SHADOW_POWER;
#endif
        return atten;
    }
#endif

#define SAMPLES 8
		
float4 RayMarch(float2 screenPos, float3 rayStart, float3 rayDir, float rayLength)
{
    if (rayLength <= 0)
        return 0;

    float offset = GetDitherOffset(screenPos);
    
    float stepSize = rayLength / SAMPLES;
    
    float3 step = rayDir * stepSize;
    float3 wPos = rayStart + step * offset;
    float vlight = 0;

    [unroll] for (int i = 0; i < SAMPLES; ++i)
    {
        float3 toLight = wPos - _LightPos.xyz;
        float toLightL = length(toLight);
        float density = GetDensity(wPos, _NoiseData.x);
        float atten = LightAtten(wPos, toLight, toLightL);
        
        vlight += atten * stepSize * density * max(k_mieg, min(3, _LightColor.w * MieScattering(dot(toLight / toLightL, -rayDir))));
        vlight = max(0, vlight);
        wPos += step;				
    }

    // apply light's color
    return float4(saturate(_LightColor.rgb * _LightColor.w * max(0, vlight)), 0);
}

float GetProjectedDepth (float2 uv, float3 rayDir) {
    return LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv)) / dot(_CameraForward, rayDir);	
}

fixed4 fragPointInside(v2f i) : SV_Target
{	
    float2 uv = i.uv.xy / i.uv.w;
    float3 rayDir = (i.wpos - _WorldSpaceCameraPos);
    float rayLength = length(rayDir);
    rayDir /= rayLength;
    rayLength = min(rayLength, GetProjectedDepth (uv, rayDir));
    return RayMarch(i.pos.xy, _WorldSpaceCameraPos, rayDir, rayLength);
}

#endif // UNITY_CG_INCLUDED
