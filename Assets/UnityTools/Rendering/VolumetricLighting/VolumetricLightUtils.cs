using UnityEngine;
using System.Collections.Generic;
namespace UnityTools.Rendering.VolumetricLighting {
    public static class VolumetricLightUtils 
    {

        static Dictionary<LightType, Material> ltype2Material = new Dictionary<LightType, Material>();
        public static Material GetMaterial (LightType type) {
            if (!ltype2Material.TryGetValue(type, out Material material)) {
                if (type == LightType.Directional) 
                    material = new Material(Shader.Find("Hidden/VolumetricLightDirectional"));
                else if (type == LightType.Point) 
                    material = new Material(Shader.Find("Hidden/VolumetricLightPoint"));
                else if (type == LightType.Spot) 
                    material = new Material(Shader.Find("Hidden/VolumetricLightSpot"));
                
                // material.enableInstancing = true;
                material.hideFlags = HideFlags.HideAndDontSave;
                ltype2Material[type] = material;
            }
            return material;
        }

        static Texture2D _ditherTexture;
        public static Texture2D ditherTexture {
            get {
                if (_ditherTexture == null)
                    _ditherTexture = GenerateDitherTexture();
                return _ditherTexture;
            }
        }

        static readonly float[] ditherParams = new float[] {
            1.0f  , 49.0f , 13.0f , 61.0f , 4.0f  , 52.0f , 16.0f , 64.0f ,
            33.0f , 17.0f , 45.0f , 29.0f , 36.0f , 20.0f , 48.0f , 32.0f ,
            9.0f  , 57.0f , 5.0f  , 53.0f , 12.0f , 60.0f , 8.0f  , 56.0f ,
            41.0f , 25.0f , 37.0f , 21.0f , 44.0f , 28.0f , 40.0f , 24.0f ,
            3.0f  , 51.0f , 15.0f , 63.0f , 2.0f  , 50.0f , 14.0f , 62.0f ,
            35.0f , 19.0f , 47.0f , 31.0f , 34.0f , 18.0f , 46.0f , 30.0f ,
            11.0f , 59.0f , 7.0f  , 55.0f , 10.0f , 58.0f , 6.0f  , 54.0f ,
            43.0f , 27.0f , 39.0f , 23.0f , 42.0f , 26.0f , 38.0f , 22.0f ,
        };



        static Texture2D GenerateDitherTexture()
        {
            int size = 8;
            int size2 = size * size;

            Texture2D _ditheringTexture = new Texture2D(size, size, TextureFormat.Alpha8, false, true);
            _ditheringTexture.filterMode = FilterMode.Point;
            
            Color32[] c = new Color32[size2];
            for (int i = 0; i < size2; i++) {
                byte b = (byte)(ditherParams[i] / 65.0f * 255); 
                c[i] = new Color32(b, b, b, b);
            }
            _ditheringTexture.SetPixels32(c);
            _ditheringTexture.Apply();
            return _ditheringTexture;
        }
        
        static Mesh _spotlightMesh;
        public static Mesh spotlightMesh {
            get {
                if (_spotlightMesh == null)
                    _spotlightMesh = CreateSpotLightMesh();
                return _spotlightMesh;
            }
        }
        static Mesh CreateSpotLightMesh()
        {
            // copy & pasted from other project, the geometry is too complex, should be simplified
            Mesh mesh = new Mesh();

            const int segmentCount = 16;
            Vector3[] vertices = new Vector3[2 + segmentCount * 3];
            Color32[] colors = new Color32[2 + segmentCount * 3];

            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(0, 0, 1);

            float angle = 0;
            float step = Mathf.PI * 2.0f / segmentCount;
            float ratio = 0.9f;

            for (int i = 0; i < segmentCount; ++i)
            {
                vertices[i + 2] = new Vector3(-Mathf.Cos(angle) * ratio, Mathf.Sin(angle) * ratio, ratio);
                colors[i + 2] = new Color32(255, 255, 255, 255);
                vertices[i + 2 + segmentCount] = new Vector3(-Mathf.Cos(angle), Mathf.Sin(angle), 1);
                colors[i + 2 + segmentCount] = new Color32(255, 255, 255, 0);
                vertices[i + 2 + segmentCount * 2] = new Vector3(-Mathf.Cos(angle) * ratio, Mathf.Sin(angle) * ratio, 1);
                colors[i + 2 + segmentCount * 2] = new Color32(255, 255, 255, 255);
                angle += step;
            }

            mesh.vertices = vertices;
            mesh.colors32 = colors;

            int[] indices = new int[segmentCount * 3 * 2 + segmentCount * 6 * 2];
            int index = 0;

            for (int i = 2; i < segmentCount + 1; ++i)
            {
                indices[index++] = 0;
                indices[index++] = i;
                indices[index++] = i + 1;
            }

            indices[index++] = 0;
            indices[index++] = segmentCount + 1;
            indices[index++] = 2;

            for (int i = 2; i < segmentCount + 1; ++i)
            {
                indices[index++] = i;
                indices[index++] = i + segmentCount;
                indices[index++] = i + 1;

                indices[index++] = i + 1;
                indices[index++] = i + segmentCount;
                indices[index++] = i + segmentCount + 1;
            }

            indices[index++] = 2;
            indices[index++] = 1 + segmentCount;
            indices[index++] = 2 + segmentCount;

            indices[index++] = 2 + segmentCount;
            indices[index++] = 1 + segmentCount;
            indices[index++] = 1 + segmentCount + segmentCount;

            //------------
            for (int i = 2 + segmentCount; i < segmentCount + 1 + segmentCount; ++i)
            {
                indices[index++] = i;
                indices[index++] = i + segmentCount;
                indices[index++] = i + 1;

                indices[index++] = i + 1;
                indices[index++] = i + segmentCount;
                indices[index++] = i + segmentCount + 1;
            }

            indices[index++] = 2 + segmentCount;
            indices[index++] = 1 + segmentCount * 2;
            indices[index++] = 2 + segmentCount * 2;

            indices[index++] = 2 + segmentCount * 2;
            indices[index++] = 1 + segmentCount * 2;
            indices[index++] = 1 + segmentCount * 3;

            ////-------------------------------------
            for (int i = 2 + segmentCount * 2; i < segmentCount * 3 + 1; ++i)
            {
                indices[index++] = 1;
                indices[index++] = i + 1;
                indices[index++] = i;
            }

            indices[index++] = 1;
            indices[index++] = 2 + segmentCount * 2;
            indices[index++] = segmentCount * 3 + 1;

            mesh.triangles = indices;
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}