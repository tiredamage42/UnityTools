using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using UnityEditor;
using UnityTools.EditorTools;


namespace UnityTools {
    
    [System.Serializable] public class ConditionsParameters : NeatArrayWrapper<ConditionsParameter> { 
        
    }

    [CustomPropertyDrawer(typeof(ConditionsParameters))] class ConditionsParametersDrawer : NeatArrayAttributeDrawer
    {
        GUIContent chooseTypeContent = BuiltInIcons.GetIcon("ClothInspector.SelectTool", "Choose Parameter Type");

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
                
                float y = pos.y + GUITools.singleLineHeight;

                float o = GUITools.iconButtonWidth + GUITools.toolbarDividerSize;
                Rect propRect = new Rect(xOffset + o, y, (indent2Width - o) - GUITools.toolbarDividerSize * 2, EditorGUIUtility.singleLineHeight);

                int indexToDelete = -1;

                for (int i = 0; i < prop.arraySize; i++) {

                    if (GUITools.IconButton(indent1, y, deleteContent, GUITools.red))
                        indexToDelete = i;
                    
                    SerializedProperty p = prop.GetArrayElementAtIndex(i);
                    
                    if (GUITools.IconButton(xOffset, y, chooseTypeContent, GUITools.white)) {
                        System.Type[] paramTypes = typeof(ConditionsParameter).FindDerivedTypes(false);

                        GenericMenu menu = new GenericMenu();

                        Vector2 mousePos = Event.current.mousePosition;
                        menu.DropDown(new Rect(mousePos.x, mousePos.y, 0, 0));

                        for (int x = 0; x < paramTypes.Length; x++) {
                            bool isType = p.objectReferenceValue != null && p.objectReferenceValue.GetType() == paramTypes[x];
                            menu.AddItem(
                                new GUIContent(paramTypes[x].Name.Split('_')[0]), 
                                p.objectReferenceValue != null && p.objectReferenceValue.GetType() == paramTypes[x],
                                (newType) => {
                                
                                    if (!isType) {
                                        Object.DestroyImmediate(p.objectReferenceValue, true);
                                        var newObj = ScriptableObject.CreateInstance(paramTypes[(int)newType]);
                                        newObj.name = paramTypes[(int)newType].Name;

                                        if (isAsset) {
                                            AssetDatabase.AddObjectToAsset(newObj, baseObject);
                                            AssetDatabase.SaveAssets(); 
                                        }

                                        p.objectReferenceValue = newObj;
                                        p.serializedObject.ApplyModifiedProperties();
                                        EditorUtility.SetDirty(p.serializedObject.targetObject);
                                    }

                                },
                                x
                            
                            );
                        }
                        menu.ShowAsContext();
                    }

                    GUITools.DrawToolbarDivider(xOffset+GUITools.iconButtonWidth, y);
        
                    if (p.objectReferenceValue != null) {
                        
                        EditorGUI.BeginChangeCheck();
                        // float propWidth = 
                        ((ConditionsParameter)p.objectReferenceValue).DrawGUI(propRect);
                        if (EditorGUI.EndChangeCheck()) {
                            EditorUtility.SetDirty(p.objectReferenceValue);
                            EditorUtility.SetDirty(baseObject);

                        }
                        
                    }

                    float h = EditorGUI.GetPropertyHeight(p, true);
                    
                    y += h;
                    propRect.y += h;
                }

                
                if (indexToDelete != -1) {
                    SerializedProperty deleted = prop.GetArrayElementAtIndex(indexToDelete);
                    
                    Object deletedObject = deleted.objectReferenceValue;
                    
                    if (deletedObject != null) prop.DeleteArrayElementAtIndex(indexToDelete);
                    
                    prop.DeleteArrayElementAtIndex(indexToDelete);
                    
                    Object.DestroyImmediate(deletedObject, true);
                    
                    if (isAsset) {
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(baseObject));
                        AssetDatabase.SaveAssets(); 
                    }                
                }
            }

            EditorGUI.EndProperty();
        }
    }

    public abstract class ConditionsParameter : ScriptableObject {

        public abstract object GetParamObject ();
        
        public abstract void DrawGUI(Rect pos);
        public virtual float GetPropertyHeight() { 
            return GUITools.singleLineHeight; 
        }
    }
        
    [CustomPropertyDrawer(typeof(ConditionsParameter))] class ConditionsParameterDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            if (prop.objectReferenceValue == null) return GUITools.singleLineHeight;
            return ((ConditionsParameter)prop.objectReferenceValue).GetPropertyHeight();
        }
    }
}
