using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace UnityTools {

    public class SystemTools 
    {   
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
