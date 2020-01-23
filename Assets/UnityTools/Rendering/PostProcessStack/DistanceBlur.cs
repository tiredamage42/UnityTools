using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    /*
        TODO: nightvision / thermal vision / frost on screen / godrays / height fog (add noise to fog)
    
    */
    
    [System.Serializable]
    [PostProcess(typeof(DistanceBlurRenderer), PostProcessEvent.BeforeStack, "Custom/DistanceBlur")]
    public sealed class DistanceBlur : PostProcessEffectSettings
    {
        [Range(0.1f, 500f)] public FloatParameter startDistance = new FloatParameter { value = 10f };    
        [Range(0.1f, 500f)] public FloatParameter fadeRange = new FloatParameter { value = 3f };
        [Range(0.001f, 10)] public FloatParameter fadeSteepness = new FloatParameter { value = 1 };
        public FloatParameter maxDistance = new FloatParameter { value = 999 };

        [Header("Image Blur")]
        [Range(0, 2)] public IntParameter downsample = new IntParameter { value = 2 };
        [Range(0.0f, 10.0f)] public FloatParameter blurSize = new FloatParameter { value = 4.0f };
        [Range(1, 4)] public IntParameter blurIterations = new IntParameter { value = 1 };


        [Header("Depth Map Blur")]
         [Range(0.1f, 2.0f)] public FloatParameter skyboxBleed = new FloatParameter { value = .5f };
        [Range(0, 2)] public IntParameter fadeDownsample = new IntParameter { value = 0 };
        [Range(0.0f, 10.0f)] public FloatParameter fadeBlurSize = new FloatParameter { value = 10.0f };
        [Range(1, 4)] public IntParameter fadeBlurIterations = new IntParameter { value = 4 };

        public BoolParameter debugVisuals = new BoolParameter { value = false };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class DistanceBlurRenderer : PostProcessEffectRenderer<DistanceBlur>
    {
        /*
        Look into /PostProcessing/Runtime/PostProcessRenderContext.cs for a list of what's available (the file is heavily commented).
        
        void Init(): called when the renderer is created.
        DepthTextureMode GetLegacyCameraFlags(): used to set camera flags and request depth map, motion vectors, etc.
        void ResetHistory(): called when a "reset history" event is dispatched. Mainly used for temporal effects to clear history buffers and whatnot.
        void Release(): called when the renderer is destroyed. Do your cleanup there if you need it
        */

        static int _DepthBlurMap = Shader.PropertyToID("_DepthBlurMap");
        static int _DepthBlurMapID = Shader.PropertyToID("_DepthBlurMapID");
        static int _BlurredSource = Shader.PropertyToID("_BlurredSource");

        static Shader _distanceBlurShader; 
		static Shader distanceBlurShader {
			get {
				if (_distanceBlurShader == null) _distanceBlurShader = Shader.Find("Hidden/Custom/DistanceBlur");
				return _distanceBlurShader;
			}
		}

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(distanceBlurShader);
            
            if (settings.debugVisuals) 
                context.command.EnableShaderKeyword("DEBUG_VISUAL");
            else 
                context.command.DisableShaderKeyword("DEBUG_VISUAL");
            
            int blurredID = Blur.BlurImage(context, context.source, context.screenWidth, context.screenHeight, context.sourceFormat, settings.downsample, settings.blurSize, settings.blurIterations);
            
            GetBlurredDepthMap(context, sheet, settings.fadeDownsample, settings.fadeBlurSize, settings.fadeBlurIterations);
            
            context.command.SetGlobalTexture(_BlurredSource, blurredID);
            context.command.SetGlobalTexture(_DepthBlurMap, _DepthBlurMapID);
            
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 3);

            context.command.ReleaseTemporaryRT(blurredID);
            context.command.ReleaseTemporaryRT(_DepthBlurMapID);
        }

        static int _UnblurredMap = Shader.PropertyToID("_UnblurredMap");
        static int _UnblurredMapID = Shader.PropertyToID("_UnblurredMapID");
        static int _SkyboxBleed = Shader.PropertyToID("_SkyboxBleed");
        static int _DistBlurParams = Shader.PropertyToID("_DistBlurParams");
        
        void GetBlurredDepthMap (PostProcessRenderContext context, PropertySheet sheet, int downsample, float size, int iterations) {
        
            sheet.properties.SetFloat(_SkyboxBleed, settings.skyboxBleed);
            sheet.properties.SetVector(_DistBlurParams, new Vector4(settings.startDistance, settings.fadeRange, settings.fadeSteepness, settings.maxDistance));

            int rtW = context.screenWidth >> downsample;
            int rtH = context.screenHeight >> downsample;

            context.command.GetTemporaryRT(_UnblurredMapID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            context.command.GetTemporaryRT(_DepthBlurMapID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
            
            // build map
            context.command.BlitFullscreenTriangle(_DepthBlurMapID, _UnblurredMapID, sheet, 0);
            context.command.SetGlobalTexture(_UnblurredMap, _UnblurredMapID);
            
            Blur.DoBlurLoop ( context, sheet, 1, _UnblurredMapID, context.screenWidth, context.screenHeight, RenderTextureFormat.R8, _DepthBlurMapID, downsample, size, iterations);
            
            context.command.ReleaseTemporaryRT(_UnblurredMapID);
        }
    }

}


