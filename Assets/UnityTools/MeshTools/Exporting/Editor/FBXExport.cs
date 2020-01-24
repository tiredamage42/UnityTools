using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityTools.MeshTools
{
	public class FBXExport : Editor 
	{
        [MenuItem("Assets/FBX Exporter/Create Object With Procedural Texture", false, 43)]
        public static void CreateObject()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Texture2D texture = new Texture2D(128, 128);
            for (int x = 0; x < 128; ++x)
                for (int y = 0; y < 128; ++y)
                    texture.SetPixel(x, y, (x-64)*(x-64) + (y-64)*(y-64) < 1000 ? Color.white : Color.black);
            texture.Apply();
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = texture;
            cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }



		// Dropdown
		[MenuItem("GameObject/FBX Exporter/Only GameObject", false, 40)]
		public static void ExportDropdownGameObjectToFBX()
		{
			ExportCurrentGameObject(false, false);
		}

		[MenuItem("GameObject/FBX Exporter/With new Materials", false, 41)]
		public static void ExportDropdownGameObjectAndMaterialsToFBX()
		{
			ExportCurrentGameObject(true, false);
		}

		[MenuItem("GameObject/FBX Exporter/With new Materials and Textures", false, 42)]
		public static void ExportDropdownGameObjectAndMaterialsTexturesToFBX()
		{
			ExportCurrentGameObject(true, true);
		}

		// Assets
		[MenuItem("Assets/FBX Exporter/Only GameObject", false, 30)]
		public static void ExportGameObjectToFBX()
		{
			ExportCurrentGameObject(false, false);
		}
		
		[MenuItem("Assets/FBX Exporter/With new Materials", false, 31)]
		public static void ExportGameObjectAndMaterialsToFBX()
		{
			ExportCurrentGameObject(true, false);
		}
		
		[MenuItem("Assets/FBX Exporter/With new Materials and Textures", false, 32)]
		public static void ExportGameObjectAndMaterialsTexturesToFBX()
		{
			ExportCurrentGameObject(true, true);
		}

		
		static void ExportCurrentGameObject(bool copyMaterials, bool copyTextures)
		{	
			ExportGameObject(Selection.activeObject as GameObject, copyMaterials, copyTextures);
		}

		public static string ExportGameObject(GameObject gameObj, bool copyMaterials, bool copyTextures)
		{
			if(gameObj == null)
			{
				EditorUtility.DisplayDialog("Object is null", "Please select any GameObject to Export to FBX", "Okay");
				return null;
			}
			
			string newPath = GetNewPath(gameObj);
			if(newPath != null && newPath.Length != 0)
			{
				if(FBXExporter.ExportGameObjToFBX(gameObj, newPath, copyMaterials, copyTextures))
					return newPath;
				else
					EditorUtility.DisplayDialog("Warning", "The extension probably wasn't an FBX file, could not export.", "Okay");
			}
			return null;
		}
		
		static string GetNewPath(GameObject gameObject)
		{
			// NOTE: This must return a path with the starting "Assets/" or else textures won't copy right
			string newPath = newPath = EditorUtility.SaveFilePanelInProject("Export FBX File", gameObject.name + ".fbx", "fbx", "Export " + gameObject.name + " GameObject to a FBX file");
            int assetsIndex = newPath.IndexOf("Assets");
			if(assetsIndex < 0)
				return null;
			if(assetsIndex > 0)
				newPath = newPath.Remove(0, assetsIndex);
			return newPath;
		}
	}
}
