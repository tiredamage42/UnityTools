using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace UnityTools.MeshTools {
    public class OBJExport : EditorWindow
    {
        private static float autoCutMinX = 1000;
        private static float autoCutMaxX = 0;
        private static float autoCutMinY = 1000;
        private static float autoCutMaxY = 0;

        private static float cutMinX = 0;
        private static float cutMaxX = 0;
        private static float cutMinY = 0;
        private static float cutMaxY = 0;

        private static long startTime = 0;
        private static int totalCount = 0;
        private static int count = 0;
        private static int counter = 0;
        private static int progressUpdateInterval = 10000;

        [MenuItem("ExportScene/ExportSceneToObj")]
        [MenuItem("GameObject/ExportScene/ExportSceneToObj")]
        public static void Export()
        {
            ExportSceneToObj();
        }

        [MenuItem("ExportScene/ExportSelectedObj")]
        [MenuItem("GameObject/ExportScene/ExportSelectedObj", priority = 44)]
        public static void ExportObj()
        {
            ExportObj(Selection.activeGameObject);
        }

        public static void ExportObj (GameObject root) {
            if (root == null)
                return;
            string path = GetSavePath(root);
            if (string.IsNullOrEmpty(path)) 
                return;

            Terrain[] terrains = root.GetComponentsInChildren<Terrain>();
            MeshFilter[] mfs = root.GetComponentsInChildren<MeshFilter>();
            SkinnedMeshRenderer[] smrs = root.GetComponentsInChildren<SkinnedMeshRenderer>();
            ExportSceneToObj(path, terrains, mfs, smrs, false);
        }

        public static void ExportSceneToObj()
        {
            string path = GetSavePath(null);
            if (string.IsNullOrEmpty(path)) 
                return;

            Terrain[] terrains = UnityEngine.Object.FindObjectsOfType<Terrain>();
            MeshFilter[] mfs = UnityEngine.Object.FindObjectsOfType<MeshFilter>();
            SkinnedMeshRenderer[] smrs = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
            ExportSceneToObj(path, terrains, mfs, smrs, true);
        }

        public static void ExportSceneToObj(string path, Terrain[] terrains, MeshFilter[] mfs, SkinnedMeshRenderer[] smrs, bool needCheckRect)
        {
            Debug.Log("RENDERERS: " + mfs.Length + ", SKINNED RENDERERS: " + smrs.Length + ", TERRAINS: " + terrains.Length);
        
            int vOffset = 0;
            string title = "export GameObject to .obj ...";
            StreamWriter writer = new StreamWriter(path);

            startTime = GetMsTime();
            UpdateCutRect();
            counter = count = 0;
            progressUpdateInterval = 5;
            totalCount = (mfs.Length + smrs.Length) / progressUpdateInterval;
            foreach (var mf in mfs) {
                UpdateProgress(title);
                if (mf.GetComponent<Renderer>() != null && (!needCheckRect || (needCheckRect && IsInCutRect(mf.transform))))
                    ExportMeshToObj(mf.transform, mf.sharedMesh, ref writer, ref vOffset);   
            }
            foreach (var smr in smrs) {
                UpdateProgress(title);
                if (!needCheckRect || (needCheckRect && IsInCutRect(smr.transform)))
                    ExportMeshToObj(smr.transform, smr.sharedMesh, ref writer, ref vOffset);
            }
            foreach (var t in terrains)
                ExportTerrianToObj(t.terrainData, t.GetPosition(), ref writer, ref vOffset);//, autoCut);
            
            writer.Close();
            EditorUtility.ClearProgressBar();

            Debug.Log("Export SUCCESS:" + path);
            Debug.Log("Export Time:" + ((float)(GetMsTime() - startTime) / 1000) + "s");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)));
        }

        
        static string GetSavePath(GameObject obj) {
            return EditorUtility.SaveFilePanelInProject("Export .obj file", Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")), obj == null ? SceneManager.GetActiveScene().name : obj.name, "obj");
        }
        static long GetMsTime() {
            return System.DateTime.Now.Ticks / 10000;
        }
        static void UpdateCutRect() {
            cutMinX = cutMaxX = cutMinY = cutMaxY = 0;
        }
        static void UpdateAutoCutRect(Vector3 v) {
            if (v.x < autoCutMinX) autoCutMinX = v.x;
            if (v.x > autoCutMaxX) autoCutMaxX = v.x;
            if (v.z < autoCutMinY) autoCutMinY = v.z;
            if (v.z > autoCutMaxY) autoCutMaxY = v.z;
        }
        static bool IsInCutRect(Transform obj) {
            if (cutMinX == 0 && cutMaxX == 0 && cutMinY == 0 && cutMaxY == 0) 
                return true;
            Vector3 pos = obj.position;
            return pos.x >= cutMinX && pos.x <= cutMaxX && pos.z >= cutMinY && pos.z <= cutMaxY;
        }

        static void ExportMeshToObj(Transform obj, Mesh mesh, ref StreamWriter writer, ref int vOffset) {
            Quaternion r = obj.localRotation;
            StringBuilder sb = new StringBuilder();
            foreach (Vector3 vertice in mesh.vertices)
            {
                Vector3 v = obj.TransformPoint(vertice);
                UpdateAutoCutRect(v);
                sb.AppendFormat("v {0} {1} {2}\n", -v.x, v.y, v.z);
            }
            foreach (Vector3 nn in mesh.normals)
            {
                Vector3 v = r * nn;
                sb.AppendFormat("vn {0} {1} {2}\n", -v.x, -v.y, v.z);
            }
            
            foreach (Vector3 v in mesh.uv)
                sb.AppendFormat("vt {0} {1}\n", v.x, v.y);
            
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] tris = mesh.GetTriangles(i);
                for (int j = 0; j < tris.Length; j += 3)
                    sb.AppendFormat("f {1} {0} {2}\n", tris[j] + 1 + vOffset, tris[j + 1] + 1 + vOffset, tris[j + 2] + 1 + vOffset);
            }
            vOffset += mesh.vertices.Length;
            writer.Write(sb.ToString());
        }
        static void ExportTerrianToObj(TerrainData t, Vector3 tPos, ref StreamWriter writer, ref int vOffset)
        {
            int tw = t.heightmapWidth;
            int th = t.heightmapHeight;

            Vector3 meshScale = t.size;
            meshScale = new Vector3(meshScale.x / (tw - 1), meshScale.y, meshScale.z / (th - 1));
            Vector2 uvScale = new Vector2(1.0f / (tw - 1), 1.0f / (th - 1));

            Vector2 boundL = GetTerrainBoundPos(new Vector3(autoCutMinX, 0, autoCutMinY), t, tPos);
            Vector2 boundR = GetTerrainBoundPos(new Vector3(autoCutMaxX, 0, autoCutMaxY), t, tPos);
            
            int bw = (int)(boundR.x - boundL.x);
            int bh = (int)(boundR.y - boundL.y);

            int w = bh != 0 && bh < th ? bh : th;
            int h = bw != 0 && bw < tw ? bw : tw;

            int startX = (int)boundL.y;
            int startY = (int)boundL.x;
            if (startX < 0) startX = 0;
            if (startY < 0) startY = 0;

            Debug.Log(string.Format("Terrian:tw={0},th={1},sw={2},sh={3},startX={4},startY={5}", tw, th, bw, bh, startX, startY));

            float[,] tData = t.GetHeights(0, 0, tw, th);
            Vector3[] verts = new Vector3[w * h];
            Vector2[] uvs = new Vector2[w * h];

            int[] tPolys = new int[(w - 1) * (h - 1) * 6];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector3 pos = new Vector3(-(startY + y), tData[startX + x, startY + y], (startX + x));
                    verts[y * w + x] = Vector3.Scale(meshScale, pos) + tPos;
                    uvs[y * w + x] = Vector2.Scale(new Vector2(x, y), uvScale);
                }
            }
            int index = 0;
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = (y * w) + x + 1;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
            count = counter = 0;
            progressUpdateInterval = 10000;
            totalCount = (verts.Length + uvs.Length + tPolys.Length / 3) / progressUpdateInterval;
            string title = "export Terrain to .obj ...";
            for (int i = 0; i < verts.Length; i++)
            {
                UpdateProgress(title);
                StringBuilder sb = new StringBuilder(22);
                sb.AppendFormat("v {0} {1} {2}\n", verts[i].x, verts[i].y, verts[i].z);
                writer.Write(sb.ToString());
            }
            for (int i = 0; i < uvs.Length; i++)
            {
                UpdateProgress(title);
                StringBuilder sb = new StringBuilder(20);
                sb.AppendFormat("vt {0} {1}\n", uvs[i].x, uvs[i].y);
                writer.Write(sb.ToString());
            }
            for (int i = 0; i < tPolys.Length; i += 3)
            {
                UpdateProgress(title);
                StringBuilder sb = new StringBuilder(30);
                sb.AppendFormat("f {0} {1} {2}\n", tPolys[i] + 1 + vOffset, tPolys[i + 1] + 1 + vOffset, tPolys[i + 2] + 1 + vOffset);
                writer.Write(sb.ToString());
            }
            vOffset += verts.Length;
        }

        static Vector2 GetTerrainBoundPos(Vector3 wPos, TerrainData t, Vector3 terrainPos) {
            Vector3 tpos = wPos - terrainPos;
            return new Vector2((int)(tpos.x / t.size.x * t.heightmapWidth), (int)(tpos.z / t.size.z * t.heightmapHeight));
        }

        static void UpdateProgress(string title) {
            if (counter++ == progressUpdateInterval) {
                counter = 0;
                EditorUtility.DisplayProgressBar(title, string.Format("{0}/{1}({2:f2} sec.)", count, totalCount, ((float)(GetMsTime() - startTime)) / 1000), Mathf.InverseLerp(0, totalCount, ++count));
            }
        }
    }
}