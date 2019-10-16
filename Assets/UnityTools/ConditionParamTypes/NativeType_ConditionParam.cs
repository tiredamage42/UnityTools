// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;



using UnityEditor;
namespace UnityTools {


    public class NativeType_ConditionParam : ConditionsParameter
    {
        public enum NativeType {
            Float, Int, Bool, String
        };
        public float nValue;
        public bool bValue;
        public string sValue;
        public NativeType type;

        public override void DrawGUI(Rect pos) {
            float hWidth = pos.width * .5f;
            type = (NativeType)EditorGUI.EnumPopup(new Rect(pos.x, pos.y, hWidth, pos.height), type);
            pos = new Rect(pos.x + hWidth, pos.y, hWidth, pos.height);
            switch (type) {
                case NativeType.Float:
                    nValue = EditorGUI.FloatField(pos, nValue);
                    break;
                case NativeType.Int:
                    nValue = EditorGUI.IntField(pos, (int)nValue);
                    break;
                case NativeType.Bool:
                    bValue = EditorGUI.Toggle(pos, bValue);
                    break;
                case NativeType.String:
                    sValue = EditorGUI.TextField(pos, sValue);
                    break;
            }
                
        }
        public override object GetParamObject() {
            switch (type) {
                case NativeType.Float: return nValue;
                case NativeType.Int: return nValue;
                case NativeType.Bool: return bValue;
                case NativeType.String: return sValue;
            }

            return nValue;
        }
    }
}
