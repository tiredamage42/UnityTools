using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(CharcoalRenderer), PostProcessEvent.AfterStack, "Custom/Charcoal")]
    public sealed class Charcoal : PostProcessEffectSettings
    {
        public ColorParameter lineColor = new ColorParameter() { value = Color.black };
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class CharcoalRenderer : PostProcessEffectRenderer<Charcoal>
    {
        static Shader _shader; 
		static Shader shader {
			get {
				if (_shader == null) _shader = Shader.Find("Hidden/Charcoal");
				return _shader;
			}
		}
        
        static int _CharcoalParams = Shader.PropertyToID("_CharcoalParams");
        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            
            sheet.properties.SetColor(_CharcoalParams, settings.lineColor);
            
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}