
// using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityTools.MeshTools
{
    public class CustomImportSettings : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!FBXExporter.shouldCopyMaterials)
            {
                ModelImporter importer = assetImporter as ModelImporter;
                importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
            }
        }
    }
        
	public class FBXExporter
	{
        public static bool shouldCopyMaterials;

		public static bool ExportGameObjToFBX(GameObject gameObj, string newPath, bool copyMaterials = false, bool copyTextures = false)
		{
			// Check to see if the extension is right
			if (Path.GetExtension(newPath).ToLower() != ".fbx")
			{
				Debug.LogError("The end of the path wasn't \".fbx\"");
				return false;
			}

			if(copyMaterials)
				CopyComplexMaterialsToPath(gameObj, newPath, copyTextures);

			string buildMesh = MeshToString(gameObj, newPath, copyMaterials, copyTextures);

			if(File.Exists(newPath))
				File.Delete(newPath);

			File.WriteAllText(newPath, buildMesh);

#if UNITY_EDITOR
			// Import the model properly so it looks for the material instead of by the texture name
			
            // Temporarily enables a custom importer when refreshing the asset to get around materisl being imported.
 			shouldCopyMaterials = copyMaterials;

            // TODO: By calling refresh, it imports the model with the wrong materials, but we can't find the model to import without
			// refreshing the database. A chicken and the egg issue
			AssetDatabase.Refresh();

            shouldCopyMaterials = false;


			string stringLocalPath = newPath.Remove(0, newPath.LastIndexOf("/Assets") + 1);

			ModelImporter modelImporter = ModelImporter.GetAtPath(stringLocalPath) as ModelImporter;
			if(modelImporter != null)
			{
				ModelImporterMaterialName modelImportOld = modelImporter.materialName;
				modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;
#if UNITY_5_1
                modelImporter.normalImportMode = ModelImporterTangentSpaceMode.Import;
#else
                modelImporter.importNormals = ModelImporterNormals.Import;
#endif
                if (copyMaterials == false)
					modelImporter.materialSearch = ModelImporterMaterialSearch.Everywhere;
				
				AssetDatabase.ImportAsset(stringLocalPath, ImportAssetOptions.ForceUpdate);
			}
			else
			{
				Debug.Log("Model Importer is null and can't import");
			}

			AssetDatabase.Refresh(); 
#endif
            return true;
		}

		
		public static long GetRandomFBXId()
		{
			return System.BitConverter.ToInt64(System.Guid.NewGuid().ToByteArray(), 0);
		}

		public static string MeshToString (GameObject gameObj, string newPath, bool copyMaterials = false, bool copyTextures = false)
		{
			StringBuilder sb = new StringBuilder();
			
			StringBuilder objectProps = new StringBuilder();
			objectProps.AppendLine("; Object properties");
			objectProps.AppendLine(";------------------------------------------------------------------");
			objectProps.AppendLine("");
			objectProps.AppendLine("Objects:  {");
			
			StringBuilder objectConnections = new StringBuilder();
			objectConnections.AppendLine("; Object connections");
			objectConnections.AppendLine(";------------------------------------------------------------------");
			objectConnections.AppendLine("");
			objectConnections.AppendLine("Connections:  {");
			objectConnections.AppendLine("\t");

			Material[] materials = new Material[0];

			// First finds all unique materials and compiles them (and writes to the object connections)
			string materialsObjectSerialized = "";
			string materialConnectionsSerialized = "";
			FBXUnityMaterialGetter.GetAllMaterialsToString(gameObj, newPath, copyMaterials, copyTextures, out materials, out materialsObjectSerialized, out materialConnectionsSerialized);

			// Run recursive FBX Mesh grab over the entire gameobject
			FBXUnityMeshGetter.GetMeshToString(gameObj, materials, ref objectProps, ref objectConnections);

			// write the materials to the objectProps here. Should not do it in the above as it recursive.

			objectProps.Append(materialsObjectSerialized);
			objectConnections.Append(materialConnectionsSerialized);

			// Close up both builders;
			objectProps.AppendLine("}");
			objectConnections.AppendLine("}");

			
			// ========= Create header ========
			
			// Intro
			sb.AppendLine("; FBX 7.3.0 project file");
			sb.AppendLine("; Copyright (C) 1997-2010 Autodesk Inc. and/or its licensors.");
			sb.AppendLine("; All rights reserved.");
			sb.AppendLine("; ----------------------------------------------------");
			sb.AppendLine();
			
			// The header
			sb.AppendLine("FBXHeaderExtension:  {");
			sb.AppendLine("\tFBXHeaderVersion: 1003");
			sb.AppendLine("\tFBXVersion: 7300");

			// Creationg Date Stamp
			System.DateTime currentDate = System.DateTime.Now;
			sb.AppendLine("\tCreationTimeStamp:  {");
			sb.AppendLine("\t\tVersion: 1000");
			sb.AppendLine("\t\tYear: " + currentDate.Year);
			sb.AppendLine("\t\tMonth: " + currentDate.Month);
			sb.AppendLine("\t\tDay: " + currentDate.Day);
			sb.AppendLine("\t\tHour: " + currentDate.Hour);
			sb.AppendLine("\t\tMinute: " + currentDate.Minute);
			sb.AppendLine("\t\tSecond: " + currentDate.Second);
			sb.AppendLine("\t\tMillisecond: " + currentDate.Millisecond);
			sb.AppendLine("\t}");
			
			// Info on the Creator
			sb.AppendLine("\tCreator: \"" + "Unity FBX Exporter" + "\"");
			sb.AppendLine("\tSceneInfo: \"SceneInfo::GlobalInfo\", \"UserData\" {");
			sb.AppendLine("\t\tType: \"UserData\"");
			sb.AppendLine("\t\tVersion: 100");
			sb.AppendLine("\t\tMetaData:  {");
			sb.AppendLine("\t\t\tVersion: 100");
			sb.AppendLine("\t\t\tTitle: \"\"");
			sb.AppendLine("\t\t\tSubject: \"\"");
			sb.AppendLine("\t\t\tAuthor: \"\"");
			sb.AppendLine("\t\t\tKeywords: \"\"");
			sb.AppendLine("\t\t\tRevision: \"\"");
			sb.AppendLine("\t\t\tComment: \"\"");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t\tProperties70:  {");

			// Information on how this item was originally generated
			string documentInfoPaths = Application.dataPath + newPath + ".fbx";
			sb.AppendLine("\t\t\tP: \"DocumentUrl\", \"KString\", \"Url\", \"\", \"" + documentInfoPaths + "\"");
			sb.AppendLine("\t\t\tP: \"SrcDocumentUrl\", \"KString\", \"Url\", \"\", \"" + documentInfoPaths + "\"");
			sb.AppendLine("\t\t\tP: \"Original\", \"Compound\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"Original|ApplicationVendor\", \"KString\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"Original|ApplicationName\", \"KString\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"Original|ApplicationVersion\", \"KString\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"Original|DateTime_GMT\", \"DateTime\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"Original|FileName\", \"KString\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"LastSaved\", \"Compound\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationVendor\", \"KString\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationName\", \"KString\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationVersion\", \"KString\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t\tP: \"LastSaved|DateTime_GMT\", \"DateTime\", \"\", \"\", \"\"");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			sb.AppendLine("}");
			
			// The Global information
			sb.AppendLine("GlobalSettings:  {");
			sb.AppendLine("\tVersion: 1000");
			sb.AppendLine("\tProperties70:  {");
			sb.AppendLine("\t\tP: \"UpAxis\", \"int\", \"Integer\", \"\",1");
			sb.AppendLine("\t\tP: \"UpAxisSign\", \"int\", \"Integer\", \"\",1");
			sb.AppendLine("\t\tP: \"FrontAxis\", \"int\", \"Integer\", \"\",2");
			sb.AppendLine("\t\tP: \"FrontAxisSign\", \"int\", \"Integer\", \"\",1");
			sb.AppendLine("\t\tP: \"CoordAxis\", \"int\", \"Integer\", \"\",0");
			sb.AppendLine("\t\tP: \"CoordAxisSign\", \"int\", \"Integer\", \"\",1");
			sb.AppendLine("\t\tP: \"OriginalUpAxis\", \"int\", \"Integer\", \"\",-1");
			sb.AppendLine("\t\tP: \"OriginalUpAxisSign\", \"int\", \"Integer\", \"\",1");
			sb.AppendLine("\t\tP: \"UnitScaleFactor\", \"double\", \"Number\", \"\",100"); // NOTE: This sets the resize scale upon import
			sb.AppendLine("\t\tP: \"OriginalUnitScaleFactor\", \"double\", \"Number\", \"\",100");
			sb.AppendLine("\t\tP: \"AmbientColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
			sb.AppendLine("\t\tP: \"DefaultCamera\", \"KString\", \"\", \"\", \"Producer Perspective\"");
			sb.AppendLine("\t\tP: \"TimeMode\", \"enum\", \"\", \"\",11");
			sb.AppendLine("\t\tP: \"TimeSpanStart\", \"KTime\", \"Time\", \"\",0");
			sb.AppendLine("\t\tP: \"TimeSpanStop\", \"KTime\", \"Time\", \"\",479181389250");
			sb.AppendLine("\t\tP: \"CustomFrameRate\", \"double\", \"Number\", \"\",-1");
			sb.AppendLine("\t}");
			sb.AppendLine("}");
			
			// The Object definations
			sb.AppendLine("; Object definitions");
			sb.AppendLine(";------------------------------------------------------------------");
			sb.AppendLine("");
			sb.AppendLine("Definitions:  {");
			sb.AppendLine("\tVersion: 100");
			sb.AppendLine("\tCount: 4");

			sb.AppendLine("\tObjectType: \"GlobalSettings\" {");
			sb.AppendLine("\t\tCount: 1");
			sb.AppendLine("\t}");


			sb.AppendLine("\tObjectType: \"Model\" {");
			sb.AppendLine("\t\tCount: 1"); // TODO figure out if this count matters
			sb.AppendLine("\t\tPropertyTemplate: \"FbxNode\" {");
			sb.AppendLine("\t\t\tProperties70:  {");
			sb.AppendLine("\t\t\t\tP: \"QuaternionInterpolate\", \"enum\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationOffset\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"RotationPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"ScalingOffset\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"ScalingPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"TranslationActive\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"TranslationMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"TranslationMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"TranslationMinX\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"TranslationMinY\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"TranslationMinZ\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"TranslationMaxX\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"TranslationMaxY\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"TranslationMaxZ\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationOrder\", \"enum\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationSpaceForLimitOnly\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationStiffnessX\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationStiffnessY\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationStiffnessZ\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"AxisLen\", \"double\", \"Number\", \"\",10");
			sb.AppendLine("\t\t\t\tP: \"PreRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"PostRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"RotationActive\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"RotationMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"RotationMinX\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationMinY\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationMinZ\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationMaxX\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationMaxY\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"RotationMaxZ\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"ScalingActive\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"ScalingMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",1,1,1");
			sb.AppendLine("\t\t\t\tP: \"ScalingMinX\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"ScalingMinY\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"ScalingMinZ\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"ScalingMaxX\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"ScalingMaxY\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"ScalingMaxZ\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"GeometricTranslation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"GeometricRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"GeometricScaling\", \"Vector3D\", \"Vector\", \"\",1,1,1");
			sb.AppendLine("\t\t\t\tP: \"MinDampRangeX\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MinDampRangeY\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MinDampRangeZ\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MaxDampRangeX\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MaxDampRangeY\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MaxDampRangeZ\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MinDampStrengthX\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MinDampStrengthY\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MinDampStrengthZ\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthX\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthY\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthZ\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"PreferedAngleX\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"PreferedAngleY\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"PreferedAngleZ\", \"double\", \"Number\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"LookAtProperty\", \"object\", \"\", \"\"");
			sb.AppendLine("\t\t\t\tP: \"UpVectorProperty\", \"object\", \"\", \"\"");
			sb.AppendLine("\t\t\t\tP: \"Show\", \"bool\", \"\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"NegativePercentShapeSupport\", \"bool\", \"\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",-1");
			sb.AppendLine("\t\t\t\tP: \"Freeze\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"LODBox\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",1,1,1");
			sb.AppendLine("\t\t\t\tP: \"Visibility\", \"Visibility\", \"\", \"A\",1");
			sb.AppendLine("\t\t\t\tP: \"Visibility Inheritance\", \"Visibility Inheritance\", \"\", \"\",1");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			
			// The geometry, this is IMPORTANT
			sb.AppendLine("\tObjectType: \"Geometry\" {");
			sb.AppendLine("\t\tCount: 1"); // TODO - this must be set by the number of items being placed.
			sb.AppendLine("\t\tPropertyTemplate: \"FbxMesh\" {");
			sb.AppendLine("\t\t\tProperties70:  {");
			sb.AppendLine("\t\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",0.8,0.8,0.8");
			sb.AppendLine("\t\t\t\tP: \"BBoxMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"BBoxMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"Primary Visibility\", \"bool\", \"\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"Casts Shadows\", \"bool\", \"\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"Receive Shadows\", \"bool\", \"\", \"\",1");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			
			// The materials that are being placed. Has to be simple I think
			sb.AppendLine("\tObjectType: \"Material\" {");
			sb.AppendLine("\t\tCount: 1");
			sb.AppendLine("\t\tPropertyTemplate: \"FbxSurfacePhong\" {");
			sb.AppendLine("\t\t\tProperties70:  {");
			sb.AppendLine("\t\t\t\tP: \"ShadingModel\", \"KString\", \"\", \"\", \"Phong\"");
			sb.AppendLine("\t\t\t\tP: \"MultiLayer\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"EmissiveColor\", \"Color\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"EmissiveFactor\", \"Number\", \"\", \"A\",1");
			sb.AppendLine("\t\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",0.2,0.2,0.2");
			sb.AppendLine("\t\t\t\tP: \"AmbientFactor\", \"Number\", \"\", \"A\",1");
			sb.AppendLine("\t\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",0.8,0.8,0.8");
			sb.AppendLine("\t\t\t\tP: \"DiffuseFactor\", \"Number\", \"\", \"A\",1");
			sb.AppendLine("\t\t\t\tP: \"Bump\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"NormalMap\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"BumpFactor\", \"double\", \"Number\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"TransparencyFactor\", \"Number\", \"\", \"A\",0");
			sb.AppendLine("\t\t\t\tP: \"DisplacementColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"DisplacementFactor\", \"double\", \"Number\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"VectorDisplacementColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"VectorDisplacementFactor\", \"double\", \"Number\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"SpecularColor\", \"Color\", \"\", \"A\",0.2,0.2,0.2");
			sb.AppendLine("\t\t\t\tP: \"SpecularFactor\", \"Number\", \"\", \"A\",1");
			sb.AppendLine("\t\t\t\tP: \"ShininessExponent\", \"Number\", \"\", \"A\",20");
			sb.AppendLine("\t\t\t\tP: \"ReflectionColor\", \"Color\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"ReflectionFactor\", \"Number\", \"\", \"A\",1");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");

			// Explanation of how textures work
			sb.AppendLine("\tObjectType: \"Texture\" {");
			sb.AppendLine("\t\tCount: 2"); // TODO - figure out if this texture number is important
			sb.AppendLine("\t\tPropertyTemplate: \"FbxFileTexture\" {");
			sb.AppendLine("\t\t\tProperties70:  {");
			sb.AppendLine("\t\t\t\tP: \"TextureTypeUse\", \"enum\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"Texture alpha\", \"Number\", \"\", \"A\",1");
			sb.AppendLine("\t\t\t\tP: \"CurrentMappingType\", \"enum\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"WrapModeU\", \"enum\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"WrapModeV\", \"enum\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"UVSwap\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"PremultiplyAlpha\", \"bool\", \"\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"Translation\", \"Vector\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"Rotation\", \"Vector\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"Scaling\", \"Vector\", \"\", \"A\",1,1,1");
			sb.AppendLine("\t\t\t\tP: \"TextureRotationPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"TextureScalingPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			sb.AppendLine("\t\t\t\tP: \"CurrentTextureBlendMode\", \"enum\", \"\", \"\",1");
			sb.AppendLine("\t\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"default\"");
			sb.AppendLine("\t\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t\tP: \"UseMipMap\", \"bool\", \"\", \"\",0");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");

			sb.AppendLine("}");
			sb.AppendLine("");

			sb.Append(objectProps.ToString());
			sb.Append(objectConnections.ToString());

			return sb.ToString();
		}

		public static void CopyComplexMaterialsToPath(GameObject gameObj, string path, bool copyTextures, string texturesFolder = "/Textures", string materialsFolder = "/Materials")
		{
#if UNITY_EDITOR
			int folderIndex = path.LastIndexOf('/');
			path = path.Remove(folderIndex, path.Length - folderIndex);

			// 1. First create the directories that are needed
			string texturesPath = path + texturesFolder;
			string materialsPath = path + materialsFolder;
			
			if(Directory.Exists(path) == false)
				Directory.CreateDirectory(path);
			if(Directory.Exists(materialsPath) == false)
				Directory.CreateDirectory(materialsPath);


            // 2. Copy every distinct Material into the Materials folder
            //@cartzhang modify.As meshrender and skinnedrender is same level in inherit relation shape.
            // if not check,skinned render ,may lost some materials.
            Renderer[] meshRenderers = gameObj.GetComponentsInChildren<Renderer>();
			List<Material> everyMaterial = new List<Material>();
			for(int i = 0; i < meshRenderers.Length; i++)
			{
				for(int n = 0; n < meshRenderers[i].sharedMaterials.Length; n++)
					everyMaterial.Add(meshRenderers[i].sharedMaterials[n]);
				//Debug.Log(meshRenderers[i].gameObject.name);
			}

            Material[] everyDistinctMaterial = everyMaterial.Distinct().ToArray<Material>();
			everyDistinctMaterial = everyDistinctMaterial.OrderBy(o => o.name).ToArray<Material>();

			// Log warning if there are multiple assets with the same name
			for(int i = 0; i < everyDistinctMaterial.Length; i++)
			{
				for(int n = 0; n < everyDistinctMaterial.Length; n++)
				{
					if(i == n)
						continue;

					if(everyDistinctMaterial[i].name == everyDistinctMaterial[n].name)
					{
						Debug.LogErrorFormat("Two distinct materials {0} and {1} have the same name, this will not work with the FBX Exporter", everyDistinctMaterial[i], everyDistinctMaterial[n]);
						return;
					}
				}
			}

			List<string> everyMaterialName = new List<string>();
			// Structure of materials naming, is used when packaging up the package
			// PARENTNAME_ORIGINALMATNAME.mat
			for(int i = 0; i < everyDistinctMaterial.Length; i++)
			{
				string newName = gameObj.name + "_" + everyDistinctMaterial[i].name;
				string fullPath = materialsPath + "/" + newName + ".mat";

				if(File.Exists(fullPath))
					File.Delete(fullPath);

				if(CopyAndRenameAsset(everyDistinctMaterial[i], newName, materialsPath))
					everyMaterialName.Add(newName);
			}

			// 3. Go through newly moved materials and copy every texture and update the material
			AssetDatabase.Refresh();

			List<Material> allNewMaterials = new List<Material>();

			for (int i = 0; i < everyMaterialName.Count; i++) 
			{
				string assetPath = materialsPath;
				if(assetPath[assetPath.Length - 1] != '/')
					assetPath += "/";

				assetPath += everyMaterialName[i] + ".mat";

				Material sourceMat = (Material)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material));

				if(sourceMat != null)
					allNewMaterials.Add(sourceMat);
			}

			// Get all the textures from the mesh renderer

			if(copyTextures)
			{
				if(Directory.Exists(texturesPath) == false)
					Directory.CreateDirectory(texturesPath);

				AssetDatabase.Refresh();

				for(int i = 0; i < allNewMaterials.Count; i++)
					allNewMaterials[i] = CopyTexturesAndAssignCopiesToMaterial(allNewMaterials[i], texturesPath);
				
			}

			AssetDatabase.Refresh();
#endif
		}

		public static bool CopyAndRenameAsset(Object obj, string newName, string newFolderPath)
		{
#if UNITY_EDITOR
			string path = newFolderPath;
			
			if(path[path.Length - 1] != '/')
				path += "/";
			string testPath = path.Remove(path.Length - 1);

//			if(AssetDatabase.IsValidFolder(testPath) == false)
//			{
//				Debug.LogError("This folder does not exist " + testPath);
//				return false;
//			}

			string assetPath = AssetDatabase.GetAssetPath(obj);
			string extension = Path.GetExtension(assetPath);

			string newFileName = path + newName + extension;

			if(File.Exists(newFileName))
				return false;

			return AssetDatabase.CopyAsset(assetPath, newFileName);
#else
			return false;

#endif
		}

		private static string GetFileName(string path)
		{
			string fileName = path.ToString();
			fileName = fileName.Remove(0, fileName.LastIndexOf('/') + 1);
			return fileName;
		}

		private static Material CopyTexturesAndAssignCopiesToMaterial(Material material, string newPath)
		{


 			// Copy every texture in shader

 			for (int i = 0; i < ShaderUtil.GetPropertyCount (material.shader); i++)
 			{
 				GetTextureUpdateMaterialWithPath (material, ShaderUtil.GetPropertyName (material.shader, i), newPath);
 			}

			// if(material.shader.name == "Standard" || material.shader.name == "Standard (Specular setup)")
			// {
			// 	GetTextureUpdateMaterialWithPath(material, "_MainTex", newPath);

			// 	if(material.shader.name == "Standard")
			// 		GetTextureUpdateMaterialWithPath(material, "_MetallicGlossMap", newPath);

			// 	if(material.shader.name == "Standard (Specular setup)")
			// 		GetTextureUpdateMaterialWithPath(material, "_SpecGlossMap", newPath);

			// 	GetTextureUpdateMaterialWithPath(material, "_BumpMap", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_BumpMap", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_BumpMap", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_BumpMap", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_ParallaxMap", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_OcclusionMap", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_EmissionMap", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_DetailMask", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_DetailAlbedoMap", newPath);
			// 	GetTextureUpdateMaterialWithPath(material, "_DetailNormalMap", newPath);

			// }
			// else
			// 	Debug.LogError("WARNING: " + material.name + " is not a physically based shader, may not export to package correctly");

			return material;
		}

		/// <summary>
		/// Copies and renames the texture and assigns it to the material provided.
		/// NAME FORMAT: Material.name + textureShaderName
		/// </summary>
		private static void GetTextureUpdateMaterialWithPath(Material material, string textureShaderName, string newPath)
		{
			Texture textureInQ = material.GetTexture(textureShaderName);
			if(textureInQ != null)
			{
				string name = material.name + textureShaderName;
				
				Texture newTexture = (Texture)CopyAndRenameAssetReturnObject(textureInQ, name, newPath);
				if(newTexture != null)
					material.SetTexture(textureShaderName, newTexture);
			}
		}

		public static Object CopyAndRenameAssetReturnObject(Object obj, string newName, string newFolderPath)
		{
			#if UNITY_EDITOR
			string path = newFolderPath;
			
			if(path[path.Length - 1] != '/')
				path += "/";
			string testPath = path.Remove(path.Length - 1);
			
			if(System.IO.Directory.Exists(testPath) == false)
			{
				Debug.LogError("This folder does not exist " + testPath);
				return null;
			}
			
			string assetPath =  AssetDatabase.GetAssetPath(obj);
			string fileName = GetFileName(assetPath);
			string extension = fileName.Remove(0, fileName.LastIndexOf('.'));
			
			string newFullPathName = path + newName + extension;
			
			if(AssetDatabase.CopyAsset(assetPath, newFullPathName) == false)
				return null;
			
			AssetDatabase.Refresh();
			
			return AssetDatabase.LoadAssetAtPath(newFullPathName, typeof(Texture));
			#else
			return null;
			#endif
		}
	}
	public class FBXUnityMaterialGetter
	{

		/// <summary>
		/// Finds all materials in a gameobject and writes them to a string that can be read by the FBX writer
		/// </summary>
		/// <param name="gameObj">Parent GameObject being exported.</param>
		/// <param name="newPath">The path to export to.</param>
		/// <param name="materials">Materials which were written to this fbx file.</param>
		/// <param name="matObjects">The material objects to write to the file.</param>
		/// <param name="connections">The connections to write to the file.</param>
		public static void GetAllMaterialsToString(GameObject gameObj, string newPath, bool copyMaterials, bool copyTextures, out Material[] materials, out string matObjects, out string connections)
		{
			StringBuilder tempObjectSb = new StringBuilder();
			StringBuilder tempConnectionsSb = new StringBuilder();

            // Need to get all unique materials for the submesh here and then write them in
            //@cartzhang modify.As meshrender and skinnedrender is same level in inherit relation shape.
            // if not check,skinned render ,may lost some materials.
            Renderer[] meshRenders = gameObj.GetComponentsInChildren<Renderer>();
			
			List<Material> uniqueMaterials = new List<Material>();

			// Gets all the unique materials within this GameObject Hierarchy
			for(int i = 0; i < meshRenders.Length; i++)
			{
				for(int n = 0; n < meshRenders[i].sharedMaterials.Length; n++)
				{
					Material mat = meshRenders[i].sharedMaterials[n];
					
					if(uniqueMaterials.Contains(mat) == false && mat != null)
					{
						uniqueMaterials.Add(mat);
					}
				}
			}

            for (int i = 0; i < uniqueMaterials.Count; i++)
			{
				Material mat = uniqueMaterials[i];

				// We rename the material if it is being copied
				string materialName = mat.name;
				if(copyMaterials)
					materialName = gameObj.name + "_" + mat.name;

				int referenceId = Mathf.Abs(mat.GetInstanceID());

				tempObjectSb.AppendLine();
				tempObjectSb.AppendLine("\tMaterial: " + referenceId + ", \"Material::" + materialName + "\", \"\" {");
				tempObjectSb.AppendLine("\t\tVersion: 102");
				tempObjectSb.AppendLine("\t\tShadingModel: \"phong\"");
				tempObjectSb.AppendLine("\t\tMultiLayer: 0");
				tempObjectSb.AppendLine("\t\tProperties70:  {");


				// tempObjectSb.AppendFormat("\t\t\tP: \"Diffuse\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", mat.color.r, mat.color.g, mat.color.b);
				// tempObjectSb.AppendLine();
				// tempObjectSb.AppendFormat("\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",{0},{1},{2}", mat.color.r, mat.color.g, mat.color.b);
				// tempObjectSb.AppendLine();

                // expcetion of some shader might not have _Color

 				if (mat.HasProperty("_Color"))
 				{
 					tempObjectSb.AppendFormat("\t\t\tP: \"Diffuse\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", mat.color.r, mat.color.g, mat.color.b);
 					tempObjectSb.AppendLine();
 					tempObjectSb.AppendFormat("\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",{0},{1},{2}", mat.color.r, mat.color.g, mat.color.b);
 					tempObjectSb.AppendLine();
 				}
 				else
 				{
 					tempObjectSb.AppendFormat("\t\t\tP: \"Diffuse\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", Color.white.r, Color.white.g, Color.white.b);
 					tempObjectSb.AppendLine();
 					tempObjectSb.AppendFormat("\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",{0},{1},{2}", Color.white.r, Color.white.g, Color.white.b);
 					tempObjectSb.AppendLine();
 				}



				// TODO: Figure out if this property can be written to the FBX file
	//			if(mat.HasProperty("_MetallicGlossMap"))
	//			{
	//				Debug.Log("has metallic gloss map");
	//				Color color = mat.GetColor("_Color");
	//				tempObjectSb.AppendFormat("\t\t\tP: \"Specular\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", color.r, color.g, color.r);
	//				tempObjectSb.AppendLine();
	//				tempObjectSb.AppendFormat("\t\t\tP: \"SpecularColor\", \"ColorRGB\", \"Color\", \" \",{0},{1},{2}", color.r, color.g, color.b);
	//				tempObjectSb.AppendLine();
	//			}

				if(mat.HasProperty("_SpecColor"))
				{
					Color color = mat.GetColor("_SpecColor");
					tempObjectSb.AppendFormat("\t\t\tP: \"Specular\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", color.r, color.g, color.r);
					tempObjectSb.AppendLine();
					tempObjectSb.AppendFormat("\t\t\tP: \"SpecularColor\", \"ColorRGB\", \"Color\", \" \",{0},{1},{2}", color.r, color.g, color.b);
					tempObjectSb.AppendLine();
				}

				if(mat.HasProperty("_Mode"))
				{
					Color color = Color.white;

					switch((int)mat.GetFloat("_Mode"))
					{
					case 0: // Map is opaque

						break;

					case 1: // Map is a cutout
						//  TODO: Add option if it is a cutout
						break;

					case 2: // Map is a fade
						color = mat.GetColor("_Color");
						
						tempObjectSb.AppendFormat("\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",{0},{1},{2}", color.r, color.g, color.b);
						tempObjectSb.AppendLine();
						tempObjectSb.AppendFormat("\t\t\tP: \"Opacity\", \"double\", \"Number\", \"\",{0}", color.a);
						tempObjectSb.AppendLine();
						break;

					case 3: // Map is transparent
						color = mat.GetColor("_Color");

						tempObjectSb.AppendFormat("\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",{0},{1},{2}", color.r, color.g, color.b);
						tempObjectSb.AppendLine();
						tempObjectSb.AppendFormat("\t\t\tP: \"Opacity\", \"double\", \"Number\", \"\",{0}", color.a);
						tempObjectSb.AppendLine();
						break;
					}
				}

				// NOTE: Unity doesn't currently import this information (I think) from an FBX file.
				if(mat.HasProperty("_EmissionColor"))
				{
					Color color = mat.GetColor("_EmissionColor");

					tempObjectSb.AppendFormat("\t\t\tP: \"Emissive\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", color.r, color.g, color.b);
					tempObjectSb.AppendLine();

					float averageColor = (color.r + color.g + color.b) / 3f;

					tempObjectSb.AppendFormat("\t\t\tP: \"EmissiveFactor\", \"Number\", \"\", \"A\",{0}", averageColor);
					tempObjectSb.AppendLine();
				}

				// TODO: Add these to the file based on their relation to the PBR files
//				tempObjectSb.AppendLine("\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",0,0,0");
//				tempObjectSb.AppendLine("\t\t\tP: \"ShininessExponent\", \"Number\", \"\", \"A\",6.31179285049438");
//				tempObjectSb.AppendLine("\t\t\tP: \"Ambient\", \"Vector3D\", \"Vector\", \"\",0,0,0");
//				tempObjectSb.AppendLine("\t\t\tP: \"Shininess\", \"double\", \"Number\", \"\",6.31179285049438");
//				tempObjectSb.AppendLine("\t\t\tP: \"Reflectivity\", \"double\", \"Number\", \"\",0");

				tempObjectSb.AppendLine("\t\t}");
				tempObjectSb.AppendLine("\t}");

				string textureObjects;
				string textureConnections;

				SerializedTextures(gameObj, newPath, mat, materialName, copyTextures, out textureObjects, out textureConnections);

				tempObjectSb.Append(textureObjects);
				tempConnectionsSb.Append(textureConnections);
			}

			materials = uniqueMaterials.ToArray<Material>();

			matObjects = tempObjectSb.ToString();
			connections = tempConnectionsSb.ToString();
		}

		/// <summary>
		/// Serializes textures to FBX format.
		/// </summary>
		/// <param name="gameObj">Parent GameObject being exported.</param>
		/// <param name="newPath">The path to export to.</param>
		/// <param name="materials">Materials that holds all the textures.</param>
		/// <param name="matObjects">The string with the newly serialized texture file.</param>
		/// <param name="connections">The string to connect this to the  material.</param>
		private static void SerializedTextures(GameObject gameObj, string newPath, Material material, string materialName, bool copyTextures, out string objects, out string connections)
		{
			// TODO: FBX import currently only supports Diffuse Color and Normal Map
			// Because it is undocumented, there is no way to easily find out what other textures
			// can be attached to an FBX file so it is imported into the PBR shaders at the same time.
			// Also NOTE, Unity 5.1.2 will import FBX files with legacy shaders. This is fix done
			// in at least 5.3.4.

			StringBuilder objectsSb = new StringBuilder();
			StringBuilder connectionsSb = new StringBuilder();

			int materialId = Mathf.Abs(material.GetInstanceID());

			Texture mainTexture = material.GetTexture("_MainTex");

			string newObjects = null;
			string newConnections = null;

			// Serializeds the Main Texture, one of two textures that can be stored in FBX's sysytem
			if(mainTexture != null)
			{
				SerializeOneTexture(gameObj, newPath, material, materialName, materialId, copyTextures, "_MainTex", "DiffuseColor", out newObjects, out newConnections);
				objectsSb.AppendLine(newObjects);
				connectionsSb.AppendLine(newConnections);
			}

			if(SerializeOneTexture(gameObj, newPath, material, materialName, materialId, copyTextures, "_BumpMap", "NormalMap", out newObjects, out newConnections))
			{
				objectsSb.AppendLine(newObjects);
				connectionsSb.AppendLine(newConnections);
			}

			connections = connectionsSb.ToString();
			objects = objectsSb.ToString();
		}

		private static bool SerializeOneTexture(GameObject gameObj, 
		                                        string newPath, 
		                                        Material material, 
		                                        string materialName,
		                                        int materialId,
		                                        bool copyTextures, 
		                                        string unityExtension, 
		                                        string textureType, 
		                                        out string objects, 
		                                        out string connections)
		{
			StringBuilder objectsSb = new StringBuilder();
			StringBuilder connectionsSb = new StringBuilder();

			Texture texture = material.GetTexture(unityExtension);

			if(texture == null)
			{
				objects = "";
				connections = "";
				return false;
			}
			string originalAssetPath = "";

#if UNITY_EDITOR
			originalAssetPath = AssetDatabase.GetAssetPath(texture);
#else
			Debug.LogError("Unity FBX Exporter can not serialize textures at runtime (yet). Look in FBXUnityMaterialGetter around line 250ish. Fix it and contribute to the project!");
			objects = "";
			connections = "";
			return false;
#endif
			string fullDataFolderPath = Application.dataPath;
			string textureFilePathFullName = originalAssetPath;
			string textureName = Path.GetFileNameWithoutExtension(originalAssetPath);
			string textureExtension = Path.GetExtension(originalAssetPath);

			// If we are copying the textures over, we update the relative positions
			if(copyTextures)
			{
				int indexOfAssetsFolder = fullDataFolderPath.LastIndexOf("/Assets");
				fullDataFolderPath = fullDataFolderPath.Remove(indexOfAssetsFolder, fullDataFolderPath.Length - indexOfAssetsFolder);
				
				string newPathFolder = newPath.Remove(newPath.LastIndexOf('/') + 1, newPath.Length - newPath.LastIndexOf('/') - 1);
				textureName = gameObj.name + "_" + material.name + unityExtension;

				textureFilePathFullName = fullDataFolderPath + "/" + newPathFolder + textureName + textureExtension;
			}

			long textureReference = FBXExporter.GetRandomFBXId();

			// TODO - test out different reference names to get one that doesn't load a _MainTex when importing.

			objectsSb.AppendLine("\tTexture: " + textureReference + ", \"Texture::" + materialName + "\", \"\" {");
			objectsSb.AppendLine("\t\tType: \"TextureVideoClip\"");
			objectsSb.AppendLine("\t\tVersion: 202");
			objectsSb.AppendLine("\t\tTextureName: \"Texture::" + materialName + "\"");
			objectsSb.AppendLine("\t\tProperties70:  {");
			objectsSb.AppendLine("\t\t\tP: \"CurrentTextureBlendMode\", \"enum\", \"\", \"\",0");
			objectsSb.AppendLine("\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"map1\"");
			objectsSb.AppendLine("\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",1");
			objectsSb.AppendLine("\t\t}");
			objectsSb.AppendLine("\t\tMedia: \"Video::" + materialName + "\"");

			// Sets the absolute path for the copied texture
			objectsSb.Append("\t\tFileName: \"");
			objectsSb.Append(textureFilePathFullName);
			objectsSb.AppendLine("\"");
			
			// Sets the relative path for the copied texture
			// TODO: If we don't copy the textures to a relative path, we must find a relative path to write down here
			if(copyTextures)
				objectsSb.AppendLine("\t\tRelativeFilename: \"/Textures/" + textureName + textureExtension + "\"");

			objectsSb.AppendLine("\t\tModelUVTranslation: 0,0"); // TODO: Figure out how to get the UV translation into here
			objectsSb.AppendLine("\t\tModelUVScaling: 1,1"); // TODO: Figure out how to get the UV scaling into here
			objectsSb.AppendLine("\t\tTexture_Alpha_Source: \"None\""); // TODO: Add alpha source here if the file is a cutout.
			objectsSb.AppendLine("\t\tCropping: 0,0,0,0");
			objectsSb.AppendLine("\t}");
			
			connectionsSb.AppendLine("\t;Texture::" + textureName + ", Material::" + materialName + "\"");
			connectionsSb.AppendLine("\tC: \"OP\"," + textureReference + "," + materialId + ", \"" + textureType + "\""); 
			
			connectionsSb.AppendLine();

			objects = objectsSb.ToString();
			connections = connectionsSb.ToString();

			return true;
		}
	}

	public class FBXUnityMeshGetter
	{

        /// <summary>
        /// Gets all the meshes and outputs to a string (even grabbing the child of each gameObject)
        /// </summary>
        /// <returns>The mesh to string.</returns>
        /// <param name="gameObj">GameObject Parent.</param>
        /// <param name="materials">Every Material in the parent that can be accessed.</param>
        /// <param name="objects">The StringBuidler to create objects for the FBX file.</param>
        /// <param name="connections">The StringBuidler to create connections for the FBX file.</param>
        /// <param name="parentObject">Parent object, if left null this is the top parent.</param>
        /// <param name="parentModelId">Parent model id, 0 if top parent.</param>
        public static long GetMeshToString(GameObject gameObj,
                                           Material[] materials,
                                           ref StringBuilder objects,
                                           ref StringBuilder connections,
                                           GameObject parentObject = null,
                                           long parentModelId = 0)
        {
            StringBuilder tempObjectSb = new StringBuilder();
            StringBuilder tempConnectionsSb = new StringBuilder();

            long geometryId = FBXExporter.GetRandomFBXId();
            long modelId = FBXExporter.GetRandomFBXId();
            //@cartzhang if SkinnedMeshRender gameobject,but has no meshfilter,add one.            
            SkinnedMeshRenderer[] meshfilterRender = gameObj.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < meshfilterRender.Length; i++)
            {
                if (meshfilterRender[i].GetComponent<MeshFilter>() == null)
                {
                    meshfilterRender[i].gameObject.AddComponent<MeshFilter>();
                    meshfilterRender[i].GetComponent<MeshFilter>().sharedMesh = GameObject.Instantiate(meshfilterRender[i].sharedMesh);
                }
            } 

            // Sees if there is a mesh to export and add to the system
            MeshFilter filter = gameObj.GetComponent<MeshFilter>();

			string meshName = gameObj.name;

			// A NULL parent means that the gameObject is at the top
			string isMesh = "Null";

			if(filter != null)
			{
				meshName = filter.sharedMesh.name;
				isMesh = "Mesh";
			}

			if(parentModelId == 0)
				tempConnectionsSb.AppendLine("\t;Model::" + meshName + ", Model::RootNode");
			else
				tempConnectionsSb.AppendLine("\t;Model::" + meshName + ", Model::USING PARENT");
			tempConnectionsSb.AppendLine("\tC: \"OO\"," + modelId + "," + parentModelId);
			tempConnectionsSb.AppendLine();
			tempObjectSb.AppendLine("\tModel: " + modelId + ", \"Model::" + gameObj.name + "\", \"" + isMesh + "\" {");
			tempObjectSb.AppendLine("\t\tVersion: 232");
			tempObjectSb.AppendLine("\t\tProperties70:  {");
			tempObjectSb.AppendLine("\t\t\tP: \"RotationOrder\", \"enum\", \"\", \"\",4");
			tempObjectSb.AppendLine("\t\t\tP: \"RotationActive\", \"bool\", \"\", \"\",1");
			tempObjectSb.AppendLine("\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",1");
			tempObjectSb.AppendLine("\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
			tempObjectSb.AppendLine("\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");
			// ===== Local Translation Offset =========
			Vector3 position = gameObj.transform.localPosition;

			tempObjectSb.Append("\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A+\",");

			// Append the X Y Z coords to the system
			tempObjectSb.AppendFormat("{0},{1},{2}", position.x * - 1, position.y, position.z);
			tempObjectSb.AppendLine();

			// Rotates the object correctly from Unity space
			Vector3 localRotation = gameObj.transform.localEulerAngles;
			tempObjectSb.AppendFormat("\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A+\",{0},{1},{2}", localRotation.x, localRotation.y * -1, -1 * localRotation.z);
			tempObjectSb.AppendLine();

			// Adds the local scale of this object
		    Vector3 localScale = gameObj.transform.localScale;
		    tempObjectSb.AppendFormat("\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",{0},{1},{2}", localScale.x, localScale.y, localScale.z);
			tempObjectSb.AppendLine();

			tempObjectSb.AppendLine("\t\t\tP: \"currentUVSet\", \"KString\", \"\", \"U\", \"map1\"");
			tempObjectSb.AppendLine("\t\t}");
			tempObjectSb.AppendLine("\t\tShading: T");
			tempObjectSb.AppendLine("\t\tCulling: \"CullingOff\"");
			tempObjectSb.AppendLine("\t}");


			// Adds in geometry if it exists, if it it does not exist, this is a empty gameObject file and skips over this
			if(filter != null)
			{
				Mesh mesh = filter.sharedMesh;

				// =================================
				//         General Geometry Info
				// =================================
				// Generate the geometry information for the mesh created

				tempObjectSb.AppendLine("\tGeometry: " + geometryId + ", \"Geometry::\", \"Mesh\" {");
				
				// ===== WRITE THE VERTICIES =====
				Vector3[] verticies = mesh.vertices;
				int vertCount = mesh.vertexCount * 3; // <= because the list of points is just a list of comma seperated values, we need to multiply by three

				tempObjectSb.AppendLine("\t\tVertices: *" + vertCount + " {");
				tempObjectSb.Append("\t\t\ta: ");
				for(int i = 0; i < verticies.Length; i++)
				{
					if(i > 0)
						tempObjectSb.Append(",");

					// Points in the verticies. We also reverse the x value because Unity has a reverse X coordinate
					tempObjectSb.AppendFormat("{0},{1},{2}", verticies[i].x * - 1, verticies[i].y, verticies[i].z);
				}

				tempObjectSb.AppendLine();
				tempObjectSb.AppendLine("\t\t} ");
				
				// ======= WRITE THE TRIANGLES ========
				int triangleCount = mesh.triangles.Length;
				int[] triangles = mesh.triangles;

				tempObjectSb.AppendLine("\t\tPolygonVertexIndex: *" + triangleCount + " {");

				// Write triangle indexes
				tempObjectSb.Append("\t\t\ta: ");
				for(int i = 0; i < triangleCount; i += 3)
				{
					if(i > 0)
						tempObjectSb.Append(",");

					// To get the correct normals, must rewind the triangles since we flipped the x direction
					tempObjectSb.AppendFormat("{0},{1},{2}", 
					                          triangles[i],
					                          triangles[i + 2], 
					                          (triangles[i + 1] * -1) - 1); // <= Tells the poly is ended

				}

				tempObjectSb.AppendLine();

				tempObjectSb.AppendLine("\t\t} ");
				tempObjectSb.AppendLine("\t\tGeometryVersion: 124");
				tempObjectSb.AppendLine("\t\tLayerElementNormal: 0 {");
				tempObjectSb.AppendLine("\t\t\tVersion: 101");
				tempObjectSb.AppendLine("\t\t\tName: \"\"");
				tempObjectSb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
				tempObjectSb.AppendLine("\t\t\tReferenceInformationType: \"Direct\"");
				
				// ===== WRITE THE NORMALS ==========
				Vector3[] normals = mesh.normals;

				tempObjectSb.AppendLine("\t\t\tNormals: *" + (triangleCount * 3) + " {");
				tempObjectSb.Append("\t\t\t\ta: ");

				for(int i = 0; i < triangleCount; i += 3)
				{
					if(i > 0)
						tempObjectSb.Append(",");

					// To get the correct normals, must rewind the normal triangles like the triangles above since x was flipped
					Vector3 newNormal = normals[triangles[i]];

					tempObjectSb.AppendFormat("{0},{1},{2},", 
					                         newNormal.x * -1, // Switch normal as is tradition
					                         newNormal.y, 
					                         newNormal.z);

					newNormal = normals[triangles[i + 2]];

					tempObjectSb.AppendFormat("{0},{1},{2},", 
					                          newNormal.x * -1, // Switch normal as is tradition
					                          newNormal.y, 
					                          newNormal.z);

					newNormal = normals[triangles[i + 1]];

					tempObjectSb.AppendFormat("{0},{1},{2}", 
					                          newNormal.x * -1, // Switch normal as is tradition
					                          newNormal.y, 
					                          newNormal.z);
				}

				tempObjectSb.AppendLine();
				tempObjectSb.AppendLine("\t\t\t}");
				tempObjectSb.AppendLine("\t\t}");
				
				// ===== WRITE THE COLORS =====
				bool containsColors = mesh.colors.Length == verticies.Length;
				
				if(containsColors)
				{
					Color[] colors = mesh.colors;
                
					Dictionary<Color, int> colorTable = new Dictionary<Color, int>(); // reducing amount of data by only keeping unique colors.
					int idx = 0;

					// build index table of all the different colors present in the mesh            
					for (int i = 0; i < colors.Length; i++)
					{
						if (!colorTable.ContainsKey(colors[i]))
						{
							colorTable[colors[i]] = idx;
							idx++;
						}
					}

					tempObjectSb.AppendLine("\t\tLayerElementColor: 0 {");
					tempObjectSb.AppendLine("\t\t\tVersion: 101");
					tempObjectSb.AppendLine("\t\t\tName: \"Col\"");
					tempObjectSb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
					tempObjectSb.AppendLine("\t\t\tReferenceInformationType: \"IndexToDirect\"");
					tempObjectSb.AppendLine("\t\t\tColors: *" + colorTable.Count * 4 + " {");
					tempObjectSb.Append("\t\t\t\ta: ");

					bool first = true;
					foreach (KeyValuePair<Color, int> color in colorTable)
					{
						if (!first)
							tempObjectSb.Append(",");

						tempObjectSb.AppendFormat("{0},{1},{2},{3}", color.Key.r, color.Key.g, color.Key.b, color.Key.a);
						first = false;
					}
					tempObjectSb.AppendLine();

					tempObjectSb.AppendLine("\t\t\t\t}");

					// Color index
					tempObjectSb.AppendLine("\t\t\tColorIndex: *" + triangles.Length + " {");
					tempObjectSb.Append("\t\t\t\ta: ");

					for (int i = 0; i < triangles.Length; i += 3)
					{
						if (i > 0)
							tempObjectSb.Append(",");

						// Triangles need to be fliped for the x flip
						int index1 = triangles[i];
						int index2 = triangles[i + 2];
						int index3 = triangles[i + 1];

						// Find the color index related to that vertice index
						index1 = colorTable[colors[index1]];
						index2 = colorTable[colors[index2]];
						index3 = colorTable[colors[index3]];

						tempObjectSb.AppendFormat("{0},{1},{2}", index1, index2, index3);
					}

					tempObjectSb.AppendLine();

					tempObjectSb.AppendLine("\t\t\t}");
					tempObjectSb.AppendLine("\t\t}");
				}
				else
                    Debug.LogWarning("Mesh contains " + mesh.vertices.Length + " vertices for " + mesh.colors.Length + " colors. Skip color export");
				
                

				// ================ UV CREATION =========================

				// -- UV 1 Creation
				int uvLength = mesh.uv.Length;
				Vector2[] uvs = mesh.uv;

				tempObjectSb.AppendLine("\t\tLayerElementUV: 0 {"); // the Zero here is for the first UV map
				tempObjectSb.AppendLine("\t\t\tVersion: 101");
				tempObjectSb.AppendLine("\t\t\tName: \"map1\"");
				tempObjectSb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
				tempObjectSb.AppendLine("\t\t\tReferenceInformationType: \"IndexToDirect\"");
				tempObjectSb.AppendLine("\t\t\tUV: *" + uvLength * 2 + " {");
				tempObjectSb.Append("\t\t\t\ta: ");

				for(int i = 0; i < uvLength; i++)
				{
					if(i > 0)
						tempObjectSb.Append(",");

					tempObjectSb.AppendFormat("{0},{1}", uvs[i].x, uvs[i].y);

				}
				tempObjectSb.AppendLine();

				tempObjectSb.AppendLine("\t\t\t\t}");

				// UV tile index coords
				tempObjectSb.AppendLine("\t\t\tUVIndex: *" + triangleCount +" {");
				tempObjectSb.Append("\t\t\t\ta: ");

				for(int i = 0; i < triangleCount; i += 3)
				{
					if(i > 0)
						tempObjectSb.Append(",");

					// Triangles need to be fliped for the x flip
					int index1 = triangles[i];
					int index2 = triangles[i+2];
					int index3 = triangles[i+1];

					tempObjectSb.AppendFormat("{0},{1},{2}", index1, index2, index3);
				}

				tempObjectSb.AppendLine();

				tempObjectSb.AppendLine("\t\t\t}");
				tempObjectSb.AppendLine("\t\t}");

				// -- UV 2 Creation
				if (mesh.uv2.Length != 0) {
 					uvLength = mesh.uv2.Length;
 					uvs = mesh.uv2;

 					tempObjectSb.AppendLine("\t\tLayerElementUV: 1 {"); // the Zero here is for the first UV map
 					tempObjectSb.AppendLine("\t\t\tVersion: 101");
 					tempObjectSb.AppendLine("\t\t\tName: \"map2\"");
 					tempObjectSb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
 					tempObjectSb.AppendLine("\t\t\tReferenceInformationType: \"IndexToDirect\"");
 					tempObjectSb.AppendLine("\t\t\tUV: *" + uvLength * 2 + " {");
 					tempObjectSb.Append("\t\t\t\ta: ");

 					for(int i = 0; i < uvLength; i++)
 					{
 						if(i > 0)
 							tempObjectSb.Append(",");

 						tempObjectSb.AppendFormat("{0},{1}", uvs[i].x, uvs[i].y);

 					}
 					tempObjectSb.AppendLine();

 					tempObjectSb.AppendLine("\t\t\t\t}");

 					// UV tile index coords
 					tempObjectSb.AppendLine("\t\t\tUVIndex: *" + triangleCount +" {");
 					tempObjectSb.Append("\t\t\t\ta: ");

 					for(int i = 0; i < triangleCount; i += 3)
 					{
 						if(i > 0)
 							tempObjectSb.Append(",");

 						// Triangles need to be fliped for the x flip
 						int index1 = triangles[i];
 						int index2 = triangles[i+2];
 						int index3 = triangles[i+1];

 						tempObjectSb.AppendFormat("{0},{1},{2}", index1, index2, index3);
 					}

 					tempObjectSb.AppendLine();

 					tempObjectSb.AppendLine("\t\t\t}");
 					tempObjectSb.AppendLine("\t\t}");
 				}

				// -- Smoothing
				// TODO: Smoothing doesn't seem to do anything when importing. This maybe should be added. -KBH

				// ============ MATERIALS =============

				tempObjectSb.AppendLine("\t\tLayerElementMaterial: 0 {");
				tempObjectSb.AppendLine("\t\t\tVersion: 101");
				tempObjectSb.AppendLine("\t\t\tName: \"\"");
				tempObjectSb.AppendLine("\t\t\tMappingInformationType: \"ByPolygon\"");
				tempObjectSb.AppendLine("\t\t\tReferenceInformationType: \"IndexToDirect\"");

				int totalFaceCount = 0;

				// So by polygon means that we need 1/3rd of how many indicies we wrote.
				int numberOfSubmeshes = mesh.subMeshCount;

				StringBuilder submeshesSb = new StringBuilder();

				// For just one submesh, we set them all to zero
				if(numberOfSubmeshes == 1)
				{
					int numFaces = triangles.Length / 3;

					for(int i = 0; i < numFaces; i++)
					{
						submeshesSb.Append("0,");
						totalFaceCount++;
					}
				}
				else
				{
					List<int[]> allSubmeshes = new List<int[]>();
					
					// Load all submeshes into a space
					for(int i = 0; i < numberOfSubmeshes; i++)
						allSubmeshes.Add(mesh.GetIndices(i));

					// TODO: Optimize this search pattern
					for(int i = 0; i < triangles.Length; i += 3)
					{
						for(int subMeshIndex = 0; subMeshIndex < allSubmeshes.Count; subMeshIndex++)
						{
							bool breaker = false;
							
							for(int n = 0; n < allSubmeshes[subMeshIndex].Length; n += 3)
							{
								if(triangles[i] == allSubmeshes[subMeshIndex][n]
								   && triangles[i + 1] == allSubmeshes[subMeshIndex][n + 1]
								   && triangles[i + 2] == allSubmeshes[subMeshIndex][n + 2])
								{
									submeshesSb.Append(subMeshIndex.ToString());
									submeshesSb.Append(",");
									totalFaceCount++;
									break;
								}
								
								if(breaker)
									break;
							}
						}
					}
				}

				tempObjectSb.AppendLine("\t\t\tMaterials: *" + totalFaceCount + " {");
				tempObjectSb.Append("\t\t\t\ta: ");
				tempObjectSb.AppendLine(submeshesSb.ToString());
				tempObjectSb.AppendLine("\t\t\t} ");
				tempObjectSb.AppendLine("\t\t}");

				// ============= INFORMS WHAT TYPE OF LATER ELEMENTS ARE IN THIS GEOMETRY =================
				tempObjectSb.AppendLine("\t\tLayer: 0 {");
				tempObjectSb.AppendLine("\t\t\tVersion: 100");
				tempObjectSb.AppendLine("\t\t\tLayerElement:  {");
				tempObjectSb.AppendLine("\t\t\t\tType: \"LayerElementNormal\"");
				tempObjectSb.AppendLine("\t\t\t\tTypedIndex: 0");
				tempObjectSb.AppendLine("\t\t\t}");
				tempObjectSb.AppendLine("\t\t\tLayerElement:  {");
				tempObjectSb.AppendLine("\t\t\t\tType: \"LayerElementMaterial\"");
				tempObjectSb.AppendLine("\t\t\t\tTypedIndex: 0");
				tempObjectSb.AppendLine("\t\t\t}");
				tempObjectSb.AppendLine("\t\t\tLayerElement:  {");
				tempObjectSb.AppendLine("\t\t\t\tType: \"LayerElementTexture\"");
				tempObjectSb.AppendLine("\t\t\t\tTypedIndex: 0");
				tempObjectSb.AppendLine("\t\t\t}");
				if(containsColors)
				{
					tempObjectSb.AppendLine("\t\t\tLayerElement:  {");
					tempObjectSb.AppendLine("\t\t\t\tType: \"LayerElementColor\"");
					tempObjectSb.AppendLine("\t\t\t\tTypedIndex: 0");
					tempObjectSb.AppendLine("\t\t\t}");
				}
				tempObjectSb.AppendLine("\t\t\tLayerElement:  {");
				tempObjectSb.AppendLine("\t\t\t\tType: \"LayerElementUV\"");
				tempObjectSb.AppendLine("\t\t\t\tTypedIndex: 0");
				tempObjectSb.AppendLine("\t\t\t}");


				// TODO: Here we would add UV layer 1 for ambient occlusion UV file
	//			tempObjectSb.AppendLine("\t\t\tLayerElement:  {");
	//			tempObjectSb.AppendLine("\t\t\t\tType: \"LayerElementUV\"");
	//			tempObjectSb.AppendLine("\t\t\t\tTypedIndex: 1");
	//			tempObjectSb.AppendLine("\t\t\t}");

                if (mesh.uv2.Length != 0) {
 					tempObjectSb.AppendLine("\t\t}");
 					tempObjectSb.AppendLine("\t\tLayer: 1 {");
 					tempObjectSb.AppendLine("\t\t\tVersion: 100");
 					tempObjectSb.AppendLine("\t\t\tLayerElement:  {");
 					tempObjectSb.AppendLine("\t\t\t\tType: \"LayerElementUV\"");
 					tempObjectSb.AppendLine("\t\t\t\tTypedIndex: 1");
 					tempObjectSb.AppendLine("\t\t\t}");
 				}




				tempObjectSb.AppendLine("\t\t}");
				tempObjectSb.AppendLine("\t}");

				// Add the connection for the model to the geometry so it is attached the right mesh
				tempConnectionsSb.AppendLine("\t;Geometry::, Model::" + mesh.name);
				tempConnectionsSb.AppendLine("\tC: \"OO\"," + geometryId + "," + modelId);
				tempConnectionsSb.AppendLine();

				// Add the connection of all the materials in order of submesh
				MeshRenderer meshRenderer = gameObj.GetComponent<MeshRenderer>();
				if(meshRenderer != null)
				{
					Material[] allMaterialsInThisMesh = meshRenderer.sharedMaterials;

					for(int i = 0; i < allMaterialsInThisMesh.Length; i++)
					{
						Material mat = allMaterialsInThisMesh[i];
						int referenceId = Mathf.Abs(mat.GetInstanceID());
		
						if(mat == null)
						{
							Debug.LogError("ERROR: the game object " + gameObj.name + " has an empty material on it. This will export problematic files. Please fix and reexport");
							continue;
						}

						tempConnectionsSb.AppendLine("\t;Material::" + mat.name + ", Model::" + mesh.name);
						tempConnectionsSb.AppendLine("\tC: \"OO\"," + referenceId + "," + modelId);
						tempConnectionsSb.AppendLine();
					}
				}

			}

			// Recursively add all the other objects to the string that has been built.
			for(int i = 0; i < gameObj.transform.childCount; i++)
			{
				GameObject childObject = gameObj.transform.GetChild(i).gameObject;

				FBXUnityMeshGetter.GetMeshToString(childObject, materials, ref tempObjectSb, ref tempConnectionsSb, gameObj, modelId);
			}

			objects.Append(tempObjectSb.ToString());
			connections.Append(tempConnectionsSb.ToString());

			return modelId;
		}

        //private Mesh CreateMeshInstance(UnityEngine.Object obj,Mesh mesh)
        //{
        //    obj = new UnityEngine.Object();
        //    Mesh instanceMesh = UnityEngine.Instantiate(Mesh);
        //}
	}
}