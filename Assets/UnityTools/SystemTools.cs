using UnityEngine;

using System;
namespace UnityTools {

    public class SystemTools : MonoBehaviour
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
        
    }
}
