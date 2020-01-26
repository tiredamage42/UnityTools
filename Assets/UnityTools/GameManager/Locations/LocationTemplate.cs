using System;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
#endif

using UnityTools.EditorTools;
namespace UnityTools.Internal {
    /*
        Editor Representation of the locations, used as a template
        for the final locations saved
    */

    #if UNITY_EDITOR 

    [Serializable] public class SubLocationArray : NeatArrayWrapper<SubLocation> { }
    [Serializable] public struct SubLocation {
        public bool isError, hovered, selected;
        public string name;
        public Vector3 position;
        public float rotation;

        public static void InitializeNewSubLocation (SerializedProperty prop, Vector3 position, float rotation) {
            prop.FindPropertyRelative("isError").boolValue = false;
            prop.FindPropertyRelative("hovered").boolValue = false;
            prop.FindPropertyRelative("selected").boolValue = false;
            
            prop.FindPropertyRelative("name").stringValue = null;
            prop.FindPropertyRelative("position").vector3Value = position;
            prop.FindPropertyRelative("rotation").floatValue = rotation;
        }
    }

    [CustomPropertyDrawer(typeof(SubLocationArray))] class SubLocationArrayDrawer : PropertyDrawer
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

                if (!LocationPainter.drawing)
                {
                    if((EventType.KeyUp == e.type ||  EventType.KeyDown == e.type ) && KeyCode.F == e.keyCode) {
                        focus = EventType.KeyUp == e.type;
                        e.Use();
                    }
                }    
            }

            for (int i = 0; i < prop.arraySize; i++) {
                if (GUITools.IconButton(pos.x, fieldsY, BuiltInIcons.GetIcon("Toolbar Minus", "Delete Element"), GUITools.red))
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
                    nameProp.stringValue = "SubLcoation_" + i.ToString();
                EditorGUI.PropertyField(fieldRect, nameProp, GUITools.noContent, true);

                GUITools.DrawIconToggle(p.FindPropertyRelative("selected"), new GUIContent("", "Select For Movement"), selectX, fieldsY);

                if (!isRotating) {
                    if (Event.current.type == EventType.MouseDown) {
            
                        if (rotateRect.Contains(mousePos)) {
                            hovered.boolValue = true;
                            rotateProp = p;
                            isRotating = true;
                        }
                    }
                }
                GUITools.DrawIconToggle(isRotating && hovered.boolValue, rotateGUI, rotateX, fieldsY);
                

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

    public class LocationTemplate : MonoBehaviour {
        #if UNITY_EDITOR 
        public LocationType type;
        public Vector3 size = Vector3.one;
        public SubLocationArray subLocations;
        public bool isSelected;
        static readonly Color colorAlpha = new Color(1, .5f, 0, .5f);
        static readonly Color colorSolid = new Color(1, .5f, 0, 1f);
        
        void OnDrawGizmos () {
            switch(type) {
                case LocationType.Point:
                    Gizmos.color = colorAlpha;
                    Gizmos.DrawSphere(transform.position, .5f);
                    Gizmos.color = colorSolid;
                    Gizmos.DrawWireSphere(transform.position, .5f);
                    Gizmos.DrawRay(transform.position, transform.forward * 2);
                    break;
                case LocationType.Area:
                    Gizmos.color = colorAlpha;
                    Gizmos.DrawCube(transform.position, size);
                    break;
                case LocationType.Group:
                    for (int i = 0; i < subLocations.Length; i++) {
                        Vector3 pos = subLocations[i].position;
                        Gizmos.color = colorAlpha;
                        Gizmos.DrawSphere(pos, .5f);
                        if (isSelected) {
                            Gizmos.color = GUITools.green;
                            Gizmos.DrawWireSphere(pos, .5f);
                        }
                        Gizmos.color = colorSolid;
                        Gizmos.DrawRay(pos, (Quaternion.Euler(0, subLocations[i].rotation, 0) * Vector3.forward) * 2);
                    }
                    break;
            }
        }
        #endif
    }
        
    #if UNITY_EDITOR 
    [CustomEditor(typeof(LocationTemplate))]
    class LocationTemplateEditor : Editor {
        SerializedProperty typeProp, sizeProp, subLocsProp, subLocsListProp;

        void OnEnable () {
            Tools.hidden = false;
            (target as LocationTemplate).isSelected = true;

            typeProp = serializedObject.FindProperty("type");
            subLocsProp = serializedObject.FindProperty("subLocations");
            subLocsListProp = subLocsProp.FindPropertyRelative("list");
            sizeProp = serializedObject.FindProperty("size");
        }
        void OnDisable () {
            Tools.hidden = false;
            (target as LocationTemplate).isSelected = false;
        }
        void OnDestroy () {
            Tools.hidden = false;
            (target as LocationTemplate).isSelected = false;
        }


        string RepeatedNamesString (List<string> copyName) {
            string r = "\n";
            for (int i = 0; i < copyName.Count; i++) {
                r += "'" + copyName[i] + "',\n";
            }
            return r;
        }

        bool CheckSubLocations (SerializedProperty subLocsListProp, out List<string> copyName) {

            int c = subLocsListProp.arraySize;
            for (int i = 0; i < c; i++) {
                subLocsListProp.GetArrayElementAtIndex(i).FindPropertyRelative("isError").boolValue = false;
            }
            
            copyName = new List<string>();
                
            for (int i = 0; i < c; i++) {
                SerializedProperty locProp0 = subLocsListProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp0 = locProp0.FindPropertyRelative("name");
                SerializedProperty errorProp0 = locProp0.FindPropertyRelative("isError");
                string name0 = nameProp0.stringValue;


                for (int j = i+1; j < c; j++) {
                    SerializedProperty locProp1 = subLocsListProp.GetArrayElementAtIndex(j);
                    SerializedProperty nameProp1 = locProp1.FindPropertyRelative("name");

                    if (name0 == nameProp1.stringValue) {
                        errorProp0.boolValue = true;
                        locProp1.FindPropertyRelative("isError").boolValue = true;
                        
                        if (!copyName.Contains(name0)) 
                            copyName.Add(name0);
                        
                        break;
                    }
                }
            }
            return copyName.Count > 0;
        }

        const string changeGroupMessage = "Are you sure you want to change the location type? Disabling 'Group' type will delete all built sub locations in the group!";
        public override void OnInspectorGUI() {
            
            LocationType oldType = (LocationType)typeProp.enumValueIndex;
            LocationType newType = (LocationType)EditorGUILayout.EnumPopup("Type", oldType);

            if (newType != oldType) {
                if (oldType == LocationType.Group) {
                    
                    if (EditorUtility.DisplayDialog("Change Location Type", changeGroupMessage, "Yes", "No")) 
                        subLocsListProp.ClearArray();
                    else 
                        newType = LocationType.Group;
                }
                typeProp.enumValueIndex = (int)newType;
            }
            
            if (newType == LocationType.Area)
                EditorGUILayout.PropertyField(sizeProp);
            
            if (newType == LocationType.Group) {
                List<string> copyNames;                
                if (CheckSubLocations (subLocsListProp, out copyNames)) {
                    EditorGUILayout.HelpBox("Sub Locations Have Repeated Names: \n" + RepeatedNamesString(copyNames) + "\nSubLocations marked red will not be saved...", MessageType.Error);
                }
                EditorGUILayout.PropertyField(subLocsProp);
                LocationPainter.OnGUI( );
            }

            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(target);
        }

        void OnSceneGUI () {
            Tools.hidden = false;

            LocationType type = (LocationType)typeProp.enumValueIndex;
            switch(type) {
                case LocationType.Area: 
                    DrawBounds(); 
                    break;
                case LocationType.Group: 
                    Tools.hidden = true;
                    DrawGroupSceneGUI(subLocsListProp); 
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        static readonly Color32 colorSolid = new Color32(255, 128, 0, 255);

        void DrawGroupSceneGUI (SerializedProperty subLocsListProp) {

            float screenW = SceneView.lastActiveSceneView.position.width;
            float screenH = SceneView.lastActiveSceneView.position.height;
            
            for (int i = 0; i < subLocsListProp.arraySize; i++) {
                SerializedProperty s = subLocsListProp.GetArrayElementAtIndex(i);

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

            LocationPainter.OnSceneGUI( subLocsListProp, GetHashCode() );
    
        }

        BoxBoundsHandle boundsHandle = new BoxBoundsHandle();
        void DrawBounds() {
            Handles.color = new Color(1, .5f, 0, 1);
            boundsHandle.center = (target as LocationTemplate).transform.position;
            boundsHandle.size = sizeProp.vector3Value;
            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Change Bounds");
                sizeProp.vector3Value = boundsHandle.size;
            }
        }
    }


    public static class LocationPainter
    {
        public static bool drawing;
        static float yAngle;
        static Vector3 hitPos;
        
        static public void OnGUI ()
        {    
            if (GUITools.Button(new GUIContent((drawing ? "Exit " : "") + "Draw Mode"), drawing ? GUITools.red : GUITools.white, GUITools.button, GUITools.black))
                drawing = !drawing;   
        }
        
        static public void OnSceneGUI( SerializedProperty locationList, int hashID )
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
                    if (current.button == 0) 
                        AddNewSubLocation( locationList );
                }

                GUI.enabled = false;
                Handles.PositionHandle (hitPos, Quaternion.Euler(0,yAngle,0));
                GUI.enabled = true;
            }
            
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(controlId);

            SceneView.RepaintAll();
        }

        static void AddNewSubLocation (SerializedProperty locationList) {
            int c = locationList.arraySize;
            locationList.InsertArrayElementAtIndex(c);
            SubLocation.InitializeNewSubLocation (locationList.GetArrayElementAtIndex(c), hitPos, yAngle);
        }

        static bool CalculateMousePosition (Vector2 mousePos)
        {
            RaycastHit hit;
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(mousePos), out hit, Mathf.Infinity, PhysicsTools.environmentMask))
            {
                hitPos = hit.point;
                return true;
            }
            return false;
        }   
    }

    #endif
}