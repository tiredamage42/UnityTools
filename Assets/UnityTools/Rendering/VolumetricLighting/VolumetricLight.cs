
using UnityEngine;
using UnityEditor;

namespace UnityTools.Rendering.VolumetricLighting
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(Light))]
    public class VolumetricLight : MonoBehaviour 
    {
        [Range(0, 10)] public float maxIntensity = 1;
        [Range(0, 1)] public float attenuationStart = .9f;
        [Range(0, 0.999f)] public float mieG = 0.999f;
        
        Light _lightC;
        public Light lightC {
            get {
                if (_lightC == null) 
                    _lightC = GetComponent<Light>();
                return _lightC;
            }
        }

        public float range { get { return lightC.range; } }
        public float spotAngle { get { return lightC.spotAngle; } }
        public Texture cookie { get { return lightC.cookie; } }
        public LightType type { get { return lightC.type; } }


        // cache vector math when preparing lights for rendering...
        public Vector3 cam2Light;
        public float cam2LightSq, cam2LightDist, finalIntensity;


        public ShadowmapCache shadowmapCache = new ShadowmapCache();
        public World2ShadowCache world2ShadowCache = new World2ShadowCache();
        public ShadowmapForcer shadowmapForcer = new ShadowmapForcer();

        void OnEnable() {
            shadowmapCache.OnEnable(lightC);
            world2ShadowCache.OnEnable(lightC);
            VolumetricLightsManager.AddLight(this);
        }
        void OnDisable() {
            DisableVolumetricLight();
        }
        void OnDestroy () {
            DisableVolumetricLight();
        }

        void DisableVolumetricLight () {
            shadowmapCache.OnDisable();
            world2ShadowCache.OnDisable();
            lightC.RemoveAllCommandBuffers();
            VolumetricLightsManager.RemoveLight(this);
        }

        public void UpdateShadowCommandBuffers (bool drawShadows) {
            shadowmapCache.UpdateCommandBuffer(drawShadows, lightC.type);
            world2ShadowCache.UpdateCommandBuffer (drawShadows, lightC.type);
        }

        void Update () {
            shadowmapForcer.Update (lightC.type);
        }


        
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(VolumetricLight))]
    class VolumetricLightEditor : Editor {
        public override void OnInspectorGUI() {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxIntensity"));
            if ((target as VolumetricLight).lightC.type != LightType.Directional) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("attenuationStart"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mieG"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}