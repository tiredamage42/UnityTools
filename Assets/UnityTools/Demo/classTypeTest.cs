
using UnityEngine;
using UnityTools;

using UnityEditor;
using UnityTools.EditorTools;


    using ONamepsac;
[System.Serializable] public class BaseClassTestArray : NeatArrayWrapper<BaseClassTest> { }



    [CustomPropertyDrawer(typeof(BaseClassTestArray))] 
    class ConditionsArrayAttributeDrawer : NeatArrayAttributeDrawer
    {
        

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
                
                float typeSelectWidth = 100;
                
                System.Type[] conditionTypes = typeof(BaseClassTest).FindDerivedTypes(false);

                string[] conditionTypeNames = new string[conditionTypes.Length];
                for (int x = 0; x < conditionTypes.Length; x++) {
                    conditionTypeNames[x] = conditionTypes[x].Name;
                }
                    
                float y = pos.y + GUITools.singleLineHeight;

                Rect typeSelectRect = new Rect(xOffset, y, typeSelectWidth, EditorGUIUtility.singleLineHeight);
                Rect propRect = new Rect(xOffset + typeSelectWidth, y, (indent2Width - GUITools.toolbarDividerSize) - typeSelectWidth, EditorGUIUtility.singleLineHeight);


                int indexToDelete = -1;

                for (int i = 0; i < prop.arraySize; i++) {

                    if (GUITools.IconButton(indent1, y, deleteContent, GUITools.red))
                        indexToDelete = i;
                    
                    SerializedProperty p = prop.GetArrayElementAtIndex(i);

                    int origType = -1;

                    if (p.objectReferenceValue != null) {
                        for (int x = 0; x < conditionTypes.Length; x++) {
                            if (p.objectReferenceValue.GetType() == conditionTypes[x]) {
                                origType = x;
                                break;
                            }
                        }
                    }

                    int newType = EditorGUI.Popup(typeSelectRect, origType, conditionTypeNames);

                    if (newType != origType) {
                        Object.DestroyImmediate(p.objectReferenceValue, true);

                        var newObj = ScriptableObject.CreateInstance(conditionTypes[newType]);
                        newObj.name = conditionTypes[newType].Name;

                        if (isAsset) {
                            AssetDatabase.AddObjectToAsset(newObj, baseObject);
                            AssetDatabase.SaveAssets(); 
                        }

                        p.objectReferenceValue = newObj;
                    }

                    if (p.objectReferenceValue == null) {
                        EditorGUI.PropertyField(propRect, p, label);   
                    }
                    else {

                        EditorGUI.BeginChangeCheck();
                        ((BaseClassTest)p.objectReferenceValue).DrawGUI(propRect);
                        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(p.objectReferenceValue);
                    }
                    
                    float h = EditorGUI.GetPropertyHeight(p, true);

                    y += h;

                    typeSelectRect.y += h;
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



namespace ONamepsac {

    public abstract class BaseClassTest : ScriptableObject {

        public abstract void DrawGUI(Rect position);
        public virtual float GetPropertyHeight() {
            return GUITools.singleLineHeight;
        }
    }
        

    [CustomPropertyDrawer(typeof(BaseClassTest))] 
    class BaseClassTestDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, GUITools.noContent);   
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null) return GUITools.singleLineHeight;
            return ((BaseClassTest)property.objectReferenceValue).GetPropertyHeight();
        }
    }
}
