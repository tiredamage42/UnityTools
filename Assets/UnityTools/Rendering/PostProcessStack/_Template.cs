using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(REPLACERenderer), PostProcessEvent.AfterStack, "Custom/REPLACE")]
    public sealed class REPLACE : PostProcessEffectSettings
    {
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class REPLACERenderer : PostProcessEffectRenderer<REPLACE>
    {
        static Shader _shader; 
		static Shader shader {
			get {
				if (_shader == null) _shader = Shader.Find("Hidden/REPLACE");
				return _shader;
			}
		}
        
        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            
            
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}