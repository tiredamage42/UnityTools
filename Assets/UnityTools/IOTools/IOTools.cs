
using System.Collections;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace UnityTools {
    public static class IOTools 
    {
        
        public static void ZipDirectory(string inputDir, string outputPath)
        {
            try
            {
                if (!outputPath.EndsWith(".zip"))
                    outputPath += ".zip";

                if (!Directory.Exists(inputDir))
                {
                    UnityEngine.Debug.LogError(string.Format("Input path does not exist: {0}", inputDir));
                    return;
                }

                // Make sure that all parent directories in path are already created.
                string parentPath = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(parentPath))
                    Directory.CreateDirectory(parentPath);
                
                // Delete old file if it exists.
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(outputPath))
                {
                    zip.ParallelDeflateThreshold = -1; // Parallel deflate is bugged in DotNetZip, so we need to disable it.
                    zip.AddDirectory(inputDir);
                    zip.Save();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.ToString());
            }
        }

        

		
        public static void SaveToFile (object obj, string filePath) {
            using (FileStream file = File.Create(filePath)) {
                using (GZipStream compressed = new GZipStream(file, CompressionMode.Compress)) {
                    using (MemoryStream ms = new MemoryStream()) {
                        new BinaryFormatter().Serialize(ms, obj);
                        byte[] bytesToCompress = ms.ToArray();
                        compressed.Write(bytesToCompress, 0, bytesToCompress.Length);
                    }
                }
            }
        }

        public static object LoadFromFile (string filePath) {
            object obj = null;
            using (FileStream file = File.Open(filePath, FileMode.Open)) {
                using (GZipStream decompressed = new GZipStream(file, CompressionMode.Decompress)) {
                    obj = new BinaryFormatter().Deserialize(decompressed);
                }
            }
            return obj;
        }

        static void RefreshAssetDatabase () {
            #if UNITY_EDITOR
            if (Application.isPlaying)
                AssetDatabase.Refresh();
            #endif
        }

        // FOLDER OPS


        public static void MoveFolder(string inputPath, string outputPath, bool overwrite = true)
        {
            
            if (!Directory.Exists(inputPath))
            {
                UnityEngine.Debug.LogError("Folder Move Failed. Input does not exist.");
                return;
            }

            // Make sure that all parent directories in path are already created.
            string parentPath = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(parentPath))
                Directory.CreateDirectory(parentPath);
            
            if (overwrite && Directory.Exists(outputPath))
            {
                // Delete previous output.
                Directory.Delete(outputPath);
            }

            
            Directory.Move(inputPath, outputPath);

            RefreshAssetDatabase();
        }

        public static void CopyFolder(string inputPath, string outputPath, bool overwrite = true)
        {
            if (!Directory.Exists(inputPath))
            {
                UnityEngine.Debug.LogError("Folder Copy Failed. Input does not exist.");
                return;
            }

            // Make sure that all parent directories in path are already created.
            // string parentPath = Path.GetDirectoryName(outputPath);
            // if (!Directory.Exists(parentPath))
            //     Directory.CreateDirectory(parentPath);

            // Delete previous output.
            if (overwrite && Directory.Exists(outputPath))
                Directory.Delete(outputPath);

            _CopyFolder(inputPath, outputPath);

            RefreshAssetDatabase();
        }

        static void _CopyFolder(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);
    
            CopyAll(diSource, diTarget);
        }
    
        static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
    
            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                // Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }
    
            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static void DeleteFolder(string inputPath)
        {
            if (Directory.Exists(inputPath))
            {
                Directory.Delete(inputPath);
                RefreshAssetDatabase();
            }
        }

        // FILE OPS
        public static void MoveFile(string inputPath, string outputPath, bool overwrite = true, bool recursiveSearch = true)
        {
            bool containsWildcard = string.IsNullOrEmpty(inputPath) ? false : Path.GetFileNameWithoutExtension(inputPath).Contains("*");

            if (!containsWildcard && !File.Exists(inputPath))
                return;
            
            // Delete previous output.
            if (!containsWildcard && overwrite && File.Exists(outputPath))
                File.Delete(outputPath);
            
            if (containsWildcard)
            {
                string inputDirectory = Path.GetDirectoryName(inputPath);
                string outputDirectory = Path.GetDirectoryName(outputPath);

                SearchOption option = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] fileList = Directory.GetFiles(inputDirectory, Path.GetFileName(inputPath), option);

                for (int i = 0; i < fileList.Length; i++)
                {
                    string fileName = Path.GetFileName(fileList[i]);
                    string outputFile = Path.Combine(outputDirectory, fileName);

                    if (File.Exists(outputFile))
                        File.Delete(outputFile);
        
                
                    File.Move(fileList[i], outputFile);
                }
            }
            else
            {
                File.Move(inputPath, outputPath);
            }
            RefreshAssetDatabase();
        }

        public static void CopyFile(string inputPath, string outputPath, bool overwrite = true, bool recursiveSearch = true)
        {
            bool containsWildcard = string.IsNullOrEmpty(inputPath) ? false : Path.GetFileNameWithoutExtension(inputPath).Contains("*");

            if (!containsWildcard && !File.Exists(inputPath))
                return;
            

            // Delete previous output.
            if (!containsWildcard && overwrite && File.Exists(outputPath))
                File.Delete(outputPath);
            
            if (containsWildcard)
            {
                string inputDirectory = Path.GetDirectoryName(inputPath);
                string outputDirectory = Path.GetDirectoryName(outputPath);

                SearchOption option = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] fileList = Directory.GetFiles(inputDirectory, Path.GetFileName(inputPath), option);

                for (int i = 0; i < fileList.Length; i++)
                {
                    string fileName = Path.GetFileName(fileList[i]);
                    string outputFile = Path.Combine(outputDirectory, fileName);

                    if (File.Exists(outputFile))
                        File.Delete(outputFile);
        

                    File.Copy(fileList[i], outputFile, overwrite);
                }
            }
            else
            {
                File.Copy(inputPath, outputPath, overwrite);
            }
            RefreshAssetDatabase();
        }

        public static void DeleteFile(string inputPath, bool recursiveSearch = true)
        {
            bool containsWildcard = string.IsNullOrEmpty(inputPath) ? false : Path.GetFileNameWithoutExtension(inputPath).Contains("*");

            if (!containsWildcard && File.Exists(inputPath))
            {
                File.Delete(inputPath);
            }
            else if (containsWildcard)
            {
                string inputDirectory = Path.GetDirectoryName(inputPath);

                SearchOption option = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] fileList = Directory.GetFiles(inputDirectory, Path.GetFileName(inputPath), option);

                for (int i = 0; i < fileList.Length; i++)
                {
                    File.Delete(fileList[i]);
                }
            }
            else
            {
                // Error. File does not exist.
            }
            RefreshAssetDatabase();
            
        }


        

    }
}
