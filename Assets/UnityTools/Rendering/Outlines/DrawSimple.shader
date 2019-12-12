Shader "Hidden/OutlinesDrawSimple" {
    SubShader {			
        Lighting Off
        ZWrite On
        // Cull Back
        // Fog { Mode Off }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
		
            fixed4 _OutColor;
            fixed4 vert(fixed4 v:POSITION) : POSITION { return UnityObjectToClipPos (v); }
            fixed4 frag() : COLOR { return _OutColor; }
            ENDCG
        }
    }
}
