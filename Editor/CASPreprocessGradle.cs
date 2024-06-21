//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CAS.UEditor
{
    using Utils = CASEditorUtils;

    internal static class CASPreprocessGradle
    {
        [Obsolete("No longer used")]
        internal static void Configure(CASEditorSettings settings)
        {
        }

        internal static void UpdateGradleTemplateIfNeed()
        {
#if UNITY_2019_3_OR_NEWER
            var needUpdate = true;
#else
            var needUpdate = false;
#endif

            if (!needUpdate || !File.Exists(Utils.mainGradlePath))
                return;

            try
            {
                needUpdate = false;
                using (var reader = new StreamReader(Utils.mainGradlePath))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        if (line.Contains("classpath 'com.android.tools.build:gradle:"))
                        {
                            needUpdate = true;
                            break;
                        }
                        line = reader.ReadLine();
                    }
                }
                if (needUpdate)
                    TryEnableGradleTemplate(Utils.mainGradlePath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal static string[] TryEnableGradleTemplate(string assetPath)
        {
            var fileName = Path.GetFileName(assetPath);
            var internalTemplate = GetUnityAndroidToolsPath("GradleTemplates", fileName);

            if (!File.Exists(internalTemplate))
            {
                Debug.LogError(Utils.logTag + "Template file not found: " + internalTemplate);
                return null;
            }
            try
            {
                var fileLines = File.ReadAllLines(internalTemplate);
                Utils.WriteToAsset(assetPath, fileLines);
                Utils.Log("Gradle template activated: " + assetPath);
                return fileLines;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        internal static Version GetGradleWrapperVersion()
        {
            string gradleLibPath;
            if (IsUsedGradleWrapperEmbeddedInUnity())
                gradleLibPath = GetUnityAndroidToolsPath("gradle", "lib");
            else
                gradleLibPath = Path.Combine(EditorPrefs.GetString("GradlePath"), "lib");

            if (!Directory.Exists(gradleLibPath))
                return null;

            const string wrapperName = "gradle-wrapper-";
            const string wrapeprExtension = ".jar";
            foreach (var file in Directory.GetFiles(gradleLibPath, wrapperName + "*" + wrapeprExtension))
            {
                try
                {
                    var prefixLength = Path.Combine(gradleLibPath, wrapperName).Length;
                    var version = file.Substring(prefixLength, file.Length - prefixLength - wrapeprExtension.Length);
                    if (version.Length > 0)
                        return new Version(version);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return null;
        }

        private static List<string> ReadGradleFile(string prefix, string path, bool required = true)
        {
            try
            {
                if (File.Exists(path))
                    return new List<string>(File.ReadAllLines(path));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (!required)
                return null;

            var message = "A successful build requires do modifications to " + prefix + " template. " +
                "But the template is not activated now.";
            Utils.DialogOrCancelBuild(message + "\nClick Сontinue to activate.", BuildTarget.NoTarget);
            var fileLines = TryEnableGradleTemplate(path);
            if (fileLines != null)
                return new List<string>(fileLines);
            Utils.StopBuildWithMessage(message, BuildTarget.NoTarget);
            return null;
        }

        private static bool IsUsedGradleWrapperEmbeddedInUnity()
        {
            return EditorPrefs.GetBool("GradleUseEmbedded");
        }

        private static string GetUnityAndroidToolsPath(params string[] parts)
        {
            var result = GetUnityAndroidToolsDirPath();
            for (int i = 0; i < parts.Length; i++)
                result = Path.Combine(result, parts[i]);

            return Path.GetFullPath(result);
        }

        private static string GetUnityAndroidToolsDirPath()
        {
            // Alternate of internal unity method
            // BuildPipeline.GetBuildToolsDirectory( ( BuildTarget )13 );
            try
            {
                return (string)typeof(BuildPipeline)
                    .GetMethod("GetBuildToolsDirectory", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new object[] { BuildTarget.Android });
            }
            catch { }

            // App path ends `version/Unity.app` or `version/Editor/Unity.exe`
            var appPath = EditorApplication.applicationPath;
            var result = Path.GetDirectoryName(EditorApplication.applicationPath);
            // Windows path: 2020.3.11\Editor\Data\PlaybackEngines\AndroidPlayer\Tools\gradle\lib
            // Macos path: 2020.3.11/PlaybackEngines/AndroidPlayer/Tools/gradle/lib
            if (appPath.EndsWith(".exe"))
                result = Path.Combine(result, "Data");
            return Path.Combine(Path.Combine(Path.Combine(result, "PlaybackEngines"), "AndroidPlayer"), "Tools");
        }

    }
}