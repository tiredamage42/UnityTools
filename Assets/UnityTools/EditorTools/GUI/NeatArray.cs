
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityTools.EditorTools {
    
    #if UNITY_EDITOR
    /*
        more intuitive array/list handling for gui 
    */

    [CustomPropertyDrawer(typeof(NeatArrayAttribute))] 
    public class NeatArrayAttributeDrawer : PropertyDrawer
    {
        protected GUIContent isShownContent = BuiltInIcons.GetIcon("animationvisibilitytoggleon", "Hide");
        protected GUIContent hiddenContent = BuiltInIcons.GetIcon("animationvisibilitytoggleoff", "Show");
        protected GUIContent addContent = BuiltInIcons.GetIcon("Toolbar Plus", "Add New Element");
        protected GUIContent deleteContent = BuiltInIcons.GetIcon("Toolbar Minus", "Delete Element");




        const string displayedName = "displayed";
        const string listName = "list";

        void MakeSureSizeIsOK (SerializedProperty prop, int enforceSize) {
            
            if (enforceSize < 0)
                return;
            

            if (prop.arraySize != enforceSize) {
                if (prop.arraySize > enforceSize) {
                    prop.ClearArray();
                }
                
                int c = enforceSize - prop.arraySize;
                for (int i = 0; i < c; i++) {
                    prop.InsertArrayElementAtIndex(prop.arraySize);
                }
            }
        }

        protected bool DrawDisplayedToggle (Rect pos, SerializedProperty prop) {
            SerializedProperty displayed = prop.FindPropertyRelative(displayedName);
            if (GUITools.IconButton(pos.x, pos.y, displayed.boolValue ? isShownContent : hiddenContent, GUITools.white)){
                displayed.boolValue = !displayed.boolValue;
            }
            return displayed.boolValue;
        }

        protected void DrawArrayTitle (Rect pos, SerializedProperty prop, GUIContent label, float xOffset) {
            
            label.text += " [" + prop.arraySize + "]";
            GUITools.Label(new Rect(xOffset, pos.y, pos.width, EditorGUIUtility.singleLineHeight), label, GUITools.black, GUITools.boldLabel);
        }

        protected void DrawAddElement (Rect pos, SerializedProperty prop, float indent1, bool displayedValue) {
            GUI.enabled = displayedValue;
            if (GUITools.IconButton(indent1, pos.y, addContent, displayedValue ? GUITools.green : GUITools.white)) {
                prop.InsertArrayElementAtIndex(prop.arraySize);
                SerializedProperty p = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                if (p.propertyType == SerializedPropertyType.ObjectReference) {
                    p.objectReferenceValue = null;
                }
            }
            GUI.enabled = true;
        }

        protected void StartArrayDraw (Rect pos, ref SerializedProperty prop, ref GUIContent label, out float indent1, out float indent2, out float indent2Width, out bool displayedValue) {
            indent1 = pos.x + GUITools.iconButtonWidth;
            indent2 = indent1 + GUITools.iconButtonWidth;
            indent2Width = pos.width - GUITools.iconButtonWidth * 2;
            
            EditorGUI.BeginProperty(pos, label, prop);

            displayedValue = DrawDisplayedToggle ( pos, prop );

            // the property we want to draw is the list child
            prop = prop.FindPropertyRelative(listName);

            DrawBox ( pos, prop, ref label, indent1, displayedValue );
        }

        protected void DrawBox (Rect pos, SerializedProperty prop, ref GUIContent label, float indent1, bool displayedValue) {
            string lbl = label.text;
            string tooltip = label.tooltip;
            
            float h = CalculateHeight(prop, displayedValue);
            label.text = lbl;
            label.tooltip = tooltip;
            
            GUITools.Box ( new Rect ( indent1,  pos.y, pos.width - GUITools.iconButtonWidth, h + GUITools.singleLineHeight * .1f), GUITools.shade );
        }
            
            
            

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            NeatArrayAttribute att = attribute as NeatArrayAttribute;

            float indent1, indent2, indent2Width;
            bool displayedValue;
            StartArrayDraw ( pos, ref prop, ref label, out indent1, out indent2, out indent2Width, out displayedValue );

            MakeSureSizeIsOK(prop, att.enforceSize);
            
            if (att.enforceSize < 0) {

                DrawAddElement ( pos, prop, indent1, displayedValue );
            }

            float xOffset = (att.enforceSize < 0 ? indent2 : indent1) + GUITools.toolbarDividerSize;

            DrawArrayTitle ( pos, prop, label, xOffset );
            
            if (displayedValue) {
                float y = pos.y;
                y += GUITools.singleLineHeight;
                
                int indexToDelete = -1;

                for (int i = 0; i < prop.arraySize; i++) {
                    if (att.enforceSize < 0) {
                        if (GUITools.IconButton(indent1, y, deleteContent, GUITools.red))
                            indexToDelete = i;
                    }
                    
                    SerializedProperty p = prop.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(xOffset, y, indent2Width - GUITools.toolbarDividerSize, EditorGUIUtility.singleLineHeight), p, true);
                    y += EditorGUI.GetPropertyHeight(p, true);
                }
                
                if (indexToDelete != -1) {
                    SerializedProperty p = prop.GetArrayElementAtIndex(indexToDelete);
                    if (p.propertyType == SerializedPropertyType.ObjectReference) {
                        prop.DeleteArrayElementAtIndex(indexToDelete);
                    }
                    prop.DeleteArrayElementAtIndex(indexToDelete);
                }
            }

            EditorGUI.EndProperty();
        }

        protected float CalculateHeight (SerializedProperty prop, bool displayed) {
            if (!displayed) return GUITools.singleLineHeight;
            float h = GUITools.singleLineHeight;
            int arraySize = prop.arraySize;
            for (int i = 0; i < arraySize; i++) h += EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(i), true);
            return h;
        }
    
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return CalculateHeight(prop.FindPropertyRelative(listName), prop.FindPropertyRelative(displayedName).boolValue) + GUITools.singleLineHeight * .25f;
        }
    }
    #endif


    /*
        when we need custom classes to wrap elements with attributes
    */
    [System.Serializable] public class NeatArrayElement { };

    [CustomPropertyDrawer(typeof(NeatArrayElement), true)] 
    public class NeatArrayElementDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative(NeatArray.elementName), GUITools.noContent, true);
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight;
        }
    }

    public class NeatArray {
        public const string elementName = "element";
    }
    
    //the actual attribute
    public class NeatArrayAttribute : PropertyAttribute { 
        public int enforceSize;
        public NeatArrayAttribute () {
            enforceSize = -1;
        }
        public NeatArrayAttribute(int enforceSize) {
            this.enforceSize = enforceSize;
        }
    }
    
    [Serializable] public class NeatBoolList : NeatListWrapper<bool> {}
    [Serializable] public class NeatBoolArray : NeatArrayWrapper<bool> {}
    
    [Serializable] public class NeatStringList : NeatListWrapper<string> {}
    [Serializable] public class NeatStringArray : NeatArrayWrapper<string> {}
    
    [Serializable] public class NeatIntList : NeatListWrapper<int> {}
    [Serializable] public class NeatIntArray : NeatArrayWrapper<int> {}
    
    [Serializable] public class NeatFloatList : NeatListWrapper<float> {}
    [Serializable] public class NeatFloatArray : NeatArrayWrapper<float> {}

    [Serializable] public class NeatAudioClipList : NeatListWrapper<AudioClip> {}
    [Serializable] public class NeatAudioClipArray : NeatArrayWrapper<AudioClip> {}

    [Serializable] public class NeatAnimationClipList : NeatListWrapper<AnimationClip> {}
    [Serializable] public class NeatAnimationClipArray : NeatArrayWrapper<AnimationClip> {}

    [Serializable] public class NeatAudioSourceList : NeatListWrapper<AudioSource> {}
    [Serializable] public class NeatAudioSourceArray : NeatArrayWrapper<AudioSource> {}

    [Serializable] public class NeatTransformList : NeatListWrapper<Transform> {}
    [Serializable] public class NeatTransformArray : NeatArrayWrapper<Transform> {}

    [Serializable] public class NeatGameObjectList : NeatListWrapper<GameObject> {}
    [Serializable] public class NeatGameObjectArray : NeatArrayWrapper<GameObject> {}


    public class NeatArrayWrapper<T> {
        public T GetRandom(T defaultValue) { return list.GetRandom<T>(defaultValue); }
        
        public T[] list;
        public int Length { get { return list.Length; } }
        public T this[int index] { get { return list[index]; } }
        public bool displayed;
        public static implicit operator T[](NeatArrayWrapper<T> c) { return c.list; }
    }
    public class NeatListWrapper<T> {
        public T GetRandom(T defaultValue) { return list.GetRandom<T>(defaultValue); }
            
        public List<T> list;
        public bool displayed;
        public int Count { get { return list.Count; } }
        public T this[int index] { get { return list[index]; } }
        public static implicit operator List<T>(NeatListWrapper<T> c) { return c.list; }
    }
}


