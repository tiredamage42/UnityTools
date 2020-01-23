
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityTools.Rendering
{

    [Serializable]
    [PostProcess(typeof(FlareRenderer), PostProcessEvent.BeforeStack, "Custom/LensFlare")]
    public sealed class LensFlare : PostProcessEffectSettings
    {
        [Range(0, 6)] public IntParameter downsample = new IntParameter() { value = 2 };

        [Header("Ghosts")]
        [Range(0, 8)] public IntParameter ghosts = new IntParameter() { value = 5 };
        [Range(0, 2)] public FloatParameter ghostDisplacement = new FloatParameter() { value = 0.4f };
        [Range(0, 3)] public FloatParameter ghostThreshold = new FloatParameter() { value = 3 };
        
        [Header("Halo")]
        [Range(0, 3)] public FloatParameter haloThreshold = new FloatParameter() { value = 3 };
        [Range(0, 3)] public FloatParameter haloRadius = new FloatParameter() { value = .6f };
        [Range(0, .4f)] public FloatParameter haloThickness = new FloatParameter() { value = .4f };

        [Header("Chromatic Aberration")]
        public FloatParameter chromaticDisplacement = new FloatParameter();
        
        [Header("Blur")]
        [Range(1, 4)] public IntParameter blurIterations = new IntParameter() { value = 1 };
        [Range(0, 5)] public FloatParameter blurSize = new FloatParameter() { value = 1 };
    }

    public sealed class FlareRenderer : PostProcessEffectRenderer<LensFlare>
    {
        const int lookupRes = 64;
        static Texture2D _noiseLookup;
        static Texture2D noiseLookup {
            get {
                if (_noiseLookup == null) {
                    _noiseLookup = new Texture2D(lookupRes, 1);
                    
                    Color32[] colors = new Color32[lookupRes];
                    for (int i = 0; i < lookupRes; i++) {
                        byte r = (byte)( Mathf.Clamp01(UnityEngine.Random.value + .25f) * 255);
                        colors[i] = new Color32(r, r, r, 1);
                    }
                    
                    _noiseLookup.SetPixels32(colors);
                    _noiseLookup.Apply();
                    _noiseLookup.hideFlags = HideFlags.HideAndDontSave;
                }
                return _noiseLookup;
            }
        }


        static Texture2D _lensColor;
        static Texture2D lensColor {
            get {
                if (_lensColor == null)
                    _lensColor = Resources.Load<Texture2D>("LensColor");
                return _lensColor;
            }
        } 
        
        static Texture2D _lensDirt;
        static Texture2D lensDirt {
            get {
                if (_lensDirt == null)
                    _lensDirt = Resources.Load<Texture2D>("LensDirt02");
                return _lensDirt;
            }
        } 

        static Shader _shader; 
		static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/LensFlare", ref _shader); } }
        
        
        static int RT_features = Shader.PropertyToID("RT_Add");
        static int RT_aberration = Shader.PropertyToID("RT_Abb");


        static int _GhostParameters = Shader.PropertyToID("_GhostParameters");
        static int _HaloParameters = Shader.PropertyToID("_HaloParameters");
        static int _LensColor = Shader.PropertyToID("_LensColor");
        static int _LensArtifacts = Shader.PropertyToID("_LensArtifacts");
        static int _LensDirt = Shader.PropertyToID("_LensDirt");
        static int _LensStar = Shader.PropertyToID("_LensStar");
        static int _CamFwd = Shader.PropertyToID("_CamFwd");
            
        public override void Render(PostProcessRenderContext context)
        {
            RenderTextureFormat format = RenderTextureFormat.Default;
            int w = context.screenWidth >> settings.downsample;
            int h = context.screenHeight >> settings.downsample;

            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetVector(_GhostParameters, new Vector3(settings.ghosts, settings.ghostDisplacement, settings.ghostThreshold));
            sheet.properties.SetVector(_HaloParameters, new Vector3(settings.haloRadius, settings.haloThickness, settings.haloThreshold));
            sheet.properties.SetTexture(_LensColor, lensColor);
            
            context.command.GetTemporaryRT(RT_features, w, h, 0, FilterMode.Bilinear, format);
            context.command.BlitFullscreenTriangle(context.source, RT_features, sheet, 0);
            
            // CHROMATIC ABERRATION            
            context.command.GetTemporaryRT(RT_aberration, w, h, 0, FilterMode.Bilinear, format);
            ColorShiftRenderer.DoRender(context, RT_features, RT_aberration, settings.chromaticDisplacement, true);
            
            // BLUR
            int RT_blur1 = Blur.BlurImage(context, RT_aberration, w, h, format, 0, settings.blurSize, settings.blurIterations);

            // COMPOSITE
            context.command.SetGlobalTexture(_LensArtifacts, RT_blur1);
            context.command.SetGlobalTexture(_LensDirt, lensDirt);
            context.command.SetGlobalTexture(_LensStar, noiseLookup);
            context.command.SetGlobalVector(_CamFwd, context.camera.transform.forward);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 1);
            
            context.command.ReleaseTemporaryRT(RT_features);
            context.command.ReleaseTemporaryRT(RT_aberration);
            context.command.ReleaseTemporaryRT(RT_blur1);
        }
    }
}