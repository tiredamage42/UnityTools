using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace UnityTools.MeshTools {

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode] public class CableRenderer : MonoBehaviour {

        [HideInInspector] public Vector3 b;
        [Range(1, 20)] public int lengthRes = 20;
        [Range(3, 8)] public int radiusRes = 6;
        public float curvature = 1;
        [Range(0, 10)] public float radius = 0.2f;

        public bool drawEditorLines = true;
        MeshFilter _meshFilter;
        MeshFilter meshFilter { get { return this.GetComponentIfNull<MeshFilter>(ref _meshFilter, false); } }

        void OnEnable() {
            RegenerateMesh();
        }

        public float CurveHeight(int i)
        {
            return (Mathf.Pow((((float)Mathf.Clamp(i, 0, lengthRes) / lengthRes) * 2) - 1, 2) - 1) * curvature;
        }

        public Vector3 PointPosition(Vector3 seg, int i)
        {
            return seg * i + new Vector3(0, CurveHeight(i), 0);
        }

        public List<Vector3> VerticesForPoint(float angleStep, Vector3 crossB, Vector3 pp, Vector3 seg, int i)
        {
            Quaternion rotation = Quaternion.LookRotation(PointPosition(seg, Mathf.Min(i + 1, lengthRes)) - PointPosition(seg, Mathf.Max(0, i - 1)), crossB);
            
            List<Vector3> vertices = new List<Vector3>();
            for(int h = 0; h < radiusRes; h++) {
                float angle = angleStep * h;
                vertices.Add(pp + rotation * new Vector3( Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0 ));
            }

            return vertices;
        }

        public Mesh RegenerateMesh()
        {
            if (meshFilter.sharedMesh == null) {
                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.name = "Cable Mesh";
            }
            Mesh mesh = meshFilter.sharedMesh;
            mesh.Clear();
        
            List<Vector3> vertices = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            float length = 0;

            Vector3 seg = b / lengthRes;
            float angleStep = (360f / (radiusRes - 1)) * Mathf.Deg2Rad;
            Vector3 crossB = Vector3.Cross(Vector3.down, b);

            
            for (int i = 0; i <= lengthRes; i++)
            {

                Vector3 pp = PointPosition(seg, i);

                List<Vector3> vertsPoint = VerticesForPoint(angleStep, crossB, pp, seg, i);
                for (int h = 0; h < vertsPoint.Count; h++)
                {
                    vertices.Add(vertsPoint[h]);
                    normals.Add((vertsPoint[h] - pp).normalized);
                    uvs.Add(new Vector2(length, (float)h / (vertsPoint.Count-1)));

                    if (i < lengthRes)
                    {
                        int index = h + (i * radiusRes);
                        tris.Add(index);
                        tris.Add(index + 1);
                        tris.Add(index + radiusRes);
                        tris.Add(index);
                        tris.Add(index + radiusRes);
                        tris.Add(index + radiusRes - 1);
                    }
                }
                length += SegmentLenght(seg, i, i + 1);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(tris, 0, calculateBounds: true);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateTangents();
            return mesh;
        }

        public float SegmentLenght(Vector3 segment, int a,int b)
        {
            return (PointPosition(segment, b) - PointPosition(segment, a)).magnitude;
        }
    }


#if UNITY_EDITOR

[CustomEditor(typeof(CableRenderer))]
public class CableRendererEditor : Editor {

    CableRenderer cable;

    public void OnEnable()
    {
        cable = (CableRenderer)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("drawEditorLines"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("curvature"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lengthRes"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("radiusRes"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"), true);

        if (EditorGUI.EndChangeCheck())
            cable.RegenerateMesh();

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        Matrix4x4 m = Handles.matrix;
        Handles.matrix = cable.transform.localToWorldMatrix;

        EditorGUI.BeginChangeCheck();
        Vector3 newBposition = Handles.DoPositionHandle(cable.b, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            cable.b = newBposition;
            cable.RegenerateMesh();
            EditorUtility.SetDirty(cable);
        }

        if (cable.drawEditorLines)
        {
            float angleStep = (360f / (cable.radiusRes - 1)) * Mathf.Deg2Rad;
            Vector3 crossB = Vector3.Cross(Vector3.down, cable.b);
            Vector3 seg = cable.b / cable.lengthRes;
            for (int i = 0; i <= cable.lengthRes; i++)
            {
                Vector3 pp = cable.PointPosition(seg, i);
                Handles.DrawLine(pp, cable.PointPosition(seg, i + 1));
                
                List<Vector3> verts = cable.VerticesForPoint(angleStep, crossB, pp, seg, i);
                for (int h = 0; h < verts.Count - 1; h++)
                    Handles.DrawLine(verts[h], verts[h + 1]);

                Handles.DrawLine(verts[cable.radiusRes-1], verts[0]);
                    
            }
            Handles.DrawPolyLine(cable.VerticesForPoint(angleStep, crossB, cable.PointPosition(seg, cable.lengthRes), seg, cable.lengthRes).ToArray());            
        }
        Handles.matrix = m;
    }


}
#endif

}
