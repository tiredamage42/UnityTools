using UnityEngine;
using UnityEngine.Rendering;
namespace UnityTools.Rendering.VolumetricLighting {

    /*
        until i figure out how to calculate the world2 shadow matrices for each cascade
        for a directional light, we cache them in a 4x4 rgba texture,
        and rebuild them in the vertex program when rendering the directional light
    */
    public class World2ShadowCache
    {
        static Material _material;
        static Material material { get { return RenderUtils.CreateMaterialIfNull("Hidden/TransferWorld2Shadow", ref _material); } }

        CommandBuffer cb;
        RenderTexture _world2ShadowTex;
        public RenderTexture world2ShadowTex {
            get {
                if (_world2ShadowTex == null) {
                    _world2ShadowTex = RenderUtils.GetRenderTexture(4, 4, 0, RenderTextureFormat.ARGBFloat, FilterMode.Point);
                    _world2ShadowTex.wrapMode = TextureWrapMode.Clamp;
                }
                return _world2ShadowTex;
            }
        }
       
        public void OnEnable(Light light) {

            if(light.type != LightType.Directional) 
                return;
            
            if (cb == null) {
                cb = new CommandBuffer();
                cb.name = "Cache World2Shadow Matrices";
            }            
            light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, cb);  
        }

        public void OnDisable() {
            if (_world2ShadowTex != null)
                RenderUtils.ReleaseRenderTexture(ref _world2ShadowTex);
        }
        
        public void UpdateCommandBuffer (bool drawShadows, LightType type) {
            if(type != LightType.Directional) 
                return;
            
            cb.Clear();
            if (drawShadows)
                cb.Blit(null as Texture, world2ShadowTex, material, 0);
        }        
    }
}
