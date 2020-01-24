


using UnityEngine;
using UnityEditor;

using UnityTools.Rendering;
namespace UnityTools.MeshTools {
    
    [ExecuteInEditMode]
    [RequireComponent (typeof(MeshFilter))]
    
    public class MeshVisualizer : MonoBehaviour { 
        public enum MeshVisualizerType { 
            Cone, Cube, Cylinder, Frustum, Plane, Sphere, IcoSphere, Torus, Tube
        }

        public MeshVisualizerType type;

        [SerializeField] float ico_radius = 1f;
        [SerializeField] int ico_recursionLevel = 3;

        [SerializeField] float torus_radius = 1f;
        [SerializeField] float torus_radius2 = .3f;
        [SerializeField] int torus_segments = 24;
        [SerializeField] int torus_sides = 18;


        [SerializeField] int tube_sides = 24;
        [SerializeField] float tube_height = 1f;
        [SerializeField] float tube_bottom_radius1 = .5f;
        [SerializeField] float tube_bottom_radius2 = .15f;
        [SerializeField] float tube_top_radius1 = .5f;
        [SerializeField] float tube_top_radius2 = .15f;

        [SerializeField, Range(5, 20)] int cone_subdivision = 10;
        [SerializeField, Range(0.5f, 10f)] float cone_radius = 1f;
        [SerializeField, Range(0.5f, 10f)] float cone_height = 1f;

        [SerializeField] Vector3 cubeSize = Vector3.one;
        [SerializeField] Vector3Int cubeResolutions = new Vector3Int(2,2,2);

        [SerializeField] float cylinder_radius_bottom = 1f;
        [SerializeField] float cylinder_radius_top = 1f;
        [SerializeField] float cylinder_height = 4f;
        [SerializeField] int cylinder_segments = 8;

        [SerializeField, Range(0.1f, 1f)] float frustum_nearClip = 0.1f;
        [SerializeField, Range(1f, 5f)] float frustum_farClip = 1f;
        [SerializeField, Range(45f, 90f)] float frustum_fieldOfView = 60f;
        [SerializeField, Range(0f, 1f)] float frustum_aspectRatio = 1f;

        public enum PlaneType { Default, Noise };

        [SerializeField] PlaneType plane_type = PlaneType.Default;
        [SerializeField, Range(0.5f, 10f)] float plane_width = 1f;
        [SerializeField, Range(0.5f, 10f)] float plane_height = 1f;
        [SerializeField, Range(2, 40)] int plane_wSegments = 2;
        [SerializeField, Range(2, 40)] int plane_hSegments = 2;

        [SerializeField, Range(0.5f, 10f)] float sphere_radius = 1f;
        [SerializeField, Range(8, 20)] int sphere_lonSegments = 10;
        [SerializeField, Range(8, 20)] int sphere_latSegments = 10;

        
        Mesh BuildMesh () {
            switch (type) {
                case MeshVisualizerType.Cone:
                    return Primitives.BuildCone(cone_subdivision, cone_radius, cone_height);
                case MeshVisualizerType.Cube:
                    return Primitives.BuildCube(cubeSize, cubeResolutions);
                case MeshVisualizerType.Cylinder:
                    return Primitives.BuildCylinder(cylinder_height, cylinder_radius_bottom, cylinder_radius_top, cylinder_segments);
                case MeshVisualizerType.Frustum:
                    return Primitives.BuildFrustum(Vector3.forward, Vector3.up, frustum_nearClip, frustum_farClip, frustum_fieldOfView, frustum_aspectRatio);
                case MeshVisualizerType.Plane:
                    switch(plane_type) {
                        case PlaneType.Noise:
                            return Primitives.BuildPlane(new ParametricPlanePerlin(Vector2.zero, new Vector2(2f, 2f), 1), plane_width, plane_height, plane_wSegments, plane_hSegments);
                        default:
                            return Primitives.BuildPlane(plane_width, plane_height, plane_wSegments, plane_hSegments);
                    }
                case MeshVisualizerType.Sphere:
                    return Primitives.BuildSphere(sphere_radius, sphere_lonSegments, sphere_latSegments);
                case MeshVisualizerType.IcoSphere:
                    return Primitives.CreateIcoSphere(ico_radius, ico_recursionLevel);
                case MeshVisualizerType.Torus:
                    return Primitives.Torus (torus_radius, torus_radius2, torus_segments, torus_sides);
                case MeshVisualizerType.Tube:
                    return Primitives.BuildTube (tube_height, tube_bottom_radius1, tube_bottom_radius2, tube_top_radius1, tube_top_radius2, tube_sides);

              
                

            }
            return null;
        }
        MeshFilter _meshFilter;
        public MeshFilter meshFilter {
            get {
                if (_meshFilter == null)
                    _meshFilter = GetComponent<MeshFilter>();
                return _meshFilter;
            }
        }
        public void CreateMesh () {
            meshFilter.sharedMesh = BuildMesh();
        }
        Material _material;
        Material material { get { return RenderUtils.CreateMaterialIfNull("Hidden/DisplayMesh", ref _material); } }
        public DrawType drawType;
        public enum DrawType { UVs, Normals }
        
