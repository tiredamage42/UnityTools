#ifndef BLURHLSL
#define BLURHLSL

#include "ImgFX.hlsl"

static const int blurWeightsCount = 7;
// gauss'ish blur weights
// need to blur alpha as well
static const fixed blurWeights[blurWeightsCount] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };  

uniform fixed _Parameter;
uniform fixed4 _MainTex_TexelSize;
		

struct v2f_blur 
{
    fixed4 pos : SV_POSITION;
    fixed2 uv : TEXCOORD0;
    fixed2 offs : TEXCOORD1;
};

v2f_blur Vert (float4 vertex, fixed2 uv)
{
    v2f_blur o;
    StartVertex(vertex, o.pos, o.uv);
    o.offs = _MainTex_TexelSize.xy * uv * fixed2(_Parameter, -_Parameter);
    return o; 
}
v2f_blur vertBlurHorizontal (float4 vertex : POSITION) {
    return Vert(vertex, fixed2(1, 0));
}
v2f_blur vertBlurVertical (float4 vertex : POSITION) {
    return Vert(vertex, fixed2(0, 1));
}	

#endif // BLURHLSL
