


using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
namespace UnityTools.EditorTools.Internal {
    
    [System.Serializable] public class PrefabPainter : EditorWindow
    {
        [MenuItem(ProjectTools.defaultMenuItemSection + "Unity Tools/Editor Tools/Prefab Painter", false, ProjectTools.defaultMenuItemPriority)]
		static void OpenWindow () {
            EditorWindowTools.OpenWindowNextToInspector<PrefabPainter>("Prefab Painter");
		}
        void OnEnable () {
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }
        void OnDisable () {
            EndWindow();
        }
        void OnDestroy () {
            EndWindow();
        }

        void EndWindow () {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            DestroyPreview();
        }

        public enum DrawMode { Single, Field, RandomArea };
        
        static DrawMode drawMode;
        static PrefabPainterPrefabValues painterValues = new PrefabPainterPrefabValues();
        static float radius = 10;
        static int amount = 10;
        static bool alignNormal = true;
        
        static void MinMaxField (ref float min, ref float max) {
            EditorGUILayout.BeginHorizontal();
            min = EditorGUILayout.FloatField(min);
            max = EditorGUILayout.FloatField(max);
            EditorGUILayout.EndHorizontal();
        }
        static void DrawOffsetOptions (string label, ref Vector3 min, ref Vector3 max) {
            EditorGUILayout.LabelField(label, GUITools.boldLabel);
            MinMaxField (ref min.x, ref max.x);
            MinMaxField (ref min.y, ref max.y);
            MinMaxField (ref min.z, ref max.z);
        }
        static void DrawOffsetOptions () {
            DrawOffsetOptions ("Position Offset Ranges (xyz):", ref painterValues.posOffsetMin, ref painterValues.posOffsetMax);
            DrawOffsetOptions ("Rotation Offset Ranges (xyz):", ref painterValues.rotOffsetMin, ref painterValues.rotOffsetMax);
            EditorGUILayout.LabelField("Scale Multiplier Range:", GUITools.boldLabel);
            MinMaxField (ref painterValues.scaleMultiplierRange.x, ref painterValues.scaleMultiplierRange.y);
        }

        GameObject objectToPlace;

        bool DrawPrefabSelect () {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            objectToPlace = (GameObject)EditorGUILayout.ObjectField("Object To Place", objectToPlace, typeof(GameObject), true);
            if (GUITools.IconButton(new GUIContent("X"), GUITools.red)) {
                objectToPlace = null;
            }
            EditorGUILayout.EndHorizontal();
            PrefabReferenceDrawer.DrawPrefabReference(prefabInfo, new GUIContent("Prefab Info"), (s) => prefabInfo.collection = s, (s) => prefabInfo.name = s );
            return EditorGUI.EndChangeCheck();
        }


        bool drawing;
        Vector3 previewPos, previewRot, originalPreviewScale;
        float previewSizeMult;

        void UpdatePreviewRandom () {
            painterValues.AdjustTransform(out previewPos, out previewRot, out previewSizeMult);        
        }

        void DestroyPreview () {
            if (preview != null)
                MonoBehaviour.DestroyImmediate(preview.gameObject);
            MonoBehaviour.DestroyImmediate(previewEditor);
        }

        Collider[] previewCols;
        Transform CreateNewPreview (GameObject prefab) {
            Transform preview = (PrefabUtility.InstantiatePrefab(prefab) as GameObject).transform;
            PrefabUtility.UnpackPrefabInstance(preview.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            preview.gameObject.hideFlags = HideFlags.HideAndDontSave;
            previewCols = preview.GetComponentsInChildren<Collider>();
            for (int i = 0; i < previewCols.Length; i++) previewCols[i].enabled = false;
            originalPreviewScale = preview.localScale;
            return preview;
        }

        void OnPrefabChange (GameObject newPrefab) {
            DestroyPreview();
            if (newPrefab == null)
                return;

            PrefabPainterPrefab p = newPrefab.GetComponent<PrefabPainterPrefab>();
            if (p != null) 
                painterValues = new PrefabPainterPrefabValues(p.painterValues);

            preview = CreateNewPreview(newPrefab);
            UpdatePreviewRandom();
        }

        Transform preview;
        void HandlePreview (bool isValid, bool shiftHeld) {
            if (drawMode != DrawMode.Single) {
                previewLocked = false;
                if (preview != null)
                    MonoBehaviour.DestroyImmediate(preview.gameObject);
                return;
            }

            if (preview == null) {
                OnPrefabChange(GetPrefab());
            }

            if ((!isValid || shiftHeld) && !previewLocked) {
                preview.gameObject.SetActive(false);
                return;
            }

            preview.gameObject.SetActive(true);

            preview.position = hitPos;
            preview.localScale = originalPreviewScale * previewSizeMult;
            Vector3 up = alignNormal ? hitNormal : Vector3.up;
            preview.up = up;
            preview.Rotate(previewRot, Space.Self);
            preview.position = PhysicsTools.UnIntersectColliderGroup(previewCols, preview.position, up);
            preview.position += previewPos;

            GUI.enabled = false;
            Handles.PositionHandle (preview.position, preview.rotation);
            GUI.enabled = true;
        }

        bool previewLocked;


        [SerializeField] public PrefabReference prefabInfo;
        GameObject GetPrefab () {
            if (objectToPlace != null)
                return objectToPlace;

            if (string.IsNullOrEmpty(prefabInfo.collection) || string.IsNullOrEmpty(prefabInfo.name))
                return null;

            return PrefabReferenceCollection.GetPrefabReference( prefabInfo );
        }
        
        bool change;
        void OnGUI ()
        {
            
            GUITools.Space(3);            
            bool changedPrefab = DrawPrefabSelect();

            if (changedPrefab) {
                change = true;
                return;
            }
            
            GameObject prefab = GetPrefab();
            if (change) {
                change = false;
                OnPrefabChange( prefab );
            }
            
            DrawOffsetOptions();

            if (GUILayout.Button("Save Offset Options")) {
                if (prefab != null) {
                    if (AssetDatabase.Contains(prefab)) {
                        using (var scope = new EditPrefab(prefab)) {
                            PrefabPainterPrefab p = scope.root.GetComponent<PrefabPainterPrefab>();
                            if (p == null) p = scope.root.AddComponent<PrefabPainterPrefab>();
                            p.painterValues = new PrefabPainterPrefabValues(painterValues);
                        }
                    }
                }
            }
            
            if (drawMode == DrawMode.Field) {
                density = EditorGUILayout.Slider("Density", density, 0, 1);
                frequency = EditorGUILayout.Slider("Frequency", frequency, .25f, radius);
            }
            else if (drawMode == DrawMode.RandomArea) {
                amount = EditorGUILayout.IntField("Draw Amount", amount);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            if (GUITools.Button(new GUIContent((drawing ? "Exit " : "") + "Draw Mode"), drawing ? GUITools.red : GUITools.white, GUITools.button, GUITools.black))
                drawing = !drawing;
            
            if (prefab != null) {
                if (previewEditor == null) 
                    previewEditor = UnityEditor.Editor.CreateEditor(prefab);
                previewEditor.OnPreviewGUI(GUILayoutUtility.GetRect (position.width, 200), EditorStyles.helpBox); 
            }
        }
        static float density = .25f;
        static float frequency = 1;

        UnityEditor.Editor previewEditor;
        const int sceneViewBoxSize = 256;
        const float mosuePosOffset = 8;

        static readonly KeyCode switchDrawModeKey = KeyCode.Tab;
        static readonly KeyCode exitDrawKey = KeyCode.Escape;
        static readonly KeyCode focusKey = KeyCode.F;
        static readonly KeyCode lockPreviewKey = KeyCode.L;
        static readonly KeyCode alignNormalKey = KeyCode.W;
        static readonly KeyCode randomizePreviewKey = KeyCode.Space;

        const string hint0 = "Left Click: Draw\nShift+Click: Delete\n{0}: Switch Draw Mode ({1})\n{2}: Focus area\n{3}: Align With Normal ({4})\n{5}: Exit Draw\n";
        const string hint1 = "Cmd+Scroll: Adjust radius ({0:0.00})\n";
        const string hint2 = "{0}: Lock preview ({1})\n{2}: Randomize Preview\n";
        string GetHint () {
            string helpString = string.Format(hint0, switchDrawModeKey, drawMode, focusKey, alignNormalKey, alignNormal, exitDrawKey);
            if (drawMode == DrawMode.Field || drawMode == DrawMode.RandomArea) 
                helpString += string.Format(hint1, radius);
            else 
                helpString += string.Format(hint2, lockPreviewKey, previewLocked, randomizePreviewKey);
            return helpString;
        }

        void SwitchDrawMode () {
            drawMode = (DrawMode)((((int)drawMode) + 1) % 3);
        }
        void OnSceneGUI( SceneView sv )
        {
            if (!drawing) {
                if (preview != null)
                    MonoBehaviour.DestroyImmediate(preview.gameObject);
            
                previewLocked = false;
                return;
            }

            if (!InternalEditorUtility.isApplicationActive)
                return;

            GameObject prefab = GetPrefab();
            if (prefab == null)
                return;

            Event e = Event.current;
            
            Handles.BeginGUI();
            GUI.Label(new Rect(e.mousePosition.x + mosuePosOffset, (e.mousePosition.y + mosuePosOffset) - 75, sceneViewBoxSize, sceneViewBoxSize), GetHint());
            Handles.EndGUI();


            EditorWindowTools.FocusSceneViewOnMouseOver();
            
            
            Event current = Event.current;
            if (EventType.KeyUp == current.type) {
                bool earlyOut = false;
                if(exitDrawKey == current.keyCode) {
                    drawing = false;
                    earlyOut = true;
                    MonoBehaviour.DestroyImmediate(preview.gameObject);
                }
                if(lockPreviewKey == current.keyCode) {
                    earlyOut = true;
                    previewLocked = !previewLocked;
                }
                if(switchDrawModeKey == current.keyCode) {
                    earlyOut = true;
                    SwitchDrawMode();
                }
                if(alignNormalKey == current.keyCode) {
                    earlyOut = true;
                    alignNormal = !alignNormal;
                }
                if(randomizePreviewKey == current.keyCode) {
                    earlyOut = true;
                    UpdatePreviewRandom();
                }
                if (earlyOut) {
                    current.Use();
                    Repaint();
                    return;
                }
            }

            bool focus = false;
            if((EventType.KeyUp == current.type ||  EventType.KeyDown == current.type ) && focusKey == current.keyCode) {
                focus = EventType.KeyUp == current.type;
                current.Use();
            }
            
            if (EventType.ScrollWheel == current.type && current.command) {
                radius += current.delta.y;
                current.Use();
            }
            
            int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            Collider hitCollider;
            bool isValid = CalculateMousePosition(out hitCollider);
            if (isValid){
                if (drawMode == DrawMode.Field || drawMode == DrawMode.RandomArea) {
                    Handles.color = current.shift ? GUITools.red : GUITools.white;
                    if (drawMode == DrawMode.Field) {
                        Matrix4x4 mat = Handles.matrix;
                        Handles.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(Vector3.up, hitNormal), Vector3.one);
                        Handles.DrawWireCube(hitPos, new Vector3(radius * 2, 1, radius * 2));
                        Handles.matrix = mat;
                    }
                    else if (drawMode == DrawMode.RandomArea){
                        Handles.DrawWireDisc(hitPos, hitNormal, radius);
                    }
                }

                if (focus)
                    SceneView.lastActiveSceneView.Frame(new Bounds(hitPos, Vector3.one * radius), false);
                
                if ((current.type == EventType.MouseDown) && !current.alt)
                {
                    if (current.button == 0) {
                        if (!current.shift)
                            PrefabInstantiate(prefab);
                        else
                            PrefabRemove(hitCollider);
                    }
                }
            }
            
            HandlePreview(isValid, current.shift);
            
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(controlId);

            SceneView.RepaintAll();
        }


        Vector3 hitPos, hitNormal;
        public bool CalculateMousePosition (out Collider hitCollider)
        {
            hitCollider = null;

            if (previewLocked && drawMode == DrawMode.Single)
                return true;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, PhysicsTools.environmentMask))
            {
                hitPos = hit.point;
                hitNormal = hit.normal;
                hitCollider = hit.collider;
                return true;
            }
            return false;
        }
        void PrefabInstantiate (GameObject prefab)
        {
            if (drawMode == DrawMode.Single) {
                Undo.RegisterCreatedObjectUndo(InstantiateAndAdjustPrefab(prefab, hitPos, hitNormal, previewPos, previewRot, previewSizeMult), "Instantiate");
                UpdatePreviewRandom();
            }
            else {
                int undoID = -1;
                int j = 0;
                if (drawMode == DrawMode.RandomArea) {
                    for (int i = 0; i < amount; i++) {
                        Vector2 rand = Random.insideUnitCircle;
                        FinishSpawnAtPosition (prefab, hitPos + Vector3.ProjectOnPlane(new Vector3(rand.x, 0, rand.y) * radius, hitNormal), ref j, ref undoID);
                    }
                }
                else {
                    for (float z = -radius; z <= radius; z+=frequency) {
                        for (float x = -radius; x <= radius; x+=frequency) {
                            if (Random.value > density) 
                                continue;            
                            FinishSpawnAtPosition (prefab, hitPos + Vector3.ProjectOnPlane(new Vector3(x,0,z), hitNormal), ref j, ref undoID);
                        }    
                    }
                }
            }
        }
        
