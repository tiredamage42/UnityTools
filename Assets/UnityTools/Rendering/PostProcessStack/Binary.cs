
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
namespace UnityTools.Rendering {
    
    [Serializable]
    [PostProcess(typeof(BinaryRenderer), PostProcessEvent.AfterStack, "Custom/Binary")]
    public sealed class Binary : PostProcessEffectSettings
    {
        // Dither type selector
        [Serializable] public class DiteherTypeParameter : ParameterOverride<RenderUtils.DitherType> {}


        public DiteherTypeParameter ditherType = new DiteherTypeParameter();
        [Range(1, 8)] public IntParameter ditherScale = new IntParameter() { value = 1 };
        public ColorParameter color0 = new ColorParameter() { value = Color.black };
        public ColorParameter color1 = new ColorParameter() { value = Color.white };
        [Range(0, 1)] public FloatParameter opacity = new FloatParameter() { value = 1.0f };
        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && opacity.value > 0;
        }
    }

    public sealed class BinaryRenderer : PostProcessEffectRenderer<Binary>
    {
        static Shader _shader; 
		static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Binary", ref _shader); } }
        
        static readonly int _DitherTex = Shader.PropertyToID("_DitherTex");
        static readonly int _Color0 = Shader.PropertyToID("_Color0");
        static readonly int _Color1 = Shader.PropertyToID("_Color1");
        static readonly int _ScaleOpacity = Shader.PropertyToID("_ScaleOpacity");
        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetTexture(_DitherTex, RenderUtils.DitherTexture(settings.ditherType));
            sheet.properties.SetColor(_Color0, settings.color0);
            sheet.properties.SetColor(_Color1, settings.color1);
            sheet.properties.SetVector(_ScaleOpacity, new Vector2(settings.ditherScale, settings.opacity));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
