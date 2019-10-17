using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;


using UnityEngine;
namespace UnityTools {

    public static class SystemTools 
    {   
        static bool CheckParametersForMethod (MethodInfo method, object[] suppliedParameters) {
            ParameterInfo[] methodParams = method.GetParameters();

            if (methodParams.Length != suppliedParameters.Length)
                return false;

            for (int p = 0; p < methodParams.Length; p++) {
                Type methodType = methodParams[p].ParameterType;
                Type suppliedType = suppliedParameters[p].GetType();
                if (suppliedType != methodType && !suppliedType.IsSubclassOf(methodType)) 
                    return false;
            }
            return true;
        }

        public static bool TryAndCallMethod (Type type, BindingFlags flags, string callMethod, object instance, object[] parameters, out float value, bool debugError, string errorPrefix) {
            MethodInfo[] allMethods = type.GetMethods(flags);
            for (int m = 0; m < allMethods.Length; m++) {
                MethodInfo method = allMethods[m];
                if (method.Name == callMethod) {
                    if (CheckParametersForMethod ( method, parameters )) {
                        if (method.ReturnType == typeof(float) || method.ReturnType == typeof(int)) {
                            value = (float)method.Invoke(instance, parameters );
                            return true;
                        }
                        else if (method.ReturnType == typeof(bool)) {
                            value = ((bool)method.Invoke(instance, parameters )) ? 1f : 0f;
                            return true;
                        }
                    }
                }
            }
            if (debugError) {
                string typeNames = "";
                for (int i = 0; i < parameters.Length; i++) typeNames += parameters[i].GetType().Name + ", ";
                Debug.LogError(errorPrefix + " does not contain a call method: '" + callMethod + "' with parameter types: (" + typeNames + ")");
            }
            value = 0;
            return false;
        }

        public static bool CallStaticMethod (string callClassAndMethod, object[] parameters, out float value) {
            value = 0;

            int idx = callClassAndMethod.LastIndexOf('.');
            if (idx == -1) {
                Debug.LogError("Class and method string '" + callClassAndMethod + "' is not in format: Class.Method");
                return false;
            }
            
            string className = callClassAndMethod.Substring(0, idx);
            string callMethod = callClassAndMethod.Substring(idx + 1);

            Type classType = Type.GetType(className, false);

            if (classType == null) {
                Debug.LogError("Couldnt find class '" + className + "' in current assembly!");
                return false;
            }
            return TryAndCallMethod ( classType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, callMethod, null, parameters, out value, true, "Class: " + classType.FullName);
        }

        public static Type[] FindDerivedTypes(this Type type, bool includeSelf)
        {
            return type.Assembly.GetTypes().Where(t => (t != type || includeSelf) && type.IsAssignableFrom(t)).ToArray();
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
        
    }
}
