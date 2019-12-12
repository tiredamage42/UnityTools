
Shader "Hidden/CustomFastBlur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
				
		uniform fixed _Parameter;
		uniform fixed4 _MainTex_TexelSize;
		fixed4 _MainTex_ST;

		struct v2f_tap
		{
			fixed4 pos : SV_POSITION;
			fixed2 uv20 : TEXCOORD0;
			fixed2 uv21 : TEXCOORD1;
			fixed2 uv22 : TEXCOORD2;
			fixed2 uv23 : TEXCOORD3;
		};			

		v2f_tap vert4Tap ( appdata_img v )
		{
			v2f_tap o;

			o.pos = UnityObjectToClipPos (v.vertex);
        	o.uv20 = UnityStereoScreenSpaceUVAdjust(v.texcoord + _MainTex_TexelSize.xy, _MainTex_ST);
			o.uv21 = UnityStereoScreenSpaceUVAdjust(v.texcoord + _MainTex_TexelSize.xy * fixed2(-0.5h,-0.5h), _MainTex_ST);
			o.uv22 = UnityStereoScreenSpaceUVAdjust(v.texcoord + _MainTex_TexelSize.xy * fixed2(0.5h,-0.5h), _MainTex_ST);
			o.uv23 = UnityStereoScreenSpaceUVAdjust(v.texcoord + _MainTex_TexelSize.xy * fixed2(-0.5h,0.5h), _MainTex_ST);

			return o; 
		}					
		
		fixed4 fragDownsample ( v2f_tap i ) : SV_Target
		{				
			fixed4 color = tex2D (_MainTex, i.uv20);
			color += tex2D (_MainTex, i.uv21);
			color += tex2D (_MainTex, i.uv22);
			color += tex2D (_MainTex, i.uv23);
			return color / 4;
		}
	
		// weight curves

		/*
			need to blur alpha as well
		*/
		static const fixed curve[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };  // gauss'ish blur weights

		struct v2f_withBlurCoords8 
		{
			fixed4 pos : SV_POSITION;
			fixed4 uv : TEXCOORD0;
			fixed2 offs : TEXCOORD1;
		};	
		

        v2f_withBlurCoords8 Vert (appdata_img v, fixed2 uv)
		{
			v2f_withBlurCoords8 o;
			o.pos = UnityObjectToClipPos (v.vertex);
			
			o.uv = fixed4(v.texcoord.xy,1,1);
			o.offs = _MainTex_TexelSize.xy * uv * _Parameter;
			return o; 
		}

		v2f_withBlurCoords8 vertBlurHorizontal (appdata_img v)
		{
            return Vert(v, fixed2(1, 0));
		}
		
		v2f_withBlurCoords8 vertBlurVertical (appdata_img v)
		{
            return Vert(v, fixed2(0, 1));
		}	

		fixed4 fragBlur8 ( v2f_withBlurCoords8 i ) : SV_Target
		{
			fixed2 uv = i.uv.xy; 
			fixed2 netFilterWidth = i.offs;  
			fixed2 coords = uv - netFilterWidth * 3.0;  
			
			fixed4 color = 0;
  			[unroll] for( int l = 0; l < 7; l++ )  
  			{   
				color += tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(coords, _MainTex_ST)) * curve[l];
				coords += netFilterWidth;
  			}
            return color;
		}
					
	ENDCG
	
	SubShader {
	    ZTest Off Cull Off ZWrite Off Blend Off
        // 0
        Pass { 
            CGPROGRAM
            #pragma vertex vert4Tap
            #pragma fragment fragDownsample
			#pragma fragmentoption ARB_precision_hint_fastest
		
            ENDCG
		}
        // 1
        Pass {
            ZTest Always //Cull Off
            CGPROGRAM 
            #pragma vertex vertBlurVertical
            #pragma fragment fragBlur8
			#pragma fragmentoption ARB_precision_hint_fastest
		
            ENDCG 
		}	
        Pass {		
            ZTest Always //Cull Off
            CGPROGRAM
            #pragma vertex vertBlurHorizontal
            #pragma fragment fragBlur8
			#pragma fragmentoption ARB_precision_hint_fastest
            ENDCG
		}	
	}	
	FallBack Off
}

