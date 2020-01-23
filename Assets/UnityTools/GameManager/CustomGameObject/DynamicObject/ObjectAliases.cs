
using UnityEngine;
using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;
using UnityEditor;

namespace UnityTools {
    [System.Serializable] public class ObjectAliasArray : NeatArrayWrapper<ObjectAlias> { }
    [System.Serializable] public class ObjectAlias {
        public string alias;
        public PrefabReference prefabRef;
        public Location location;
    }
    
    [CreateAssetMenu(menuName="Unity Tools/Object Aliases", fileName="ObjectAliases")]
    public class ObjectAliases : GameSettingsObject {
        [NeatArray] public ObjectAliasArray aliases;
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ObjectAlias))] class ObjectAliasDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position.y = GUITools.PropertyFieldAndHeightChange(position, property, "alias");
            position.y = GUITools.PropertyFieldAndHeightChange(position, property, "prefabRef");
            position.y = GUITools.PropertyFieldAndHeightChange(position, property, "location");
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float h = 0;
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("alias"), true);
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("prefabRef"), true);
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("location"), true);
            return h;
        }
    }
    #endif
}