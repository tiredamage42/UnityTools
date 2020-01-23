using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(EmbossRenderer), PostProcessEvent.AfterStack, "Custom/Emboss")]
    public sealed class Emboss : PostProcessEffectSettings
    {
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class EmbossRenderer : PostProcessEffectRenderer<Emboss>
    {
        static Shader _shader; 

        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Emboss", ref _shader); } }
        
		public override void Render(PostProcessRenderContext context)
        {
            context.command.BlitFullscreenTriangle(context.source, context.destination, context.propertySheets.Get(shader), 0);
        }
    }
}