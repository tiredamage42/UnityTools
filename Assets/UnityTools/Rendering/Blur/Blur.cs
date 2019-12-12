using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityTools.Rendering {
    [System.Serializable] public class BlurParameters {
        [Range(0, 2)] public int downsample = 1;
        [Range(0.0f, 10.0f)] public float size = 3.0f;
        [Range(1, 4)] public int iterations = 2;
    }


    public class Blur 
    {
        [System.NonSerialized] static Material _material;
        static Material material {
            get {
                if (_material == null) {
                    _material = new Material(Shader.Find("Hidden/CustomFastBlur"));
                    _material.hideFlags = HideFlags.HideAndDontSave;
                }
                return _material;
            }
        }
        static int _Parameter = Shader.PropertyToID("_Parameter");

        public static void BlurImage (RenderTexture source, RenderTexture destination, BlurParameters parameters) {
            BlurImage(source, destination, parameters.downsample, parameters.size, parameters.iterations);
        }
        public static void BlurImage (RenderTexture source, RenderTexture destination, int downsample, float size, int iterations) {
            if (iterations < 1) {
                Debug.LogWarning("Need at least one blur iteration... setting to one");
                iterations = 1;
            }
            float widthMod = 1.0f / (1.0f * (1<<downsample));

            material.SetVector (_Parameter, new Vector4 (size * widthMod, -size * widthMod, 0.0f, 0.0f));
            source.filterMode = FilterMode.Bilinear;

            int rtW = source.width >> downsample;
            int rtH = source.height >> downsample;

            // downsample
            RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
            rt.filterMode = FilterMode.Bilinear;
            Graphics.Blit (source, rt, material, 0);

            RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
                
            for(int i = 0; i < iterations; i++) {
                float iterationOffs = (i*1.0f);
                material.SetFloat (_Parameter, size * widthMod + iterationOffs);
                // vertical blur
                Graphics.Blit (rt, rt2, material, 1);
                // horizontal blur
                Graphics.Blit (rt2, i == iterations - 1 ? destination : rt, material, 2);
            }

            RenderTexture.ReleaseTemporary (rt);
            RenderTexture.ReleaseTemporary (rt2);
        }
    }
}
