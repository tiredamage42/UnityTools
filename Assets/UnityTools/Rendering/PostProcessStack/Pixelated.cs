using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(PixelatedRenderer), PostProcessEvent.AfterStack, "Custom/Pixelated")]
    public sealed class Pixelated : PostProcessEffectSettings
    {
        [Range(16.0f, 512.0f)] public FloatParameter blockCount = new FloatParameter() { value = 128 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class PixelatedRenderer : PostProcessEffectRenderer<Pixelated>
    {
        static Shader _shader;
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Pixelated", ref _shader); } }         
		static readonly int _BlockCountSize = Shader.PropertyToID("_BlockCountSize");
		
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            float k = context.camera.aspect;
            Vector2 count = new Vector2(settings.blockCount, settings.blockCount/k);
            Vector2 size = new Vector2(1.0f/count.x, 1.0f/count.y);
            sheet.properties.SetVector(_BlockCountSize, new Vector4(count.x, count.y, size.x, size.y));            
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}