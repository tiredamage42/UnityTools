using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
namespace UnityTools.Rendering {
    public static class RenderUtils {


        public static RenderTexture GetRenderTexture (int w, int h, int depth, RenderTextureFormat format, FilterMode filterMode) {
            RenderTexture tex = RenderTexture.GetTemporary(w, h, depth, format);
            tex.filterMode = filterMode;
            return tex;
        }

        public static void ReleaseRenderTexture (ref RenderTexture tex) {
            RenderTexture.ReleaseTemporary(tex);
            tex = null;
        }
        

        public static Material CreateMaterialIfNull (string shader, ref Material check) {
            if (check == null) {
                check = new Material(Shader.Find(shader));
                check.hideFlags = HideFlags.HideAndDontSave;                
            }
            return check;
        }
        public static Shader GetShaderIfNull (string shader, ref Shader check) {
            if (check == null)
                check = Shader.Find(shader);
            return check;
        }


        public static void EnableKeyword(this CommandBuffer cb, string keyword, bool enabled) {
            if (enabled)
                cb.EnableShaderKeyword(keyword);
            else 
                cb.DisableShaderKeyword(keyword);
        }

        static readonly Matrix4x4 identity = Matrix4x4.identity;
        static readonly int _CubeF = Shader.PropertyToID("_CubeF");
        static readonly int _Cube = Shader.PropertyToID("_Cube");
        static Material _transferCubemap;
        static Material transferCubemap { get { return CreateMaterialIfNull("Hidden/TransferCubeMap", ref _transferCubemap); } }
        public static void CopyCubeMap (CommandBuffer cb, RenderTargetIdentifier cubeMap, RenderTargetIdentifier target, bool asFloat) {
            
            int passOffset = asFloat ? 0 : 6;
            cb.SetGlobalTexture(asFloat ? _CubeF : _Cube, cubeMap);
            Mesh quad = RenderUtils.GetMesh(PrimitiveType.Quad);
            Material mat = transferCubemap;
            for (int i = 0; i < 6; i++) {
                cb.SetRenderTarget(target, 0, (CubemapFace)i);
                cb.DrawMesh(quad, identity, mat, 0, i + passOffset);
            }
            // cb.SetRenderTarget(target, 0, CubemapFace.PositiveX);
            // cb.DrawMesh(RenderUtils.GetMesh(PrimitiveType.Quad), Matrix4x4.identity, transferCubemapMaterial, 0, 0 + passOffset);
            // cb.SetRenderTarget(target, 0, CubemapFace.NegativeX);
            // cb.DrawMesh(RenderUtils.GetMesh(PrimitiveType.Quad), Matrix4x4.identity, transferCubemapMaterial, 0, 1 + passOffset);
            // cb.SetRenderTarget(target, 0, CubemapFace.PositiveY);
            // cb.DrawMesh(RenderUtils.GetMesh(PrimitiveType.Quad), Matrix4x4.identity, transferCubemapMaterial, 0, 2 + passOffset);
            // cb.SetRenderTarget(target, 0, CubemapFace.NegativeY);
            // cb.DrawMesh(RenderUtils.GetMesh(PrimitiveType.Quad), Matrix4x4.identity, transferCubemapMaterial, 0, 3 + passOffset);
            // cb.SetRenderTarget(target, 0, CubemapFace.PositiveZ);
            // cb.DrawMesh(RenderUtils.GetMesh(PrimitiveType.Quad), Matrix4x4.identity, transferCubemapMaterial, 0, 4 + passOffset);
            // cb.SetRenderTarget(target, 0, CubemapFace.NegativeZ);
            // cb.DrawMesh(RenderUtils.GetMesh(PrimitiveType.Quad), Matrix4x4.identity, transferCubemapMaterial, 0, 5 + passOffset);
        }

        static Dictionary<PrimitiveType, Mesh> primitiveMeshes = new Dictionary<PrimitiveType, Mesh>();

        public static Mesh GetMesh(PrimitiveType type) {
            if (!primitiveMeshes.TryGetValue(type, out Mesh m)) {
                m = BuildMesh(type);
                primitiveMeshes[type] = m;
            }
            return m;
        }
        static Mesh BuildMesh(PrimitiveType type) {
            GameObject obj = GameObject.CreatePrimitive(type);
            Mesh m = obj.GetComponent<MeshFilter>().sharedMesh;
            if (Application.isPlaying) 
                MonoBehaviour.Destroy(obj);
            else 
                MonoBehaviour.DestroyImmediate(obj);
            return m;
        }

        static Texture2D _blackTexture;
        public static Texture blackTexture {
            get {
                if (_blackTexture == null) {
                    _blackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false, false);
                    _blackTexture.SetPixel(0, 0, Color.clear);
                    _blackTexture.Apply();
                    _blackTexture.hideFlags = HideFlags.HideAndDontSave;
                }
                return _blackTexture;
            }
        }

        
        public enum DitherType {
            Bayer2x2, Bayer3x3, Bayer4x4, Bayer8x8, BlueNoise64x64
        };


