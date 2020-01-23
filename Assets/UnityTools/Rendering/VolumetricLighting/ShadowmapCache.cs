using UnityEngine;
using UnityEngine.Rendering;

namespace UnityTools.Rendering.VolumetricLighting {

    /*
        if the light we want to render as a volumetric light casts shadows, then we
        cache the shadow map to sample it later when we render all the volumetric lights
        during the image effect render loop
    */
    public class ShadowmapCache
    {
        CommandBuffer cb;
        public RenderTexture shadowMap;
        
        static int ShadowmapResolution (LightType type) {
            switch (type) {
                case LightType.Directional:
                    return 2048; // any lower resolution causes artifacts
                case LightType.Spot:
                    return 128;
                case LightType.Point:
                    return 128;
            }
            return 1;
        }


        public void OnEnable(Light light) {
            if (cb == null) {
                cb = new CommandBuffer();
                cb.name = "Cache Shadowmap";
            }            
            light.AddCommandBuffer(LightEvent.AfterShadowMap, cb);  

            if (shadowMap == null) {
                int res = ShadowmapResolution(light.type);
                shadowMap = RenderUtils.GetRenderTexture(res, res, 0, RenderTextureFormat.RFloat, FilterMode.Point);
                if (light.type == LightType.Point) 
                    shadowMap.dimension = TextureDimension.Cube;
            }
        }
        public void OnDisable() {
            RenderUtils.ReleaseRenderTexture(ref shadowMap);
        }
        
        public void UpdateCommandBuffer (bool drawShadows, LightType type) {
            cb.Clear();

            if (!drawShadows)
                return;

            RenderTargetIdentifier renderingShadowmap = BuiltinRenderTextureType.CurrentActive;
            cb.SetShadowSamplingMode(renderingShadowmap, ShadowSamplingMode.RawDepth);                

            if (type == LightType.Point) 
                RenderUtils.CopyCubeMap (cb, renderingShadowmap, shadowMap, asFloat: true);
            else 
                cb.Blit(renderingShadowmap, shadowMap);    
        }
    }
}
