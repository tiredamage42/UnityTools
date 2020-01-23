using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(PixelatedRenderer), PostProcessEvent.AfterStack, "Custom/Pixelated")]
    public sealed class Pixelated : PostProcessEffectSettings
    {
        public IntParameter pixelWidth = new IntParameter() { value = 16 };
        public IntParameter pixelHeight = new IntParameter() { value = 16 };
		

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class PixelatedRenderer : PostProcessEffectRenderer<Pixelated>
    {
        static Shader _shader;
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Pixelated", ref _shader); } }
         
        static readonly int _PixSize = Shader.PropertyToID("_PixSize");
		
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetVector(_PixSize, new Vector2(settings.pixelWidth, settings.pixelHeight));
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}