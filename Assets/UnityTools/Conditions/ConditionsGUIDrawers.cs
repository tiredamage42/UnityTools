using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools.Internal {
    #if UNITY_EDITOR
    /*
        CLEAR THE PARAMETERS WHEN CREATING A NEW CONDITION (IN ARRAY)
        SO DIFFERENT CONDITIONS DONT REFERENCE SAME PARAMETER OBJECTS
    */
    [CustomPropertyDrawer(typeof(Conditions))] 
    class ConditionsDrawer : NeatArrayAttributeDrawer {
        protected override void OnAddNewElement (SerializedProperty newElement) {
            newElement.FindPropertyRelative("parameters").FindPropertyRelative("list").ClearArray();
        }

        protected override void OnDeleteElement (SerializedProperty deleteElement) {

            SerializedProperty paramsList = deleteElement.FindPropertyRelative("parameters").FindPropertyRelative("list");

            Object baseObject = deleteElement.serializedObject.targetObject;
            bool isAsset = AssetDatabase.Contains(baseObject);
                
            for (int i = paramsList.arraySize - 1; i >= 0; i--) {
                SerializedProperty deleted = paramsList.GetArrayElementAtIndex(i);
                    
                Object deletedObj = deleted.objectReferenceValue;
                    
                if (deletedObj != null) paramsList.DeleteArrayElementAtIndex(i);
                paramsList.DeleteArrayElementAtIndex(i);
                    
                Object.DestroyImmediate(deletedObj, true);
                    
                if (isAsset) {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(baseObject));
                    AssetDatabase.SaveAssets(); 
                }
            }


        }
    }

    /*
        DRAW A SINGLE CONDITION:
    */
    [CustomPropertyDrawer(typeof(Condition))] 
    class ConditionDrawer : PropertyDrawer {
        static readonly string[] numericalCheckOptions = new string[] { " == ", " != ", " < ", " > ", " <= ", " >= " };
        protected const float numericalCheckWidth = 40;

        static GUIContent _useGlobalValueGUI;
        static GUIContent useGlobalValueGUI {
            get {
                if (_useGlobalValueGUI == null) {
                    _useGlobalValueGUI = BuiltInIcons.GetIcon("ToolHandleGlobal", "Use Global Value Threshold");
                }
                return _useGlobalValueGUI;
            }
        }
        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            float origX = pos.x;
            float origWidth = pos.width;

            pos.width = 65;

            SerializedProperty runTargetProp = prop.FindPropertyRelative("runTarget");
            EditorGUI.PropertyField(pos, runTargetProp, GUITools.noContent, true);

            pos.x += pos.width;

            pos.width = 125;

            GUITools.StringFieldWithDefault(pos.x, pos.y, pos.width, pos.height, prop.FindPropertyRelative("callMethod"), "Call Method");            
            pos.x += pos.width;

            
            SerializedProperty numCheckProp = prop.FindPropertyRelative("numericalCheck");
            pos.width = numericalCheckWidth;
            numCheckProp.enumValueIndex = EditorGUI.Popup (pos, numCheckProp.enumValueIndex, numericalCheckOptions);
            pos.x += pos.width;
            
            SerializedProperty useGlobalValueThresholdProp = prop.FindPropertyRelative("useGlobalValueThreshold");
            bool useGlobalValueThreshold = useGlobalValueThresholdProp.boolValue;
            

            if (useGlobalValueThreshold) {
                pos.width = 100;
        
                GlobalGameValues.DrawGlobalValueSelector (pos, prop.FindPropertyRelative("globalValueThresholdName"));

            }
            else {

                pos.width = 60;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("threshold"), GUITools.noContent, true);
            }
            pos.x += pos.width;

            GUITools.DrawToolbarDivider(pos.x, pos.y);
            pos.x += GUITools.toolbarDividerSize;
            
            useGlobalValueThreshold = GUITools.DrawToggleButton(useGlobalValueThresholdProp, useGlobalValueGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
            pos.x += GUITools.iconButtonWidth;
            
            bool showParameters = GUITools.DrawToggleButton(prop.FindPropertyRelative("showParameters"), new GUIContent("P", "Show Parameters"), pos.x, pos.y, GUITools.blue, GUITools.white);
            pos.x += GUITools.iconButtonWidth;
            
            SerializedProperty orProp = prop.FindPropertyRelative("or");
            orProp.boolValue = GUITools.DrawToggleButton(orProp.boolValue, new GUIContent(orProp.boolValue ? "||" : "&&"), pos.x, pos.y, GUITools.white, GUITools.white);


            if (runTargetProp.enumValueIndex == (int)RunTarget.Reference) {
                pos.y += EditorGUIUtility.singleLineHeight;
                pos.x = origX;
                pos.width = origWidth ;
                
                EditorGUI.LabelField(pos, "Reference:");

                pos.x += 65;
                pos.width -= 65;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("referenceTarget"), GUITools.noContent, true);
            }
            
                     
            pos.y += EditorGUIUtility.singleLineHeight;
            pos.x = origX ;
            pos.width = origWidth ;
            
            if (showParameters) {

                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("parameters"), true);
            }
            else {
                SerializedProperty paramsProp = prop.FindPropertyRelative("parameters");
                
                if (paramsProp.FindPropertyRelative("list").arraySize > 0) {
                    pos.x += GUITools.iconButtonWidth;
                    pos.width -= GUITools.iconButtonWidth;

                    ConditionsParametersDrawer.DrawFlat(pos, paramsProp);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            
            float h = GUITools.singleLineHeight;

            if (prop.FindPropertyRelative("showParameters").boolValue) {
                h += EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("parameters"), true);
            }
            else {
                if (prop.FindPropertyRelative("parameters").FindPropertyRelative("list").arraySize > 0) {
                    h += GUITools.singleLineHeight;
                }
            }
            if (prop.FindPropertyRelative("runTarget").enumValueIndex == (int)RunTarget.Reference) {
                h += GUITools.singleLineHeight;
            }
            return h;
        }
    }

    /*
        DRAW CONDITION FUNCTION PARAMETERS LIST
        (NEEDS SPECIAL CONSIDERATION SINCE EACH PARAMETER IS AN SCRIPTABLE OBJECT REFERENCE, IN ORDER TO
            SUPPORT POLYMORPHISM IN THE SAME ARRAY...
        )
    */

    [CustomPropertyDrawer(typeof(ConditionsParameters))] 
    class ConditionsParametersDrawer : NeatArrayAttributeDrawer
    {
        static GUIContent _chooseTypeContent;
        static GUIContent chooseTypeContent {
            get {
                if (_chooseTypeContent == null) _chooseTypeContent = BuiltInIcons.GetIcon("ClothInspector.SelectTool", "Choose Parameter Type");
                return _chooseTypeContent;
            }
        }
        static float paramsLabelW;
        
        static GUIContent _paramsLabel;
        static GUIContent paramsLabel {
            get {
                if (_paramsLabel == null) {
                    _paramsLabel = new GUIContent("Params: ");
                    paramsLabelW = GUITools.boldLabel.CalcSize(_paramsLabel).x;
                }
                return _paramsLabel;
            }
        }
        
        public static void DrawFlat (Rect pos, SerializedProperty prop) {
            // the property we want to draw is the list child
            prop = prop.FindPropertyRelative(listName);
            int arraySize = prop.arraySize;
            float propW = (pos.width - paramsLabelW) * (1f/arraySize);

            pos.width = paramsLabelW;
            GUITools.Label(pos, paramsLabel, GUITools.black, GUITools.boldLabel);
            pos.x += paramsLabelW;

            pos.width = propW;

            for (int i = 0; i < arraySize; i++) {
                Object o = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                if (o != null) {
                    EditorGUI.BeginChangeCheck();
                    ((ConditionsParameter)o).DrawGUIFlat(pos);
                    if (EditorGUI.EndChangeCheck()) {
                        EditorUtility.SetDirty(o);
                        EditorUtility.SetDirty(prop.serializedObject.targetObject);
                    }                        
                }
                pos.x += propW;
            }
        }

        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {

            float indent1, indent2, indent2Width;
            bool displayedValue;
            StartArrayDraw ( pos, ref prop, ref label, out indent1, out indent2, out indent2Width, out displayedValue );

            DrawAddElement ( pos, prop, indent1, displayedValue );
            
            float xOffset = indent2 + GUITools.toolbarDividerSize;
            DrawArrayTitle ( pos, prop, label, xOffset );
            
            if (displayedValue) {
                Object baseObject = prop.serializedObject.targetObject;
                bool isAsset = AssetDatabase.Contains(baseObject);
                
                float o = GUITools.iconButtonWidth + GUITools.toolbarDividerSize;

                float dividerX = xOffset + GUITools.iconButtonWidth;

                pos.x = xOffset + o;
                pos.y = pos.y + GUITools.singleLineHeight;
                pos.width = (indent2Width - o) - GUITools.toolbarDividerSize * 2;

                int indexToDelete = -1;

                for (int i = 0; i < prop.arraySize; i++) {

                    if (GUITools.IconButton(indent1, pos.y, deleteContent, GUITools.red))
                        indexToDelete = i;
                    
                    SerializedProperty p = prop.GetArrayElementAtIndex(i);
                    Object obj = p.objectReferenceValue;

                    if (GUITools.IconButton(xOffset, pos.y, chooseTypeContent, GUITools.white)) {
                        System.Type[] paramTypes = typeof(ConditionsParameter).FindDerivedTypes(false);

                        GenericMenu menu = new GenericMenu();

                        Vector2 mousePos = Event.current.mousePosition;
                        menu.DropDown(new Rect(mousePos.x, mousePos.y, 0, 0));

                        for (int x = 0; x < paramTypes.Length; x++) {
                            bool isType = obj != null && obj.GetType() == paramTypes[x];
                            menu.AddItem(
                                new GUIContent(paramTypes[x].Name.Split('_')[0]), 
                                isType,
                                (newType) => {
                                    if (!isType) {
                                        System.Type newT = paramTypes[(int)newType];

                                        Object.DestroyImmediate(obj, true);
                                        var newObj = ScriptableObject.CreateInstance(newT);
                                        newObj.name = newT.Name;

                                        if (isAsset) {
                                            AssetDatabase.AddObjectToAsset(newObj, baseObject);
                                            AssetDatabase.SaveAssets(); 
                                        }

                                        p.objectReferenceValue = newObj;
                                        p.serializedObject.ApplyModifiedProperties();
                                        EditorUtility.SetDirty(baseObject);
                                    }
                                },
                                x
                            );
                        }
                        menu.ShowAsContext();
                    }


                    GUITools.DrawToolbarDivider(dividerX, pos.y);

                    obj = p.objectReferenceValue;
        
                    if (obj != null) {
                        
                        EditorGUI.BeginChangeCheck();
                        ((ConditionsParameter)obj).DrawGUI(pos);
                        if (EditorGUI.EndChangeCheck()) {
                            EditorUtility.SetDirty(obj);
                            EditorUtility.SetDirty(baseObject);
                        }
                    }
                    pos.y += EditorGUI.GetPropertyHeight(p, true);
                }

                
                if (indexToDelete != -1) {
                    SerializedProperty deleted = prop.GetArrayElementAtIndex(indexToDelete);
                    
                    Object deletedObj = deleted.objectReferenceValue;
                    
                    if (deletedObj != null) prop.DeleteArrayElementAtIndex(indexToDelete);
                    
                    prop.DeleteArrayElementAtIndex(indexToDelete);
                    
                    Object.DestroyImmediate(deletedObj, true);
                    
                    if (isAsset) {
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(baseObject));
                        AssetDatabase.SaveAssets(); 
                    }                
                }
            }

            EditorGUI.EndProperty();
        }
    }
    /*
        JUST TO SPECIFY THE HEIGHT WHEN CHECKING CONDITIONS PARAMETER AS A SERIALIZED PROPERTY
        (FOR ConditionsParametersDrawer BASE CLASS CHECK HEIGHT)
    */
    [CustomPropertyDrawer(typeof(ConditionsParameter))] 
    class ConditionsParameterDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            Object o = prop.objectReferenceValue;
            if (o == null) return GUITools.singleLineHeight;
            return ((ConditionsParameter)o).GetPropertyHeight();
        }
    }
    #endif
}

