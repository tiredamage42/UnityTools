using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;
using System.Linq;

using System.Reflection;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace UnityTools {

    public static class SystemTools 
    {   
        
        public static Type[] FindDerivedTypes(this Type type, bool includeSelf)
        {
            return type.Assembly.GetTypes().Where(t => (t != type || includeSelf) && type.IsAssignableFrom(t)).ToArray();
        }

        public static Type[] FindAllDerivedTypes(this Type type)
        {
            Assembly assembly = Assembly.LoadFrom(type.Assembly.Location);
            Type[] types = assembly.GetTypes();
            List<Type> results = new List<Type>();
            GetAllDerivedTypesRecursively(types, type, ref results);
            return results.Where( t => !t.IsAbstract ).ToArray();
        }

        static void GetAllDerivedTypesRecursively(Type[] types, Type type1, ref List<Type> results) {
            if (type1.IsGenericType)
                GetDerivedFromGeneric(types, type1, ref results);
            else
                GetDerivedFromNonGeneric(types, type1, ref results);
        }
        static void GetDerivedFromGeneric(Type[] types, Type type, ref List<Type> results) {
            var derivedTypes = types.Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == type);
            results.AddRange(derivedTypes);
            foreach (Type derivedType in derivedTypes)
                GetAllDerivedTypesRecursively(types, derivedType, ref results);
        }
        static void GetDerivedFromNonGeneric(Type[] types, Type type, ref List<Type> results) {
            var derivedTypes = types.Where(t => t != type && type.IsAssignableFrom(t));
            results.AddRange(derivedTypes);
            foreach (Type derivedType in derivedTypes)
                GetAllDerivedTypesRecursively(types, derivedType, ref results);
        } 

        /*
            gets a string hash code that will persist between runs
        */
        public static int GetPersistentHashCode(this string str)
        {
            /*
                'unchecked' keyword disables overflow-checking for the integer arithmetic done inside the function. 
                If the function was executed inside a checked context, you might get an OverflowException at runtime
            */
            unchecked
            {
                
                // int hash1 = 5381;
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;
                // for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    // if (i == str.Length - 1 || str[i+1] == '\0')
                    if (i == str.Length - 1)
                        break;

                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }
                return hash1 + (hash2 * 1566083941);
            }
        }

        public static bool TryParse (string parameter, Type type, out object value) {
            value = null;
            
            if (string.IsNullOrEmpty(parameter))
                return false;

            if (type == typeof(char)) {
                // maybe check length == 1
                value = parameter[0];
                return true;
            }
            if (type == typeof(string)) {
                value = parameter;
                return true;
            }
            if (type == typeof(object)) {
                value = parameter as object;
                return true;
            }
            // if (type == typeof(Type)) {
            //     value = Type.GetType(parameter);
            //     return true;
            // }

            if (type.IsEnum) {
                try {
                    value = Enum.Parse(type, parameter, true);
                }
                catch {
                    value = null;
                }
                return value != null;
            }
            
            if (type == typeof(bool)) {
                string l = parameter.ToLower();
                if (parameter != "1" && parameter != "0" && l != "true" && l != "false")
                    return false;
                
                value = parameter == "1" || l == "true";
                return true;
            }

            MethodInfo m = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).Where(x => x.Name == "TryParse").First();    
            if (m == null) {
                Debug.LogError(type.Name + " doesnt have Try Parse...");
                return false;
            }

            object[] parameters = new object[] { parameter, null };
            if ((bool)m.Invoke (null, parameters)) {
                value = parameters[1];
                return true;
            }
            Debug.LogError(type.Name + " couldnt parse: " + parameter);
            return false;
        }

        public static T StringToEnum<T>(string value, T defValue)
		{
			if(string.IsNullOrEmpty(value))
				return defValue;
            
            try {
				return (T)Enum.Parse(typeof(T), value, true);
			}
			catch {
				return defValue;
			}
		}

		
        public static void SaveToFile (object obj, string filePath) {
            using (FileStream file = File.Create(filePath))
            {
                using (GZipStream compressed = new GZipStream(file, CompressionMode.Compress))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        new BinaryFormatter().Serialize(ms, obj);
                        byte[] bytesToCompress = ms.ToArray();
                        compressed.Write(bytesToCompress, 0, bytesToCompress.Length);
                    }
                }
            }
        }

        public static object LoadFromFile (string filePath) {
            object obj = null;
            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                using (GZipStream decompressed = new GZipStream(file, CompressionMode.Decompress))
                {
                    obj = new BinaryFormatter().Deserialize(decompressed);
                }
            }
            return obj;
        }
        

        public static double ConvertSize (double sizeInBytes, out FileSize size) {
            if (sizeInBytes < 1024) {
                size = FileSize.B;
                return sizeInBytes;
            }
            else if (sizeInBytes < 1048576) {
                size = FileSize.KB;
                return sizeInBytes / 1024;
            }
            else if (sizeInBytes < 1073741824) {
                size = FileSize.MB;
                return sizeInBytes / 1048576;
            }
            else {
                size = FileSize.GB;
                return sizeInBytes / 1073741824;
            } 
        }
    }
    public enum FileSize { B, KB, MB, GB }

}
