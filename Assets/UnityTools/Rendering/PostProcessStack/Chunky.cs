
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
namespace UnityTools.Rendering {
    
    [Serializable]
    [PostProcess(typeof(ChunkyRenderer), PostProcessEvent.AfterStack, "Custom/Chunky")]
    public sealed class Chunky : PostProcessEffectSettings
    {
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class ChunkyRenderer : PostProcessEffectRenderer<Chunky>
    {
        static Shader _shader; 
		static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Chunky", ref _shader); } }
        static readonly int _BlockCountSize = Shader.PropertyToID("_BlockCountSize");
        static readonly int _SprTex = Shader.PropertyToID("_SprTex");

        static Texture2D _chunkyTex; 
		public static Texture2D chunkyTex {
			get {
				if (_chunkyTex == null) 
                    _chunkyTex = Resources.Load<Texture2D>("Chunky");
				return _chunkyTex;
			}
		}

        public override void Render(PostProcessRenderContext context) {
            var sheet = context.propertySheets.Get(shader);
            Texture2D t = chunkyTex;
            Vector2 count = new Vector2(context.camera.pixelWidth/t.height, context.camera.pixelHeight/t.height);
            Vector2 size = new Vector2(1.0f/count.x, 1.0f/count.y);
            sheet.properties.SetVector(_BlockCountSize, new Vector4( count.x, count.y, size.x, size.y ));
            sheet.properties.SetTexture(_SprTex, chunkyTex);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}