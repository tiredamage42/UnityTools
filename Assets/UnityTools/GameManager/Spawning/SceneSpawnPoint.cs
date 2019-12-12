using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityTools.EditorTools;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
namespace UnityTools.Spawning.Internal {
    /*
        Editor Representation of the spawn point, used as a template
        for the final spawn points
    */

    #if UNITY_EDITOR 

    [Serializable] public class SubSpawnArray : NeatArrayWrapper<SubSpawn> { }
    [Serializable] public struct SubSpawn {
        public bool isError, hovered, selected;
        public string name;
        public Vector3 position;
        public float rotation;

        public static void InitializeNewSubspawn (SerializedProperty prop, Vector3 position, float rotation) {
            prop.FindPropertyRelative("isError").boolValue = false;
            prop.FindPropertyRelative("hovered").boolValue = false;
            prop.FindPropertyRelative("selected").boolValue = false;
            
            prop.FindPropertyRelative("name").stringValue = null;
            prop.FindPropertyRelative("position").vector3Value = position;
            prop.FindPropertyRelative("rotation").floatValue = rotation;
        }
    }

    [CustomPropertyDrawer(typeof(SubSpawnArray))] class SubSpawnArrayDrawer : PropertyDrawer
    {

        static SerializedProperty rotateProp;
        static bool isRotating;

        static GUIContent _rotateGUI;
        public static GUIContent rotateGUI { get { 
            if (_rotateGUI == null) _rotateGUI = BuiltInIcons.GetIcon("RotateTool", "Rotate"); 
            return _rotateGUI;
        } }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            prop = prop.FindPropertyRelative(NeatArrayAttributeDrawer.listName);

            GUITools.Label(pos, label, GUITools.black, GUITools.boldLabel);
            
            if (GUITools.IconButton(pos.x + pos.width - GUITools.iconButtonWidth, pos.y, new GUIContent("", "Unselect All"), GUITools.red)) {
                for (int i = 0; i < prop.arraySize; i++) 
                    prop.GetArrayElementAtIndex(i).FindPropertyRelative("selected").boolValue = false;
                
            }

            int indexToDelete = -1;

            float fieldsY = pos.y + GUITools.singleLineHeight;
                
            Rect fieldRect = new Rect(
                pos.x + GUITools.iconButtonWidth + GUITools.toolbarDividerSize, 
                fieldsY, 
                pos.width - (GUITools.iconButtonWidth * 3 + GUITools.toolbarDividerSize * 2),
                EditorGUIUtility.singleLineHeight
            );
            Rect colorAreaRect = new Rect(
                pos.x + GUITools.iconButtonWidth, 
                fieldsY, 
                pos.width - (GUITools.iconButtonWidth * 3),
                GUITools.singleLineHeight
            );

            Rect hoverAreaRect = new Rect (pos.x, fieldsY, pos.width, GUITools.singleLineHeight);

            float selectX = pos.x + pos.width - GUITools.iconButtonWidth;
            float rotateX = selectX - GUITools.iconButtonWidth;

            Rect rotateRect = new Rect(rotateX, fieldsY, GUITools.iconButtonWidth, GUITools.singleLineHeight);
            

            bool focus = false;

            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            if (isRotating) {
                if (e.type == EventType.MouseUp) {
                    isRotating = false;
                    rotateProp = null;
                }
                else if (e.type == EventType.MouseDrag) {
                    SerializedProperty rotationProp = rotateProp.FindPropertyRelative("rotation");
                    rotationProp.floatValue = rotationProp.floatValue + e.delta.x;
                }
            }

            else {

                if (!SpawnPainter.drawing)
                {
                    if((EventType.KeyUp == e.type ||  EventType.KeyDown == e.type ) && KeyCode.F == e.keyCode) {
                        focus = EventType.KeyUp == e.type;
                        e.Use();
                    }
                }    
            }

            for (int i = 0; i < prop.arraySize; i++) {
                if (GUITools.IconButton(pos.x, fieldsY, NeatArrayAttributeDrawer.deleteContent, GUITools.red))
                    indexToDelete = i;
                
                SerializedProperty p = prop.GetArrayElementAtIndex(i);
                
                SerializedProperty hovered = p.FindPropertyRelative("hovered");

                if (!isRotating) {
                    hovered.boolValue = false;
                    if (hoverAreaRect.Contains(mousePos)) {
                        hovered.boolValue = true;

                        if (focus)
                            SceneView.lastActiveSceneView.Frame(new Bounds(p.FindPropertyRelative("position").vector3Value, Vector3.one * 3), false);
                
                    }
                }

                if (hovered.boolValue) 
                    GUITools.Box(colorAreaRect, GUITools.blue);
                else {
                    if (p.FindPropertyRelative("isError").boolValue) 
                        GUITools.Box(colorAreaRect, GUITools.red);
                }

                SerializedProperty nameProp = p.FindPropertyRelative("name");
                if (string.IsNullOrEmpty(nameProp.stringValue)) 
                    nameProp.stringValue = "SubSpawn_" + i.ToString();
                EditorGUI.PropertyField(fieldRect, nameProp, GUITools.noContent, true);

                GUITools.DrawToggleButton(p.FindPropertyRelative("selected"), new GUIContent("", "Select For Movement"), selectX, fieldsY);

                if (!isRotating) {
                    if (Event.current.type == EventType.MouseDown) {
            
                        if (rotateRect.Contains(mousePos)) {
                            hovered.boolValue = true;
                            rotateProp = p;
                            isRotating = true;
                        }
                    }
                }
                GUITools.DrawToggleButton(isRotating && hovered.boolValue, rotateGUI, rotateX, fieldsY);
                

                fieldsY += GUITools.singleLineHeight;
                fieldRect.y = fieldsY;
                colorAreaRect.y = fieldsY;
                hoverAreaRect.y = fieldsY;
                rotateRect.y = fieldsY;
            }
            
            if (indexToDelete != -1) {
                prop.DeleteArrayElementAtIndex(indexToDelete);
            }
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return ((prop.FindPropertyRelative(NeatArrayAttributeDrawer.listName).arraySize + 2 + .25f) * GUITools.singleLineHeight);// + GUITools.singleLineHeight * .25f;
        }
    }
    #endif

    public class SceneSpawnPoint : MonoBehaviour {
        #if UNITY_EDITOR 
        public SpawnPointType type;
        public Vector3 size = Vector3.one;
        public SubSpawnArray subSpawns;
        public bool isSelected;
        static readonly Color colorAlpha = new Color(1, .5f, 0, .5f);
        static readonly Color colorSolid = new Color(1, .5f, 0, 1f);
        
        void OnDrawGizmos () {
            switch(type) {
                case SpawnPointType.Point:
                    Gizmos.color = colorAlpha;
                    Gizmos.DrawSphere(transform.position, .5f);
                    Gizmos.color = colorSolid;
                    Gizmos.DrawWireSphere(transform.position, .5f);
                    Gizmos.DrawRay(transform.position, transform.forward * 2);
                    break;
                case SpawnPointType.Area:
                    Gizmos.color = colorAlpha;
                    Gizmos.DrawCube(transform.position, size);
                    break;
                case SpawnPointType.Group:
                    for (int i = 0; i < subSpawns.Length; i++) {
                        Vector3 pos = subSpawns[i].position;
                        Gizmos.color = colorAlpha;
                        Gizmos.DrawSphere(pos, .5f);
                        if (isSelected) {
                            Gizmos.color = GUITools.green;
                            Gizmos.DrawWireSphere(pos, .5f);
                        }
                        Gizmos.color = colorSolid;
                        Gizmos.DrawRay(pos, (Quaternion.Euler(0, subSpawns[i].rotation, 0) * Vector3.forward) * 2);
                    }
                    break;
            }
        }
        #endif
    }
        
    #if UNITY_EDITOR 
    [CustomEditor(typeof(SceneSpawnPoint))]
    class SceneSpawnPointEditor : Editor {
        SerializedProperty typeProp, sizeProp, subSpawnsProp, subSpawnsListProp;

        void OnEnable () {
            Tools.hidden = false;
            (target as SceneSpawnPoint).isSelected = true;

            typeProp = serializedObject.FindProperty("type");
            subSpawnsProp = serializedObject.FindProperty("subSpawns");
            subSpawnsListProp = subSpawnsProp.FindPropertyRelative("list");
            sizeProp = serializedObject.FindProperty("size");
        }
        void OnDisable () {
            Tools.hidden = false;
            (target as SceneSpawnPoint).isSelected = false;
        }
        void OnDestroy () {
            Tools.hidden = false;
            (target as SceneSpawnPoint).isSelected = false;
        }


        string RepeatedNamesString (List<string> copyName) {
            string r = "\n";
            for (int i = 0; i < copyName.Count; i++) {
                r += "'" + copyName[i] + "',\n";
            }
            return r;
        }

        bool CheckSubSpawns (out List<string> copyName) {

            int c = subSpawnsListProp.arraySize;
            for (int i = 0; i < c; i++) {
                subSpawnsListProp.GetArrayElementAtIndex(i).FindPropertyRelative("isError").boolValue = false;
            }
            
            copyName = new List<string>();
                
            for (int i = 0; i < c; i++) {
                SerializedProperty spawnProp0 = subSpawnsListProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp0 = spawnProp0.FindPropertyRelative("name");
                SerializedProperty errorProp0 = spawnProp0.FindPropertyRelative("isError");
                string name0 = nameProp0.stringValue;


                for (int j = i+1; j < c; j++) {
                    SerializedProperty spawnProp1 = subSpawnsListProp.GetArrayElementAtIndex(j);
                    SerializedProperty nameProp1 = spawnProp1.FindPropertyRelative("name");

                    if (name0 == nameProp1.stringValue) {
                        errorProp0.boolValue = true;
                        spawnProp1.FindPropertyRelative("isError").boolValue = true;
                        
                        if (!copyName.Contains(name0)) 
                            copyName.Add(name0);
                        
                        break;
                    }
                }
            }
            return copyName.Count > 0;
        }

        const string changeGroupMessage = "Are you sure you want to change the spawn type? Disabling 'Group' type will delete all built Spawns in the group!";
        public override void OnInspectorGUI() {
            
            SpawnPointType oldType = (SpawnPointType)typeProp.enumValueIndex;
            SpawnPointType newType = (SpawnPointType)EditorGUILayout.EnumPopup("Type", oldType);

            if (newType != oldType) {
                if (oldType == SpawnPointType.Group) {
                    
                    if (EditorUtility.DisplayDialog("Change Spawn Type", changeGroupMessage, "Yes", "No")) 
                        subSpawnsListProp.ClearArray();
                    else 
                        newType = SpawnPointType.Group;
                }
                typeProp.enumValueIndex = (int)newType;
            }
            
            if (newType == SpawnPointType.Area)
                EditorGUILayout.PropertyField(sizeProp);
            
            if (newType == SpawnPointType.Group) {
                List<string> copyNames;                
                if (CheckSubSpawns (out copyNames)) {
                    EditorGUILayout.HelpBox("Sub Spawns Have Repeated Names: \n" + RepeatedNamesString(copyNames) + "\nSubspawns marked red will not be saved...", MessageType.Error);
                }
                EditorGUILayout.PropertyField(subSpawnsProp);
                SpawnPainter.OnGUI( );
            }

            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(target);
        }

        void OnSceneGUI () {
            Tools.hidden = false;

            SpawnPointType type = (SpawnPointType)typeProp.enumValueIndex;
            switch(type) {
                case SpawnPointType.Area: 
                    DrawBounds(); 
                    break;
                case SpawnPointType.Group: 
                    Tools.hidden = true;
                    DrawGroupSceneGUI(); 
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        static readonly Color32 colorSolid = new Color32(255, 128, 0, 255);

        void DrawGroupSceneGUI () {

            float screenW = SceneView.lastActiveSceneView.position.width;
            float screenH = SceneView.lastActiveSceneView.position.height;
            
            for (int i = 0; i < subSpawnsListProp.arraySize; i++) {
                SerializedProperty s = subSpawnsListProp.GetArrayElementAtIndex(i);

                bool selected = s.FindPropertyRelative("selected").boolValue;
                bool hovered = s.FindPropertyRelative("hovered").boolValue;

                if (!selected && !hovered)
                    continue;
                
                SerializedProperty posProp = s.FindPropertyRelative("position");
                
                Vector3 pos = posProp.vector3Value;
                Vector3 screenPos = SceneView.lastActiveSceneView.camera.WorldToViewportPoint(pos);

                Handles.BeginGUI();
                GUIContent lbl = new GUIContent(s.FindPropertyRelative("name").stringValue);
                float w = GUITools.label.CalcSize(lbl).x;
                Rect labelRect = new Rect(screenPos.x * screenW - w * .5f, (1-screenPos.y) * screenH - EditorGUIUtility.singleLineHeight, w, EditorGUIUtility.singleLineHeight);
                GUITools.Box(labelRect, hovered ? GUITools.white : colorSolid);
                GUI.Label(labelRect, lbl);
                Handles.EndGUI();
                
                if (selected || hovered) {
                    posProp.vector3Value = Handles.PositionHandle(pos, Quaternion.Euler(0, s.FindPropertyRelative("rotation").floatValue, 0));
                }
            }

            SpawnPainter.OnSceneGUI( subSpawnsListProp, GetHashCode() );
    
        }

        BoxBoundsHandle boundsHandle = new BoxBoundsHandle();
        void DrawBounds() {
            Handles.color = new Color(1, .5f, 0, 1);
            boundsHandle.center = (target as SceneSpawnPoint).transform.position;
            boundsHandle.size = sizeProp.vector3Value;
            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Change Bounds");
                sizeProp.vector3Value = boundsHandle.size;
            }
        }
    }


    public static class SpawnPainter
    {
        public static bool drawing;
        static float yAngle;
        static Vector3 hitPos;
        
        static public void OnGUI ()
        {    
            if (GUITools.Button(new GUIContent((drawing ? "Exit " : "") + "Draw Mode"), drawing ? GUITools.red : GUITools.white, GUITools.button, GUITools.black))
                drawing = !drawing;   
        }
        
        static public void OnSceneGUI( SerializedProperty spawnList, int hashID )
        {
            if (!drawing) 
                return;
            
            if (!InternalEditorUtility.isApplicationActive)
                return;

            Vector2 mousePos = Event.current.mousePosition;
            
            Handles.BeginGUI();
            GUI.Label(new Rect(mousePos.x + 8, (mousePos.y + 8) - 75, 256, 256), string.Format("Left Click: Draw\n{0}: Focus area\n{1}: Exit Draw\n{2}: Rotate Preview\n", KeyCode.F, KeyCode.Escape, KeyCode.Space));
            Handles.EndGUI();

            EditorWindowTools.FocusSceneViewOnMouseOver();

            Event current = Event.current;
            if (EventType.KeyUp == current.type) {
                bool earlyOut = false;
                if(KeyCode.Escape == current.keyCode) {
                    drawing = false;
                    earlyOut = true;
                }
                if(KeyCode.Space == current.keyCode) {
                    earlyOut = true;
                    yAngle = (yAngle + 15f) % 360f;
                }
                if (earlyOut) {
                    current.Use();
                    return;
                }
            }

            bool focus = false;
            if((EventType.KeyUp == current.type ||  EventType.KeyDown == current.type ) && KeyCode.F == current.keyCode) {
                focus = EventType.KeyUp == current.type;
                current.Use();
            }
            
            int controlId = GUIUtility.GetControlID(hashID, FocusType.Passive);
            bool isValid = CalculateMousePosition(mousePos);
            if (isValid){
                
                Handles.DrawWireDisc(hitPos, Vector3.up, 1f);

                if (focus)
                    SceneView.lastActiveSceneView.Frame(new Bounds(hitPos, Vector3.one), false);
                
                if ((current.type == EventType.MouseDown) && !current.alt) {
                    if (current.button == 0) {
                        AddNewSubspawn( spawnList );
                    }
                }

                GUI.enabled = false;
                Handles.PositionHandle (hitPos, Quaternion.Euler(0,yAngle,0));
                GUI.enabled = true;
            }
            
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(controlId);

            SceneView.RepaintAll();
        }

        static void AddNewSubspawn (SerializedProperty spawnList) {
            int c = spawnList.arraySize;
            spawnList.InsertArrayElementAtIndex(c);
            SubSpawn.InitializeNewSubspawn (spawnList.GetArrayElementAtIndex(c), hitPos, yAngle);
        }

        static bool CalculateMousePosition (Vector2 mousePos)
        {
            RaycastHit hit;
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(mousePos), out hit, Mathf.Infinity, GameManager.environmentMask))
            {
                hitPos = hit.point;
                return true;
            }
            return false;
        }   
    }

    #endif
}