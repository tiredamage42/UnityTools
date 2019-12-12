using System.Collections.Generic;
using System;
using UnityEngine;

using UnityEditor;

using Object = UnityEngine.Object;

namespace UnityTools.EditorTools {

    /*
        Draw project asset selection in a dropdown list

        press the button next to the field to refresh asset references
    */    
    public class AssetSelectionAttribute : PropertyAttribute {
        public Type type;   
        public bool useName;
        public virtual List<AssetSelectorElement> OnAssetsLoaded (List<AssetSelectorElement> originals) { return originals; }
        public AssetSelectionAttribute(Type type, bool useName=false) { 
            this.type = type; 
            this.useName = useName;
        }
    }

    public class AssetSelectorElement {

        public Object asset;
        public string displayName;
        public string assetName { get { return asset != null ? asset.name : null; } }

        public AssetSelectorElement(Object asset, string typeName) {
            this.asset = asset;
            this.displayName = asset != null ? asset.name : "[ Null ]";
        }    
    }

    
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(AssetSelectionAttribute), true)]
    class AssetSelectionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            AssetSelectionAttribute att = attribute as AssetSelectionAttribute;
            Type type = att.type;
            
            if (att.useName) {
                if (property.propertyType != SerializedPropertyType.String) {
                    Debug.LogWarning("Field :: " + property.displayName + " is not an string type, cannot draw names with asset selector");
                    EditorGUI.PropertyField(position, property, label);
                    return;
                }
                AssetSelector.DrawName(type, position, property, label, att.OnAssetsLoaded);
            }
            else {

                if (property.propertyType != SerializedPropertyType.ObjectReference) {
                    Debug.LogWarning("Field :: " + property.displayName + " is not an object reference type, cannot draw with asset selector");
                    EditorGUI.PropertyField(position, property, label);
                    return;
                }
                AssetSelector.Draw(type, position, property, label, att.OnAssetsLoaded);
            }
        }
    }

    public class AssetSelector {
        static Dictionary<Type, AssetSelector> allAssetSelectors = new Dictionary<Type, AssetSelector> ();
        static AssetSelector GetSelector (Type type) {
            AssetSelector selector;
            if (allAssetSelectors.TryGetValue(type, out selector)) {
                if (selector != null) {
                    return selector;
                }
            }
            allAssetSelectors[type] = new AssetSelector(type);
            return allAssetSelectors[type];
        }

        public delegate List<AssetSelectorElement> OnAssetsLoaded (List<AssetSelectorElement> assets);

        public static void Draw (Type type, SerializedProperty prop, GUIContent gui, OnAssetsLoaded onAssetsLoaded) {
            Draw(type, prop.objectReferenceValue, gui, onAssetsLoaded, 
                (picked) => {
                    prop.objectReferenceValue = picked;
                    prop.serializedObject.ApplyModifiedProperties();
                } 
            );
                
        }
        public static void Draw (Type type, Rect pos, SerializedProperty prop, GUIContent gui, OnAssetsLoaded onAssetsLoaded) {
            Draw(type, pos, prop.objectReferenceValue, gui, onAssetsLoaded, 
                (picked) => {
                    prop.objectReferenceValue = picked; 
                    prop.serializedObject.ApplyModifiedProperties();
                }         
            );
        }
        public static void Draw (Type type, Rect pos, Object current, GUIContent gui, OnAssetsLoaded onAssetsLoaded, Action<Object> onPicked) {
            GetSelector(type).Draw(pos, current, gui, onAssetsLoaded, onPicked);
        }
        public static void Draw (Type type, Object current, GUIContent gui, OnAssetsLoaded onAssetsLoaded, Action<Object> onPicked) {
            GetSelector(type).Draw(current, gui, onAssetsLoaded, onPicked);
        }

        public static void DrawName (Type type, SerializedProperty prop, GUIContent gui, OnAssetsLoaded onAssetsLoaded) {
            DrawName(type, prop.stringValue, gui, onAssetsLoaded, 
                (picked) => {
                    prop.stringValue = picked ;
                    prop.serializedObject.ApplyModifiedProperties();    
                }
            );
        }
        public static void DrawName (Type type, Rect pos, SerializedProperty prop, GUIContent gui, OnAssetsLoaded onAssetsLoaded) {
            DrawName(type, pos, prop.stringValue, gui, onAssetsLoaded, 
                (picked) => {
                    prop.stringValue = picked;
                    prop.serializedObject.ApplyModifiedProperties();
                } 
            );
        }
        public static void DrawName (Type type, Rect pos, string current, GUIContent gui, OnAssetsLoaded onAssetsLoaded, Action<string> onPicked) {
            GetSelector(type).DrawName(pos, current, gui, onAssetsLoaded, onPicked);
        }
        public static void DrawName (Type type, string current, GUIContent gui, OnAssetsLoaded onAssetsLoaded, Action<string> onPicked) {
            GetSelector(type).DrawName(current, gui, onAssetsLoaded, onPicked);
        }

        const string nullString = "[ Null ]";
        Type type;
        static GUIContent _pingGUI;
        static GUIContent pingGUI {
            get {
                if (_pingGUI == null) _pingGUI = BuiltInIcons.GetIcon("ViewToolMove", "Select Asset");
                return _pingGUI;
            }
        }
        public AssetSelector (Type type) {
            this.type = type;
        }

        List<AssetSelectorElement> BuildElements(OnAssetsLoaded onAssetsLoaded, bool addNull, bool logToConsole) {
            List<Object> assets = AssetTools.FindAssetsByType(type, logToConsole);

            List<AssetSelectorElement> r = new List<AssetSelectorElement>();
            
            for (int i = 0; i < assets.Count; i++)
                r.Add(new AssetSelectorElement(assets[i], type.Name));
            
            if (onAssetsLoaded != null)
                r = onAssetsLoaded(r);
            
            if (addNull)
                r.Insert(0, new AssetSelectorElement(null, type.Name));
                
            return r;
        }

        void BuildMenu<T> (T current, OnAssetsLoaded onAssetsLoaded, Action<T> onPicked, Func<AssetSelectorElement, T> getObj) where T : class {
            List<AssetSelectorElement> elements = BuildElements(onAssetsLoaded, true, logToConsole: false);
            GenericMenu menu = new GenericMenu();
            for (int i =0 ; i < elements.Count; i++) {
                T e = getObj (elements[i]);
                menu.AddItem (new GUIContent(elements[i].displayName), e == current, () => onPicked(e));
            }
            menu.ShowAsContext();
        }
        void DrawLabel (ref float x, float y, GUIContent gui) {
            if (!string.IsNullOrEmpty(gui.text)) {
                EditorGUI.LabelField(new Rect(x, y, EditorGUIUtility.labelWidth, GUITools.singleLineHeight), gui);
                x += EditorGUIUtility.labelWidth;
            }
        }
        void DrawLabel (GUIContent gui) {
            if (!string.IsNullOrEmpty(gui.text)) 
                EditorGUILayout.LabelField(gui, GUILayout.Width(EditorGUIUtility.labelWidth));
        }
        
        
        bool DrawButton (float x, Rect pos, GUIContent gui) {
            return GUITools.Button(x, pos.y, pos.width - ((x - pos.x) + GUITools.iconButtonWidth), GUITools.singleLineHeight, gui, GUITools.popup);
        }

        GUIContent GetGUI (Object current) { return new GUIContent(current != null ? current.name : nullString); }
        GUIContent GetGUI (string current) { return new GUIContent(current != null ? current : nullString); }

        void Draw (Rect pos, Object current, GUIContent gui, OnAssetsLoaded onAssetsLoaded, Action<Object> onPicked) {    
            float x = pos.x;
            DrawLabel (ref x, pos.y, gui);
            if (DrawButton (x, pos, GetGUI(current)))
                BuildMenu (current, onAssetsLoaded, onPicked, (e) => e.asset);
            if (GUITools.IconButton(pos.x + (pos.width - GUITools.iconButtonWidth), pos.y, pingGUI))
                EditorGUIUtility.PingObject(current);
        }

        void Draw (Object current, GUIContent gui, OnAssetsLoaded onAssetsLoaded, Action<Object> onPicked) {
            EditorGUILayout.BeginHorizontal();
            DrawLabel(gui);            
            if (GUITools.Button(GetGUI(current), GUITools.popup))
                BuildMenu (current, onAssetsLoaded, onPicked, (e) => e.asset);
            if (GUITools.IconButton(pingGUI))
                EditorGUIUtility.PingObject(current);   
            EditorGUILayout.EndHorizontal();
        }
        void PingAssetWithName (string name, OnAssetsLoaded onAssetsLoaded) {
            if (name != null) {
                List<AssetSelectorElement> elements = BuildElements(onAssetsLoaded, false, logToConsole: false);
                for (int i =0 ; i < elements.Count; i++) {
                    if (elements[i].asset.name == name) {
                        EditorGUIUtility.PingObject(elements[i].asset);
                        break;
                    }
                }
            }
        }
        void DrawName (Rect pos, string current, GUIContent gui, OnAssetsLoaded onAssetsLoaded, Action<string> onPicked) {
            float x = pos.x;
            DrawLabel (ref x, pos.y, gui);
            if (DrawButton (x, pos, GetGUI(current)))
                BuildMenu (current, onAssetsLoaded, onPicked, (e) => e.assetName);
            if (GUITools.IconButton(pos.x + (pos.width - GUITools.iconButtonWidth), pos.y, pingGUI)) 
                PingAssetWithName(current, onAssetsLoaded);
        }
        void DrawName (string current, GUIContent gui, OnAssetsLoaded onAssetsLoaded, Action<string> onPicked) {
            EditorGUILayout.BeginHorizontal();
            DrawLabel(gui);
            if (GUITools.Button(GetGUI(current), GUITools.popup))
                BuildMenu (current, onAssetsLoaded, onPicked, (e) => e.assetName);
            if (GUITools.IconButton( pingGUI )) 
                PingAssetWithName(current, onAssetsLoaded);
            EditorGUILayout.EndHorizontal();
            
        }
    }
#endif

}
