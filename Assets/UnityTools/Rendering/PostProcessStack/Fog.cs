using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using System.Collections.Generic;
namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(FogRenderer), PostProcessEvent.BeforeStack, "Custom/Fog")]
    public sealed class Fog : PostProcessEffectSettings
    {
        [Range(0,1)] public FloatParameter intensity = new FloatParameter() { value = 1 };

        public ColorParameter startColor = new ColorParameter() { value = new Color32(35, 159, 159, 255) };
        public ColorParameter endColor = new ColorParameter() { value = new Color32(255, 187, 59, 255) };
        [Range(0, 1)] public FloatParameter skyboxAffect = new FloatParameter() { value = 1 };

        [Header("Foreground Noise")]
        public Vector3Parameter fgNoiseSpeed = new Vector3Parameter() { value = new Vector3(.5f, -.25f, .25f) };
        public FloatParameter fgNoiseSize = new FloatParameter() { value = .025f };
        public FloatParameter fgNoiseIntensity = new FloatParameter() { value = 25 };
        
        [Header("Backgound Noise")]
        public Vector3Parameter bgNoiseSpeed = new Vector3Parameter() { value = new Vector3(.1f, .1f, 1) };
        public FloatParameter bgNoiseSize = new FloatParameter() { value = 3f };
        [Range(0,1)] public FloatParameter bgNoiseOffset = new FloatParameter() { value = 0f };
                        

        [Header("Height Fog")]
        public FloatParameter height = new FloatParameter() { value = 0 };
        public FloatParameter heightFade = new FloatParameter() { value = 10 };
        public FloatParameter heightStart = new FloatParameter() { value = 0 };
        public FloatParameter heightEnd = new FloatParameter() { value = 25 };
        public FloatParameter heightNoiseIntensity = new FloatParameter() { value = 5 };

        
        
        [Header("Distances")]
        public FloatParameter fogStart = new FloatParameter() { value = 0 };
        public FloatParameter fogEnd = new FloatParameter() { value = 75 };
        [Range(.1f, 3)] public FloatParameter fogSteepness = new FloatParameter() { value = 1 };
        public FloatParameter colorStart = new FloatParameter() { value = 0 };
        public FloatParameter colorEnd = new FloatParameter() { value = 25 };
        [Range(.1f, 3)] public FloatParameter colorSteepness = new FloatParameter() { value = 1 };
    
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class FogRenderer : PostProcessEffectRenderer<Fog>
    {
        static Shader _shader; 
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Fog", ref _shader); } }
        
		
        static int _NoiseTexture = Shader.PropertyToID("_NoiseTexture");        
        static int _NoiseParams = Shader.PropertyToID("_NoiseParams");
        
        static int _BGNoiseSpeed = Shader.PropertyToID("_BGNoiseSpeed");
        static int _SkyRT = Shader.PropertyToID("_SkyRT");
        static int _SkyNoise = Shader.PropertyToID("_SkyNoise");
        
        static int _FogParams = Shader.PropertyToID("_FogParams");
        static int _Params2 = Shader.PropertyToID("_Params2");
        static int _Color0 = Shader.PropertyToID("_Color0");
        static int _Color1 = Shader.PropertyToID("_Color1");
        static int _ViewProjInv = Shader.PropertyToID("_ViewProjInv");
        static int _FGNoiseSpeed = Shader.PropertyToID("_FGNoiseSpeed");

        static int _HeightParams = Shader.PropertyToID("_HeightParams");
        static int _HeightParams2 = Shader.PropertyToID("_HeightParams2");

    
        static Mesh _skyMesh;
        static Mesh skyMesh {
            get {
                if (_skyMesh == null) {
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    _skyMesh = obj.GetComponent<MeshFilter>().sharedMesh;
                    if (Application.isPlaying) 
                        MonoBehaviour.Destroy(obj);
                    else 
                        MonoBehaviour.DestroyImmediate(obj);
                }
                return _skyMesh;
            }
        }

        
        Material _skyNoiseMaterial;
        Material skyNoiseMaterial { get { return RenderUtils.CreateMaterialIfNull("Hidden/FogSkyNoise", ref _skyNoiseMaterial); } }

        public override void Render(PostProcessRenderContext context)
        {
            if (RenderSettings.fog) 
                Debug.LogError("Multiple Fogs Active! Choose either Custom Post Process Fog or RenderSettings Fog...");

            Camera cam = context.camera;

            // set the 3d noise texture we're using
            context.command.SetGlobalTexture(_NoiseTexture, RenderUtils.noiseTexture3d);
            // set the noise parameters for both passes
            context.command.SetGlobalVector(_NoiseParams, new Vector4(settings.fgNoiseSize, settings.fgNoiseIntensity, settings.bgNoiseSize, settings.bgNoiseOffset));

            // draw the background 'sky' noise seperately, so we can use uv's from the sky sphere
            // as opposed to screen space uvs...            
            context.command.SetGlobalVector(_BGNoiseSpeed, settings.bgNoiseSpeed);            
            context.command.GetTemporaryRT(_SkyRT, -4, -4, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
            context.command.SetRenderTarget(_SkyRT);
            
            // follow camera position, no rotation, size = camera far clip (like a mockup skybox)
            Matrix4x4 skyMatrix = Matrix4x4.TRS(cam.transform.position, Quaternion.identity, Vector3.one * cam.farClipPlane);
            context.command.DrawMesh(skyMesh, skyMatrix, skyNoiseMaterial, 0, 0);
            context.command.SetGlobalTexture(_SkyNoise, _SkyRT);
            

            // darw the fog image effect
            var sheet = context.propertySheets.Get(shader);
        
            Matrix4x4 viewMat = cam.worldToCameraMatrix;
            Matrix4x4 projMat = GL.GetGPUProjectionMatrix( cam.projectionMatrix, false );
            Matrix4x4 viewProjMat = (projMat * viewMat);  
            sheet.properties.SetMatrix(_ViewProjInv, viewProjMat.inverse);

            sheet.properties.SetVector(_FogParams, new Vector4(settings.fogStart, settings.fogEnd, settings.colorStart, settings.colorEnd));
            sheet.properties.SetVector(_Params2, new Vector4(settings.fogSteepness, settings.colorSteepness, settings.skyboxAffect, settings.intensity));

            sheet.properties.SetVector(_HeightParams, new Vector4(settings.height, settings.heightFade, settings.heightStart, settings.heightEnd));
            sheet.properties.SetVector(_HeightParams2, new Vector4(settings.heightNoiseIntensity, 0, 0, 0));
            sheet.properties.SetVector(_FGNoiseSpeed, settings.fgNoiseSpeed);            

            sheet.properties.SetColor(_Color0, settings.startColor);
            sheet.properties.SetColor(_Color1, settings.endColor);
            
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            context.command.ReleaseTemporaryRT(_SkyRT);
        }
    }
}