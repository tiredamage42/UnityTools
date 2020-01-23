using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace UnityTools.Rendering
{

    [System.Serializable]
    [PostProcess(typeof(SharpenRenderer), PostProcessEvent.AfterStack, "Custom/Sharpen")]
    public sealed class Sharpen : PostProcessEffectSettings
    {
        [Range(0,1)] public FloatParameter intensity = new FloatParameter();

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && intensity.value != 0;
        }
    }

    public sealed class SharpenRenderer : PostProcessEffectRenderer<Sharpen>
    {
        static Shader _shader; 

        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Sharpen", ref _shader); } }
        
        static readonly int _Intensity = Shader.PropertyToID ("_Intensity");

		public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetFloat(_Intensity, settings.intensity.value);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}