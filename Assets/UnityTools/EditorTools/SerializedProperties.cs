// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


using UnityEditor;
using System;
using System.Reflection;
namespace UnityTools.EditorTools {

    public static class SerializedProperties 
    {
         public static System.Type GetType(SerializedProperty property)
         {
             Type targetObjectType = property.serializedObject.targetObject.GetType();
             FieldInfo fi = targetObjectType.GetFieldViaPath(property.propertyPath);
             return fi.FieldType;
         }
        
        public static FieldInfo GetFieldViaPath(this Type type, string path)
        {
            Type parentType = type;
            FieldInfo fi = type.GetField(path);
            string[] perDot = path.Split('.');
            
            foreach (string fieldName in perDot)
            {
                fi = parentType.GetField(fieldName);
                if (fi != null)
                    parentType = fi.FieldType;
                else
                    return null;
            }

            if (fi != null)
                return fi;

            else return null;
        }

    }
}
