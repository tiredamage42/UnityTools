using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(RadialBlurRenderer), PostProcessEvent.AfterStack, "Custom/RadialBlur")]
    public sealed class RadialBlur : PostProcessEffectSettings {
        [Range(0f, 1f)] public FloatParameter amount = new FloatParameter { value = .2f };    
        [Range(2, 25)] public IntParameter iterations = new IntParameter { value = 10 };    
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && amount.value > 0;
        }
    }
    public sealed class RadialBlurRenderer : PostProcessEffectRenderer<RadialBlur>
    {
        static Shader _shader; 
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/RadialBlur", ref _shader); } }
        

		static int _RadialBlurParameters = Shader.PropertyToID("_RadialBlurParameters");
        static int _BlurRadius4 = Shader.PropertyToID ("_BlurRadius4");
        
        
        public override void Render(PostProcessRenderContext context)
        {
            DoRadialBlur(context, context.source, context.destination, settings.amount, settings.iterations, Vector2.one * .5f);
        }

        public static RenderTargetIdentifier DoRadialBlur(PostProcessRenderContext context, RenderTargetIdentifier source, RenderTargetIdentifier destination, float amount, int iterations, Vector2 center)
        {
            iterations = Mathf.Clamp(iterations, 2, 25);
            amount = Mathf.Clamp01(amount);
            center.x = Mathf.Clamp01(center.x);
            center.y = Mathf.Clamp01(center.y);
            
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetVector(_RadialBlurParameters, new Vector4(center.x, center.y, iterations, 1.0f / ((float)iterations) * amount));
            context.command.BlitFullscreenTriangle(source, destination, sheet, 0);
            return destination;
        }

        public static RenderTargetIdentifier DoRadialBlurAlternate(PostProcessRenderContext context, RenderTargetIdentifier source, RenderTargetIdentifier destination, float amount, int iterations, Vector2 center)
        {
            iterations = Mathf.Clamp(iterations, 1, 4);
            amount = Mathf.Clamp(amount, 1.0f, 10.0f);
            center.x = Mathf.Clamp01(center.x);
            center.y = Mathf.Clamp01(center.y);

            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetVector (_RadialBlurParameters, center);
            sheet.properties.SetFloat (_BlurRadius4, amount * (1.0f / 768.0f));
            
            for (int i = 0; i < iterations; i++ ) {
                BlurIteration (context.command, sheet, source, destination, i, 1.0f, amount);
                BlurIteration (context.command, sheet, destination, source, i, 2.0f, amount);
            }		
            return source;
        }
        static void BlurIteration (CommandBuffer command, PropertySheet sheet, RenderTargetIdentifier source, RenderTargetIdentifier destination, int i, float add, float amount) {
            command.BlitFullscreenTriangle(source, destination, sheet, 1);	
            // each iteration takes 2 * 6 samples we update _BlurRadius each time to cheaply get a very smooth look
            sheet.properties.SetFloat (_BlurRadius4, amount * (((i * 2.0f + add) * 6.0f)) / 768.0f);
        }       
    }
}