using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(WavinessRenderer), PostProcessEvent.BeforeStack, "Custom/Waviness")]
    public sealed class Waviness : PostProcessEffectSettings
    {
        [Range(0f, .5f)] public FloatParameter amount = new FloatParameter { value = .1f };    
        public Vector2Parameter speed = new Vector2Parameter { value = Vector2.one };    

        [Range(.1f, 10f)] public FloatParameter size = new FloatParameter { value = 1f };    
        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && amount.value > 0;
        }
    }

    public sealed class WavinessRenderer : PostProcessEffectRenderer<Waviness>
    {
        static Shader _shader; 
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Waviness", ref _shader); } }

        
        static int _Parameters = Shader.PropertyToID("_Parameters");
        static int _DisplTex = Shader.PropertyToID("_DisplTex");

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            Vector2 speed = settings.speed;

            sheet.properties.SetVector(_Parameters, new Vector4(settings.amount, speed.x, speed.y, 1f/settings.size));
            context.command.SetGlobalTexture(_DisplTex, RenderUtils.displacementTexture);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}