        static Texture2D _bayer2x2Texture; 
		public static Texture2D bayer2x2Texture {
			get {
				if (_bayer2x2Texture == null) 
                    _bayer2x2Texture = Resources.Load<Texture2D>("DitherBayer2");
				return _bayer2x2Texture;
			}
		}
        
        
        static Texture2D _bayer3x3Texture; 
		public static Texture2D bayer3x3Texture {
			get {
				if (_bayer3x3Texture == null) 
                    _bayer3x3Texture = Resources.Load<Texture2D>("DitherBayer3");
				return _bayer3x3Texture;
			}
		}
        
        static Texture2D _bayer4x4Texture; 
		public static Texture2D bayer4x4Texture {
			get {
				if (_bayer4x4Texture == null) 
                    _bayer4x4Texture = Resources.Load<Texture2D>("DitherBayer4");
				return _bayer4x4Texture;
			}
		}
        static Texture2D _bayer8x8Texture; 
		public static Texture2D bayer8x8Texture {
			get {
				if (_bayer8x8Texture == null) 
                    _bayer8x8Texture = Resources.Load<Texture2D>("DitherBayer8");
				return _bayer8x8Texture;
			}
		}
        static Texture2D _bnoise64x64Texture; 
		public static Texture2D bnoise64x64Texture {
			get {
				if (_bnoise64x64Texture == null) 
                    _bnoise64x64Texture = Resources.Load<Texture2D>("BNoise64");
				return _bnoise64x64Texture;
			}
		}
        
        public static Texture2D DitherTexture (DitherType type) {
            
            switch (type) {
                case DitherType.Bayer2x2: return bayer2x2Texture;
                case DitherType.Bayer3x3: return bayer3x3Texture;
                case DitherType.Bayer4x4: return bayer4x4Texture;
                case DitherType.Bayer8x8: return bayer8x8Texture;
                default: return bnoise64x64Texture;
            }
        }


        static Texture2D _displacementTexture; 
		public static Texture2D displacementTexture {
			get {
				if (_displacementTexture == null) 
                    _displacementTexture = Resources.Load<Texture2D>("Displacements");
				return _displacementTexture;
			}
		}
        
        static Texture3D _noiseTexture3d;
        public static Texture3D noiseTexture3d {
            get {
                if (_noiseTexture3d == null)
                    _noiseTexture3d = LoadNoise3dTexture();
                return _noiseTexture3d;
            }
        }
        static Texture3D LoadNoise3dTexture()
        {
            // basic dds loader for 3d texture - !not very robust!

            TextAsset data = Resources.Load("NoiseVolume") as TextAsset;

            byte[] bytes = data.bytes;

            uint height = BitConverter.ToUInt32(data.bytes, 12);
            uint width = BitConverter.ToUInt32(data.bytes, 16);
            uint pitch = BitConverter.ToUInt32(data.bytes, 20);
            uint depth = BitConverter.ToUInt32(data.bytes, 24);
            uint formatFlags = BitConverter.ToUInt32(data.bytes, 20 * 4);
            uint bitdepth = BitConverter.ToUInt32(data.bytes, 22 * 4);
            if (bitdepth == 0)
                bitdepth = pitch / width * 8;

            Texture3D _noiseTexture = new Texture3D((int)width, (int)height, (int)depth, TextureFormat.Alpha8, false);

            _noiseTexture.filterMode = FilterMode.Bilinear;
            _noiseTexture.wrapMode = TextureWrapMode.Repeat;
            
            _noiseTexture.name = "3D Noise";

            Color[] c = new Color[width * height * depth];

            uint index = 128;
            if (data.bytes[21 * 4] == 'D' && data.bytes[21 * 4 + 1] == 'X' && data.bytes[21 * 4 + 2] == '1' && data.bytes[21 * 4 + 3] == '0' &&
                (formatFlags & 0x4) != 0)
            {
                uint format = BitConverter.ToUInt32(data.bytes, (int)index);
                if (format >= 60 && format <= 65)
                    bitdepth = 8;
                else if (format >= 48 && format <= 52)
                    bitdepth = 16;
                else if (format >= 27 && format <= 32)
                    bitdepth = 32;

                index += 20;
            }

            uint byteDepth = bitdepth / 8;
            pitch = (width * bitdepth + 7) / 8;

            for (int d = 0; d < depth; ++d)
            {
                //index = 128;
                for (int h = 0; h < height; ++h)
                {
                    for (int w = 0; w < width; ++w)
                    {
                        float v = (bytes[index + w * byteDepth] / 255.0f);
                        c[w + h * width + d * width * height] = new Color(v, v, v, v);
                    }

                    index += pitch;
                }
            }

            _noiseTexture.SetPixels(c);
            _noiseTexture.Apply();
            return _noiseTexture;
        }
    }
}