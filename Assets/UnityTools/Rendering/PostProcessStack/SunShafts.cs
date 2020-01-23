
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

using UnityTools.Rendering.Internal;
namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(SunShaftsRenderer), PostProcessEvent.BeforeStack, "Custom/SunShafts")]
    public sealed class SunShafts : PostProcessEffectSettings {
        public ColorParameter sunColor = new ColorParameter() { value = Color.white };
        [Range(.1f, 1.0f)] public FloatParameter maxRadius = new FloatParameter() { value = 0.25f };
        public FloatParameter intensity = new FloatParameter() { value = 1.15f };
        
        [Header("Blur")]
        [Range(1.0f, 10.0f)] public FloatParameter blurRadius = new FloatParameter() { value = 2.5f };
        [Range(1,4)] public IntParameter blurIterations = new IntParameter() { value = 2 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (!enabled.value || intensity.value <= 0)
                return false;

            if (SunShaftEmitter.instance == null) {
                Debug.LogWarning("SunShafts: No sun shaft emitter found in scene");
                return false;
            }
            return true;
        }
    }
    public sealed class SunShaftsRenderer : PostProcessEffectRenderer<SunShafts>
    {
        static Shader _shader; 
		static Shader shader { get { return RenderUtils.GetShaderIfNull ("Hidden/SunShafts", ref _shader); } }

        static int _SunColor = Shader.PropertyToID ("_SunColor");
        static int _SunPosition = Shader.PropertyToID ("_SunPosition");
        static int _ColorBuffer = Shader.PropertyToID ("_ColorBuffer");

        static int _BlurRadius4 = Shader.PropertyToID ("_BlurRadius4");
        static int _BlurredRT1 = Shader.PropertyToID("_BlurredRT1");
        static int _BlurredRT2 = Shader.PropertyToID("_BlurredRT2");

        const int downsample = -4;
        public override void Render(PostProcessRenderContext context)
        {
            Vector3 screenPos = context.camera.WorldToViewportPoint (context.camera.transform.position + SunShaftEmitter.instance.transform.forward * -context.camera.farClipPlane);
            
            if (screenPos.z <= 0) {
                context.command.BlitFullscreenTriangle(context.source, context.destination);
                return;
            }

            context.command.GetTemporaryRT (_BlurredRT1, downsample, downsample, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
            context.command.GetTemporaryRT (_BlurredRT2, downsample, downsample, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
            
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetVector (_SunPosition, new Vector4 (screenPos.x, screenPos.y, screenPos.z, settings.maxRadius));
            context.command.BlitFullscreenTriangle(context.source, _BlurredRT1, sheet, 2);
            
            RenderTargetIdentifier blurred = DoRadialBlur(context.command, sheet, _BlurredRT1, _BlurredRT2, settings.blurRadius, settings.blurIterations);
            
            // put together:
            context.command.SetGlobalTexture (_ColorBuffer, blurred);
            sheet.properties.SetVector (_SunColor, settings.sunColor.value * settings.intensity);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            
            context.command.ReleaseTemporaryRT(_BlurredRT1);
            context.command.ReleaseTemporaryRT(_BlurredRT2);
        }

        static RenderTargetIdentifier DoRadialBlur(CommandBuffer command, PropertySheet sheet, RenderTargetIdentifier source, RenderTargetIdentifier destination, float amount, int iterations) {   
            sheet.properties.SetFloat (_BlurRadius4, amount * (1.0f / 768.0f));
            for (int i = 0; i < iterations; i++ ) {
                BlurIteration (command, sheet, source, destination, i, 1.0f, amount);
                BlurIteration (command, sheet, destination, source, i, 2.0f, amount);
            }		
            return source;
        }
        // each iteration takes 2 * 6 samples we update _BlurRadius each time to cheaply get a very smooth look
        static void BlurIteration (CommandBuffer command, PropertySheet sheet, RenderTargetIdentifier source, RenderTargetIdentifier destination, int i, float add, float amount) {
            command.BlitFullscreenTriangle(source, destination, sheet, 1);	
            sheet.properties.SetFloat (_BlurRadius4, amount * (((i * 2.0f + add) * 6.0f)) / 768.0f);
        }   
    }
}