using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace JoltPortTools
{
    public static class GenerateImportMetadata
    {
        private const string PackageRoot = "Packages/com.ciart.joltphysics/";

        public static void Run()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(arguments, "-joltPackagePath");
            if (index < 0 || index + 1 >= arguments.Length)
                throw new ArgumentException("Supply -joltPackagePath with the writable package checkout.");
            string path = Path.GetFullPath(arguments[index + 1]);
            if (!File.Exists(Path.Combine(path, "package.json")))
                throw new ArgumentException("The package checkout has no root manifest.");
            PackageInfo package = PackageInfo.GetAllRegisteredPackages().FirstOrDefault(item => item.name == "com.ciart.joltphysics");
            if (package == null || package.source != PackageSource.Embedded)
                throw new InvalidOperationException("Embed the writable checkout under the validation project's Packages/com.ciart.joltphysics before generating metadata.");
            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                foreach (string asset in AssetDatabase.GetAllAssetPaths().Where(assetPath => assetPath.StartsWith(PackageRoot + "native/", StringComparison.Ordinal)))
                {
                    PluginImporter importer = AssetImporter.GetAtPath(asset) as PluginImporter;
                    if (importer == null)
                        continue;
                    bool supported = asset == PackageRoot + "native/osx/libjoltc.dylib";
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(supported);
                    foreach (BuildTarget target in new[] { BuildTarget.StandaloneOSX, BuildTarget.StandaloneWindows, BuildTarget.StandaloneWindows64, BuildTarget.StandaloneLinux64, BuildTarget.Android, BuildTarget.iOS, BuildTarget.WebGL, BuildTarget.WSAPlayer })
                        importer.SetCompatibleWithPlatform(target, supported && target == BuildTarget.StandaloneOSX);
                    if (supported)
                    {
                        importer.SetEditorData("OS", "OSX");
                        importer.SetEditorData("CPU", "AnyCPU");
                        importer.SetPlatformData(BuildTarget.StandaloneOSX, "CPU", "AnyCPU");
                    }
                    importer.SaveAndReimport();
                }
                AssetDatabase.SaveAssets();
                if (!File.Exists(Path.Combine(path, "src/JoltPhysicsSharp/Foundation.cs.meta")))
                    throw new InvalidOperationException("Unity did not generate source metadata in the supplied checkout.");
                Finish(0, "Unity generated import metadata and configured the single-precision macOS plugin in the writable checkout.");
            }
            catch (Exception exception)
            {
                Finish(1, exception.ToString());
            }
        }

        private static void Finish(int code, string message)
        {
            if (code == 0) Debug.Log("[Jolt metadata] " + message);
            else Debug.LogError("[Jolt metadata] " + message);
            EditorApplication.Exit(code);
        }
    }
}