        void Update () {
            DrawMesh(meshFilter.sharedMesh);
        }

        void DrawMesh (Mesh mesh) {
            if (mesh == null) 
                return;

            if (drawType == DrawType.UVs)
                material.EnableKeyword("UVS");
            else  
                material.DisableKeyword("UVS");

            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, 0, null, 0, null, false, false, false);
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(MeshVisualizer))]
    public class MeshVisualizerInspector : Editor
    {
        new MeshVisualizer target;
        void OnEnable()
        {
            target = base.target as MeshVisualizer;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));

            switch (target.type) {
                case MeshVisualizer.MeshVisualizerType.Cone:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cone_subdivision"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cone_radius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cone_height"));
                    break;
                case MeshVisualizer.MeshVisualizerType.Cube:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cubeSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cubeResolutions"));
                    break;
                case MeshVisualizer.MeshVisualizerType.Cylinder:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cylinder_radius_bottom"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cylinder_radius_top"));
                    
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cylinder_height"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cylinder_segments"));
                    break;
                case MeshVisualizer.MeshVisualizerType.Frustum:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("frustum_nearClip"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("frustum_farClip"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("frustum_fieldOfView"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("frustum_aspectRatio"));
                    break;
                case MeshVisualizer.MeshVisualizerType.Plane:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("plane_type"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("plane_width"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("plane_height"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("plane_wSegments"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("plane_hSegments"));
                    break;
                case MeshVisualizer.MeshVisualizerType.Sphere:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sphere_radius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sphere_lonSegments"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sphere_latSegments"));
                    break;
                case MeshVisualizer.MeshVisualizerType.IcoSphere:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ico_radius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ico_recursionLevel"));
                    break;
                case MeshVisualizer.MeshVisualizerType.Torus:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("torus_radius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("torus_radius2"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("torus_segments"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("torus_sides"));
                    break;
                case MeshVisualizer.MeshVisualizerType.Tube:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tube_sides"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tube_height"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tube_bottom_radius1"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tube_bottom_radius2"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tube_top_radius1"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tube_top_radius2"));
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("drawType"));
            
            serializedObject.ApplyModifiedProperties();
            
            Mesh mesh = target.meshFilter.sharedMesh;
            if (mesh != null) {
                EditorGUILayout.HelpBox("Verts:\n" + mesh.vertexCount, MessageType.Info);
            }
            if (GUILayout.Button("Create Mesh")) {
                target.CreateMesh();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Save Mesh")) {
                if (mesh == null)
                    return;

                var path = EditorUtility.SaveFilePanelInProject("Save Mesh", "", mesh.name + ".asset", "mesh");
                if (path.Length != 0)
                {
                    AssetDatabase.CreateAsset(mesh, path);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
    #endif
}