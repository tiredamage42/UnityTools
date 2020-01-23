using UnityEngine;
using UnityEngine.Rendering;
namespace UnityTools.Rendering.VolumetricLighting {

    /*
        lights that use shadows dont update shadow maps when theres nothing to receive shadows
        in the camear view, and within the light range.

        this means the volumetric mesh doesnt render as the samepled shadow map is empty

        so we render the light's mesh again with a special material, so it doesnt render to screen, but
        tricks the camera and light into thinking theres a shadow caster visible
    */

    public class ShadowmapForcer
    {
        static int _layer;
        static int layer {
            get {
                if (_layer == -1) 
                    _layer = LayerMask.NameToLayer("Default");
                return _layer;
            }
        }
        static Material _material;
        static Material material { get { return RenderUtils.CreateMaterialIfNull("Custom/ShadowForce", ref _material); } }

        Matrix4x4 world;
        bool forceShadows;

        public void ForceShadows (ref Matrix4x4 world) {
            this.world = world;
            forceShadows = true;
        }

        public void Update (LightType type) {
            if (forceShadows) {
                Mesh mesh = type == LightType.Point ? RenderUtils.GetMesh(PrimitiveType.Sphere) : VolumetricLightUtils.spotlightMesh;
                Graphics.DrawMesh (mesh, world, material, layer, null, 0, null, ShadowCastingMode.On, true);

                forceShadows = false;
            }
        }
    }
}
