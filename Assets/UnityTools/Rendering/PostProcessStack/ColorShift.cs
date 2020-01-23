using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(ColorShiftRenderer), PostProcessEvent.AfterStack, "Custom/ColorShift")]
    public sealed class ColorShift : PostProcessEffectSettings {
        [Range(0f, 50f)] public FloatParameter amount = new FloatParameter { value = 5f };    
        public BoolParameter fromCenter = new BoolParameter();
        public override bool IsEnabledAndSupported(PostProcessRenderContext context) {
            return enabled.value && amount.value > 0;
        }
    }

    public class ColorShiftRenderer : PostProcessEffectRenderer<ColorShift> {
        static Shader _shader; 
		static Shader shader {
			get {
				if (_shader == null) _shader = Shader.Find("Hidden/ColorShift");
				return _shader;
			}
		}
        
        static int _Amount = Shader.PropertyToID("_Amount");

        public override void Render(PostProcessRenderContext context) {
            DoRender(context, context.source, context.destination, settings.amount, settings.fromCenter);
        }

        public static void DoRender (PostProcessRenderContext context, RenderTargetIdentifier source, RenderTargetIdentifier destination, float amount, bool fromCenter) {
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetFloat(_Amount, amount);
            context.command.BlitFullscreenTriangle(source, destination, sheet, fromCenter ? 1 : 0);
        }
    }
}