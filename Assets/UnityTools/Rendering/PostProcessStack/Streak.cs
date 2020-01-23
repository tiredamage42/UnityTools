using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
namespace UnityTools.Rendering
{

    [System.Serializable]
    [PostProcess(typeof(StreakRenderer), PostProcessEvent.BeforeStack, "Custom/Streak")]
    public sealed class Streak : PostProcessEffectSettings
    {
        [Range(0,5)] public FloatParameter threshold = new FloatParameter { value = 1 };
        [Range(0,1)] public FloatParameter stretch = new FloatParameter { value = 0.75f };
        [Range(0,1)] public FloatParameter intensity = new FloatParameter { value = 0 };
        public ColorParameter tint = new ColorParameter { value = new Color(0.55f, 0.55f, 1) };

        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && intensity.value != 0;
        }
    }

    public sealed class StreakRenderer : PostProcessEffectRenderer<Streak>
    {
        static Shader _shader; 

        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Streak", ref _shader); } }

		
        internal static readonly int _Params = Shader.PropertyToID("_Params");
        internal static readonly int _Color = Shader.PropertyToID("_Color");
        internal static readonly int _HighTex = Shader.PropertyToID("_HighTex");
        
        static int prefiltered = Shader.PropertyToID("_RTPrefiltered");

        const int maxMips = 24;


        static int[] BuildMipPyramidIDs (string key) {
            int[] ids = new int[maxMips];
            for (int i = 0; i < maxMips; i++) 
                ids[i] = Shader.PropertyToID(i + key);
            return ids;
        }

        static readonly int[] upMipIDs = BuildMipPyramidIDs("_up");
        static readonly int[] downMipIDs = BuildMipPyramidIDs("_down");
        static Stack<Vector2Int> _mipStack = new Stack<Vector2Int>();

        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            
            // Common parameters.
            sheet.properties.SetVector(_Params, new Vector3 ( settings.threshold, settings.stretch, settings.intensity ));
            sheet.properties.SetColor(_Color, settings.tint);

            // Apply the prefilter and make it half height.
            var width = context.screenWidth;
            var height = context.screenHeight / 2;
            
            context.command.GetTemporaryRT(prefiltered, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
            context.command.BlitFullscreenTriangle(context.source, prefiltered, sheet, 0);

            Vector2Int last = new Vector2Int(prefiltered, width);
            
            _mipStack.Clear();
            int i = 0;
            while (width > 16) // minimum width = 8
            {
                width /= 2;
                context.command.GetTemporaryRT(downMipIDs[i], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
                Vector2Int downRT = new Vector2Int(downMipIDs[i], width);
                context.command.BlitFullscreenTriangle(last.x, downMipIDs[i], sheet, 1);
                _mipStack.Push(last = downRT);
                i++;
                if (i >= maxMips)
                    break;
            }

            // The last element of the stack is in (last), so cut it.
            _mipStack.Pop();

            i = 0;
            // Upsample and combine.
            while (_mipStack.Count > 0)
            {
                Vector2Int hi = _mipStack.Pop();
                context.command.GetTemporaryRT(upMipIDs[i], hi.y, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
                context.command.SetGlobalTexture(_HighTex, hi.x);
                context.command.BlitFullscreenTriangle(last.x, upMipIDs[i], sheet, 2);
                context.command.ReleaseTemporaryRT(last.x);
                context.command.ReleaseTemporaryRT(hi.x);
                last = new Vector2Int(upMipIDs[i], hi.y);
                i++;
                if (i >= maxMips)
                    break;
            }

            // Final composition.
            context.command.SetGlobalTexture(_HighTex, context.source);
            context.command.BlitFullscreenTriangle(last.x, context.destination, sheet, 3);

            // Cleaning up.
            context.command.ReleaseTemporaryRT(last.x);
            context.command.ReleaseTemporaryRT(prefiltered);         
        }
    }
}
