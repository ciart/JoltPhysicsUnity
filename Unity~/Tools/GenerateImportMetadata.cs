using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<string, (BuildTarget Target, string Cpu, string EditorOS)> Platforms =
            new Dictionary<string, (BuildTarget, string, string)>
            {
                { "native/osx/libjoltc.dylib", (BuildTarget.StandaloneOSX, "AnyCPU", "OSX") },
                { "native/win-x64/joltc.dll", (BuildTarget.StandaloneWindows64, "x86_64", "Windows") },
                { "native/win-arm64/joltc.dll", (BuildTarget.StandaloneWindows64, "ARM64", "Windows") },
                { "native/linux-x64/libjoltc.so", (BuildTarget.StandaloneLinux64, "x86_64", "Linux") },
                { "native/android-arm64/libjoltc.so", (BuildTarget.Android, "ARM64", null) },
                { "native/android-x64/libjoltc.so", (BuildTarget.Android, "x86_64", null) }
            };

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
                int configured = 0;
                foreach (string asset in AssetDatabase.GetAllAssetPaths().Where(assetPath => assetPath.StartsWith(PackageRoot + "native/", StringComparison.Ordinal)))
                {
                    PluginImporter importer = AssetImporter.GetAtPath(asset) as PluginImporter;
                    if (importer == null)
                        continue;
                    bool supported = Platforms.TryGetValue(asset.Substring(PackageRoot.Length), out var configuration);
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(supported && configuration.EditorOS != null);
                    foreach (BuildTarget target in new[] { BuildTarget.StandaloneOSX, BuildTarget.StandaloneWindows, BuildTarget.StandaloneWindows64, BuildTarget.StandaloneLinux64, BuildTarget.Android, BuildTarget.iOS, BuildTarget.WebGL, BuildTarget.WSAPlayer })
                        importer.SetCompatibleWithPlatform(target, supported && target == configuration.Target);
                    if (supported)
                    {
                        if (configuration.EditorOS != null)
                        {
                            importer.SetEditorData("OS", configuration.EditorOS);
                            importer.SetEditorData("CPU", configuration.Cpu);
                        }
                        importer.SetPlatformData(configuration.Target, "CPU", configuration.Cpu);
                    }
                    importer.SaveAndReimport();
                    if (supported)
                    {
                        if (!importer.GetCompatibleWithPlatform(configuration.Target) ||
                            importer.GetPlatformData(configuration.Target, "CPU") != configuration.Cpu ||
                            importer.GetCompatibleWithEditor() != (configuration.EditorOS != null) ||
                            (configuration.EditorOS != null &&
                                (importer.GetEditorData("OS") != configuration.EditorOS || importer.GetEditorData("CPU") != configuration.Cpu)))
                            throw new InvalidOperationException("Plugin platform settings did not persist: " + asset);
                        configured++;
                        Debug.Log("[Jolt metadata] " + asset + ": " + configuration.Target + "/" + configuration.Cpu);
                    }
                }
                AssetDatabase.SaveAssets();
                if (!File.Exists(Path.Combine(path, "src/JoltPhysicsSharp/Foundation.cs.meta")))
                    throw new InvalidOperationException("Unity did not generate source metadata in the supplied checkout.");
                if (configured != Platforms.Count)
                    throw new InvalidOperationException("Not all configured native artifacts were imported.");
                Finish(0, "Unity saved and checked OS/CPU settings for " + configured + " single-precision native plugins.");
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
