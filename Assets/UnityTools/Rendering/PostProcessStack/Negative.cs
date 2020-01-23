using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(NegativeRenderer), PostProcessEvent.AfterStack, "Custom/Negative")]
    public sealed class Negative : PostProcessEffectSettings
    {
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }
    public sealed class NegativeRenderer : PostProcessEffectRenderer<Negative>
    {
        static Shader _shader;
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Negative", ref _shader); } }
         
		public override void Render(PostProcessRenderContext context)
        {
            context.command.BlitFullscreenTriangle(context.source, context.destination, context.propertySheets.Get(shader), 0);
        }
    }
}