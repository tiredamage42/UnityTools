
#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;

namespace UnityTools.EditorTools {

    public static class ItchIOUploader
    {
        const string WINDOWS = "windows";
        const string OSX = "osx";
        const string LINUX = "linux";

        public static void UploadToItch(string pathToButlerExe, ProjectBuilder.SupportedBuild buildTarget, string gamePath, string user, string gameName, string version = null)
        {
            // Disable to capture error output for debugging.
            bool showUploadProgress = true;

            // Verify that butler executable exists.
            if (!File.Exists(pathToButlerExe))
            {
                UnityEngine.Debug.LogError("Couldn't find butler.exe file at path \"" + pathToButlerExe + "\", please check provided path");
                return;
            }

            gamePath = Path.GetFullPath(gamePath);

            // Generate build args for the form: butler push {optional args} {build path} {itch username}/{itch game}:{channel}
            StringBuilder scriptArguments = new StringBuilder("push ");

            switch (buildTarget)
            {
                case ProjectBuilder.SupportedBuild.StandaloneLinuxUniversal:
                    // Fix exe permissions for Linux/OSX.
                    scriptArguments.Append("--fix-permissions ");
                    break;
            }

            if (!string.IsNullOrEmpty(version))
            {
                // Append generated versions string.
                scriptArguments.Append(string.Format("--userversion \"{0}\" ", version));
            }

            scriptArguments.Append("\"" + gamePath + "\"" + " " + user + "/" + gameName + ":");

            string itchChannel = GetChannelName(buildTarget);
            if(string.IsNullOrEmpty(itchChannel))
                UnityEngine.Debug.LogWarning("UploadItch: The current BuildTarget doesn't appear to be a standard Itch.IO build target.");
            scriptArguments.Append(itchChannel);

            // UnityEngine.Debug.Log("Would have run itch uploader with following command line: \"" + pathToButlerExe + " " + scriptArguments + "\"");

            string arguments = scriptArguments.ToString();

            
            // Create and start butler process.
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.GetFullPath(pathToButlerExe);
            startInfo.UseShellExecute = showUploadProgress;
            startInfo.CreateNoWindow = !showUploadProgress;
            startInfo.RedirectStandardOutput = !showUploadProgress;
            startInfo.RedirectStandardError = !showUploadProgress;

            if (!string.IsNullOrEmpty(arguments))
                startInfo.Arguments = arguments;

            Process proc = Process.Start(startInfo);

            StringBuilder outputText = new StringBuilder();
            if (!showUploadProgress)
            {
                // Capture stdout.
                proc.OutputDataReceived += (sendingProcess, outputLine) => { outputText.AppendLine(outputLine.Data); };

                proc.BeginOutputReadLine();
            }

            // Wait for butler to finish.
            proc.WaitForExit();

            // Display error if one occurred.
            if (proc.ExitCode != 0)
            {
                string errString;
                if (showUploadProgress)
                    errString = "Run w/ ShowUploadProgress disabled to capture debug output to console.";
                else
                    errString = "Check console window for debug output.";
                
                UnityEngine.Debug.LogError("Itch Upload Failed.  " + string.Format("Exit code: {0}\n{1}", proc.ExitCode, errString));
                
                UnityEngine.Debug.Log("ITCH.IO BUTLER OUTPUT: " + outputText.ToString());
            }
        }

        static string GetChannelName(ProjectBuilder.SupportedBuild target)
        {
            switch (target)
            {
                // Windows
                case ProjectBuilder.SupportedBuild.StandaloneWindows:
                    return WINDOWS + "-x86";
                case ProjectBuilder.SupportedBuild.StandaloneWindows64:
                    return WINDOWS + "-x64";

                // Linux
                case ProjectBuilder.SupportedBuild.StandaloneLinux:
                    return LINUX + "-x86";
                case ProjectBuilder.SupportedBuild.StandaloneLinux64:
                    return LINUX + "-x64";
                case ProjectBuilder.SupportedBuild.StandaloneLinuxUniversal:
                    return LINUX + "-universal";

                // OSX
                case ProjectBuilder.SupportedBuild.StandaloneOSX:
                    return OSX;
                
                default:
                    return null;
            }
        }
    }
}

#endif