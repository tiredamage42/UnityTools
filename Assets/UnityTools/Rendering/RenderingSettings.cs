using UnityEngine;
using UnityTools.GameSettingsSystem;
using UnityTools.EditorTools;
using UnityEngine.Rendering.PostProcessing;

using UnityTools.DevConsole;

namespace UnityTools.Rendering {
    [CreateAssetMenu()]
    public class RenderingSettings : GameSettingsObjectSingleton<RenderingSettings>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void _OnLoadGame()
        {
            GameManager.onPlayerCreate += OnPlayerCreate;

            GameState.onSettingsLoaded += OnSettingsLoaded;
            GameState.onSettingsSave += OnSettingsSave;
        }
        

        static bool _useVsync;

        [Command("vsync", "enable/disable vsync", "Rendering", false)]
        static bool useVsync {
            get { return _useVsync; }
            set {
                if (value) {
                    QualitySettings.vSyncCount = 1;
                }
                else {
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = uncappedFrameRate;
                }
                _useVsync = value;
            }
        }
        const string USE_VSYNC_KEY = "vsync";
        const int uncappedFrameRate = 500;
        static void OnSettingsLoaded () {
            _useVsync = (bool)GameState.settingsSaveState.Load(USE_VSYNC_KEY);  
        }
        static void OnSettingsSave () {
            GameState.settingsSaveState.UpdateState(USE_VSYNC_KEY, _useVsync);
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