        void FinishSpawnAtPosition (GameObject prefab, Vector3 spawnPos, ref int i, ref int undoID) {
            RaycastHit hit;
            if (!Physics.Raycast(spawnPos + hitNormal * 5, -hitNormal, out hit, 50, PhysicsTools.environmentMask))
                return;
            
            Vector3 pos, rot;
            float scaleMult;
            painterValues.AdjustTransform(out pos, out rot, out scaleMult);
            
            Undo.RegisterCreatedObjectUndo(InstantiateAndAdjustPrefab(prefab, hit.point, hit.normal, pos, rot, scaleMult), "New Undo");
            if (i == 0) {
                undoID = Undo.GetCurrentGroup();
                i++;
            }
            else {
                Undo.CollapseUndoOperations(undoID);
            }
        }
       
        GameObject InstantiateAndAdjustPrefab (GameObject prefab, Vector3 hitPos, Vector3 hitNormal, Vector3 position, Vector3 rotation, float sizeMultiplier) {
            Transform instance = (PrefabUtility.InstantiatePrefab(prefab) as GameObject).transform;
            Vector3 up = alignNormal ? hitNormal : Vector3.up;
            instance.position = hitPos;
            instance.up = up;
            instance.Rotate(rotation, Space.Self);
            instance.localScale *= sizeMultiplier;
            instance.position = PhysicsTools.UnIntersectColliderGroup(instance.GetComponentsInChildren<Collider>(), instance.position, up);
            instance.position += position;
            instance.gameObject.AddComponent<PrefabPainted>();
            return instance.gameObject;
        }

        void PrefabRemove ( Collider hitCollider)
        {
            if (drawMode == DrawMode.Field || drawMode == DrawMode.RandomArea) {
                float thresh = radius * radius;
                PrefabPainted[] prefabsInRadius = GameObject.FindObjectsOfType<PrefabPainted> ();
                foreach (var p in prefabsInRadius) {
                    if (Vector3.SqrMagnitude(hitPos - p.transform.position) <= thresh)
                        Undo.DestroyObjectImmediate(p.gameObject);
                }
            }
            else {
                PrefabPainted p = hitCollider.GetComponent<PrefabPainted>();
                if (p != null) {
                    Undo.DestroyObjectImmediate(p.gameObject);
                }
            }
        }
    }
}