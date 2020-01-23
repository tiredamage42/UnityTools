using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    [System.Serializable] public class BlurParameters {
        [Tooltip("Default: 1")] [Range(0, 2)] public int downsample = 1;
        [Tooltip("Default: 3")] [Range(0.0f, 10.0f)] public float size = 3.0f;
        [Tooltip("Default: 2")] [Range(1, 4)] public int iterations = 2;
    }

    public class Blur 
    {
        static int _Temp0 = Shader.PropertyToID("_Temp0");
        static int _Temp1 = Shader.PropertyToID("_Temp1");
        static int _Parameter = Shader.PropertyToID("_Parameter");
        static int _Destination = Shader.PropertyToID("_Destination");
        static Shader _blurShader; 
		static Shader blurShader {
			get {
				if (_blurShader == null) _blurShader = Shader.Find("Hidden/CustomFastBlur");
				return _blurShader;
			}
		}
        public static int BlurImage (PostProcessRenderContext context, RenderTargetIdentifier source, int sourceW, int sourceH, RenderTextureFormat sourceFormat, BlurParameters blur) {
            return BlurImage (context, source, sourceW, sourceH, sourceFormat, blur.downsample, blur.size, blur.iterations);
        }
        public static int BlurImage (PostProcessRenderContext context, RenderTargetIdentifier source, int sourceW, int sourceH, RenderTextureFormat sourceFormat, int downsample, float size, int iterations) {
            context.command.GetTemporaryRT(_Destination, sourceW >> downsample, sourceH >> downsample, 0, FilterMode.Bilinear, sourceFormat);
            BlurImage ( context, source, sourceW, sourceH, sourceFormat, _Destination, downsample, size, iterations);
            return _Destination;
        }
        public static void BlurImage (PostProcessRenderContext context, RenderTargetIdentifier source, int sourceW, int sourceH, RenderTextureFormat sourceFormat, int destination, BlurParameters blur) {
            BlurImage (context, source, sourceW, sourceH, sourceFormat, destination, blur.downsample, blur.size, blur.iterations);
        }
        public static void BlurImage (PostProcessRenderContext context, RenderTargetIdentifier source, int sourceW, int sourceH, RenderTextureFormat sourceFormat, int destination, int downsample, float size, int iterations) {
            DoBlurLoop ( context, context.propertySheets.Get(blurShader), 0, source, sourceW, sourceH, sourceFormat, destination, downsample, size, iterations);
        }

        public static void DoBlurLoop (PostProcessRenderContext context, PropertySheet sheet, int startPass, RenderTargetIdentifier source, int sourceW, int sourceH, RenderTextureFormat sourceFormat, int destination, int downsample, float size, int iterations) {
            if (iterations < 1) 
                iterations = 1;
            
            int w = sourceW >> downsample;
            int h = sourceH >> downsample;

            context.command.GetTemporaryRT(_Temp0, w, h, 0, FilterMode.Bilinear, sourceFormat);
            context.command.GetTemporaryRT(_Temp1, w, h, 0, FilterMode.Bilinear, sourceFormat);
            
            float widthMod = (1.0f / (1.0f * (1<<downsample))) * size;
            
            for(int i = 0; i < iterations; i++) {
                sheet.properties.SetFloat (_Parameter, widthMod + i);
                // vertical blur then horizontal
                context.command.BlitFullscreenTriangle(i == 0 ? source : _Temp0, _Temp1, sheet, startPass);
                context.command.BlitFullscreenTriangle(_Temp1, i == iterations - 1 ? destination : _Temp0, sheet, startPass + 1);
            }
            
            context.command.ReleaseTemporaryRT(_Temp0);
            context.command.ReleaseTemporaryRT(_Temp1);
        }
    }
}
