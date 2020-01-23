using UnityEngine;
using UnityTools.GameSettingsSystem;
using UnityTools.EditorTools;
using UnityEngine.Rendering.PostProcessing;

namespace UnityTools.Rendering {
    [CreateAssetMenu()]
    public class RenderingSettings : GameSettingsObjectSingleton<RenderingSettings>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void _OnLoadGame()
        {
            GameManager.onPlayerCreate += OnPlayerCreate;
        }

        static void OnPlayerCreate() {

            Camera camera = GameManager.playerCamera;
            
            PostProcessLayer layer = camera.gameObject.AddComponent<PostProcessLayer>();
            layer.volumeTrigger = camera.transform;
            layer.volumeLayer = 1 << LayerMask.NameToLayer(postProcessLayer);
            layer.stopNaNPropagation = true;
            layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;

            FastApproximateAntialiasing aa = new FastApproximateAntialiasing();
            aa.fastMode = true;
            aa.keepAlpha = true;
            layer.fastApproximateAntialiasing = aa;

            // Fog fog = new Fog();
            // fog.enabled = false;
            // layer.fog = fog;
            


            // layer.finalBlitToCameraTarget = true;

            layer.Init(instance.postProcessResources);
        }

        const string postProcessLayer = "PostProcessing";
        public PostProcessResources postProcessResources;


        [Header("Outlines")]

        [Tooltip("raise this if depth tested highlight colors bleed into overlay areas")]
        [Range(0.1f, 5)] public float outlineOverlayAlphaHelper = 1;
        public bool alternateDepthOutlines;
        public bool optimizedOutlines;
        public BasicColorDefs outlineColors;
        [NeatArray] public OutlineCollectionDefArray outlineCollections;


        [Header("Tagging")]
        public float tagsMaxRenderDistance = 100;
        public float tagDefaultRimPower = 1.25f;
        public Vector2 tagFlashRimPowerRange = new Vector2(10, .5f);
        public float tagFlashSteepness = .5f;
        public float tagFlashSpeed = 2;
        public BasicColorDefs tagColors;
    }
}