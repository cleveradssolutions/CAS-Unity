//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || CASDeveloper


#if !UNITY_2018_4_OR_NEWER
#define DeclareJavaVersion
#endif

// Unity 2023.3 use Gradle Wrapper 7.6 and plugin 7.3.1
// Unity 2022.2 use Gradle Wrapper 7.2 and plugin 7.1.2
// Unity 2020.1-2022.1 use Gradle Wrapper 6.1.1 and plugin 4.0.1
#if !UNITY_2022_2_OR_NEWER
// Many SDKs use the new <queries> element for Android 11 in their bundled Android Manifest files.
// The Android Gradle plugin version should support new elements, else this will cause build errors:
// Android resource linking failed
// error: unexpected element <queries> found in <manifest>.
#define UpdateGradleToolsMinorVersion

// Not Required by default to check wrapper version.
//#define UpdateGradleToolsForUsedWrapper

// Known issue with jCenter repository where repository is not responding
// and gradle build stops with timeout error.
#define ReplaceJCenterToMavenCentral
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Utils = CAS.UEditor.CASEditorUtils;

namespace CAS.UEditor
{
    internal static class CASPreprocessGradle
    {
        internal static void Configure(CASEditorSettings settings)
        {
            bool baseGradleChanged = false;

#if UNITY_2019_3_OR_NEWER
            const string baseGradlePath = Utils.projectGradlePath;
#if UpdateGradleToolsMinorVersion || ReplaceJCenterToMavenCentral
            var baseGradle = ReadGradleFile("Base Gradle", baseGradlePath);
#else
            var baseGradle = new List<string>();
#endif

            const string launcherGradlePath = Utils.launcherGradlePath;
            var launcherGradle = ReadGradleFile("Launcher Gradle", launcherGradlePath, false);
#else
            const string baseGradlePath = Utils.mainGradlePath;
            const string launcherGradlePath = Utils.mainGradlePath;

            var baseGradle = ReadGradleFile( "Gradle", baseGradlePath );
            var launcherGradle = baseGradle;
#endif

#if UpdateGradleToolsMinorVersion
            if (settings.updateGradlePluginVersion
                && UpdateGradlePluginVersion(baseGradle, baseGradlePath))
                baseGradleChanged = true;
#endif

#if ReplaceJCenterToMavenCentral
            if (UpdateBaseGradleRepositories(baseGradle, baseGradlePath))
                baseGradleChanged = true;
#endif

            // Enabled by default Dexing artifact transform causes issues for ExoPlayer with Gradle plugin 3.5.0+
            var dexingArtifactProp = new GradleProperty(
                "android.enableDexingArtifactTransform", "false");

            GradleProperty[] gradleProps = null;
            if (Utils.GetAndroidResolverSetting<bool>("UseJetifier"))
            {
                gradleProps = new[] { dexingArtifactProp };
            }
            else
            {
                gradleProps = new[]
                {
                    dexingArtifactProp,
                    new GradleProperty("android.useAndroidX", "true"),
                    new GradleProperty("android.enableJetifier", "true")
                };
            }

#if UNITY_2019_3_OR_NEWER
            List<string> propsFile = ReadGradleFile("Gradle Properties", Utils.propertiesGradlePath);
            if (UpdateGradlePropertiesFile(propsFile, gradleProps))
                File.WriteAllLines(Utils.propertiesGradlePath, propsFile.ToArray());
#else
            // Unity below version 2019.3 does not have a Gradle Properties file
            // and changes are applied to the base Gradle file.
            if (UpdateGradlePropertiesInMainFile( baseGradle, gradleProps, baseGradlePath ))
                baseGradleChanged = true;
#endif

            if (launcherGradle != null)
            {
                if (UpdateLauncherGradleFile(launcherGradle, settings, launcherGradlePath))
                {
#if UNITY_2019_3_OR_NEWER
                    File.WriteAllLines(launcherGradlePath, launcherGradle.ToArray());
#else
                    // Unity below version 2019.3 does not have a Gradle Launcher file
                    // and changes are applied to the base Gradle file.
                    baseGradleChanged = true;
#endif
                }
            }

            if (baseGradleChanged)
                File.WriteAllLines(baseGradlePath, baseGradle.ToArray());
        }

        internal static Version GetAndroidGradlePluginVersion()
        {
#if UNITY_2019_3_OR_NEWER
            const string path = Utils.projectGradlePath;
#else
            const string path = Utils.mainGradlePath;
#endif
            int lineIndex;
            var gradle = ReadGradleFile("Base Gradle", path);
            var version = FindLineWithAndroidGradlePluginVersion(gradle, out lineIndex);
            if (version != null)
                return version;
#if UNITY_2022_3_OR_NEWER
            return new Version(7, 1, 2);
#else
            return new Version(4, 0, 1);
#endif
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

        #region Configure gradle
        private static bool UpdateGradlePropertiesFile(List<string> propFile, GradleProperty[] props)
        {
            var isChanged = false;
#if UNITY_2019_3_OR_NEWER || CASDeveloper
            var line = 0;
            while (line < propFile.Count)
            {
                var isRemoved = false;
                for (int i = 0; i < props.Length; i++)
                {
                    if (propFile[line].Contains(props[i].name))
                    {
                        if (props[i].remove)
                        {
                            isRemoved = true;
                            propFile.RemoveAt(line);
                            Utils.Log("Remove gradle property: " + props[i].name);
                            isChanged = true;
                        }
                        else
                        {
                            props[i].exist = true;
                        }
                        break;
                    }
                }
                if (isRemoved)
                    continue;
                if (propFile[line].Contains("**ADDITIONAL_PROPERTIES**"))
                    break;
                ++line;
            }
            for (int i = 0; i < props.Length; i++)
            {
                if (!props[i].exist)
                {
                    propFile.Insert(line, props[i].name + "=" + props[i].enabled);
                    Utils.Log("Set gradle property: " + props[i].name + " = " + props[i].enabled);
                    isChanged = true;
                }
            }
#endif
            return isChanged;
        }

        private static bool UpdateGradlePropertiesInMainFile(List<string> gradle, GradleProperty[] props, string filePath)
        {
            var isChanged = false;
#if !UNITY_2019_3_OR_NEWER || CASDeveloper
            const string addBeforeLine = "apply plugin";
            const string beginPropsComment = "// CAS Properties Start";
            const string endPropsComment = "// CAS Properties End";
            var beginPropsLine = -1;
            var tryFindProp = false;

            var line = -1;
            do
            {
                ++line;
                if (line >= gradle.Count)
                {
                    LogWhenGradleLineNotFound(addBeforeLine, filePath);
                    return isChanged;
                }
                if (gradle[line].Contains("ext {"))
                {
                    tryFindProp = true;
                    continue;
                }
                if (gradle[line].StartsWith(beginPropsComment))
                {
                    beginPropsLine = line;
                    continue;
                }
                if (gradle[line].StartsWith(endPropsComment))
                {
                    if (beginPropsLine > 0)
                    {
                        var allExist = true;
                        for (int i = 0; i < props.Length; i++)
                        {
                            allExist = allExist && (props[i].exist || props[i].existByCAS);
                        }
                        if (allExist)
                            return isChanged;
                        var removeCount = line + 1 - beginPropsLine;
                        gradle.RemoveRange(beginPropsLine, removeCount);
                        line -= removeCount;
                        beginPropsLine = -1;
                    }
                    else
                    {
                        gradle.RemoveAt(line);
                        --line;
                    }
                    isChanged = true;
                    continue;
                }
                if (!tryFindProp)
                    continue;
                if (gradle[line].Contains('}'))
                {
                    tryFindProp = false;
                    continue;
                }

                for (int i = 0; i < props.Length; i++)
                {
                    if (!gradle[line].Contains(props[i].name))
                        continue;

                    if (props[i].remove)
                    {
                        gradle.RemoveAt(line);
                        Utils.Log("Remove gradle property: " + props[i].name);
                        isChanged = true;
                        --line;
                        break;
                    }
                    if (beginPropsLine < 0)
                        props[i].existByCAS = true;
                    else
                        props[i].exist = true;
                    break;
                }
            } while (!gradle[line].Contains(addBeforeLine));

            var writeToFile = false;
            var propertiesLines = new List<string>(8 + props.Length);
            propertiesLines.Add(beginPropsComment);
            propertiesLines.Add("([rootProject] + (rootProject.subprojects as List)).each {");
            propertiesLines.Add("    ext {");
            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].exist)
                    continue;
                propertiesLines.Add("        it.setProperty(\"" + props[i].name + "\", " + props[i].enabled + ")");
                Utils.Log("Set gradle property: " + props[i].name + " = " + props[i].enabled);
                writeToFile = true;
            }
            propertiesLines.Add("    }");
            propertiesLines.Add("}");
            propertiesLines.Add(endPropsComment);

            if (writeToFile)
                gradle.InsertRange(line, propertiesLines.ToArray());
            isChanged = isChanged || writeToFile;
#endif
            return isChanged;
        }

        private static bool UpdateLauncherGradleFile(List<string> gradle, CASEditorSettings settings, string filePath)
        {
            int line = 0;
            bool isChanged = false;

            // Find dependencies{} scope
            do
            {
                if (++line >= gradle.Count)
                    return isChanged;
            } while (!gradle[line].Contains(" implementation"));

            // Find Multidex dependency in scope
            const string multidexAndroidSupport = "com.android.support:multidex:";
            const string multidexAndroidX = "androidx.multidex:multidex:";
            const string exoPlayerDep = "com.google.android.exoplayer:exoplayer:";
            do
            {
                if (++line >= gradle.Count)
                    return isChanged;
                var removeLine = gradle[line].Contains(multidexAndroidSupport)
                    || gradle[line].Contains(multidexAndroidX)
                    || gradle[line].Contains(exoPlayerDep);
                if (removeLine)
                {
                    Utils.Log("Removed: '" + gradle[line] + "' from: " + filePath);
                    gradle.RemoveAt(line);
                    --line;
                    isChanged = true;
                }
            } while (!gradle[line].Contains('}'));

#if DeclareJavaVersion
            const string javaVersion = "JavaVersion.VERSION_1_8";
            var existJavaDeclaration = false;
#endif

            do // while defaultConfig scope
            {
                if (++line >= gradle.Count)
                    return isChanged;

#if DeclareJavaVersion
                if (!existJavaDeclaration && gradle[line].Contains( javaVersion ))
                    existJavaDeclaration = true;
#endif
            } while (!gradle[line].Contains("defaultConfig"));

#if DeclareJavaVersion
            if (!existJavaDeclaration)
            {
                var compileOptions = new[] {
                    "	compileOptions {",
                    "        sourceCompatibility " + javaVersion,
                    "        targetCompatibility " + javaVersion,
                    "	}",
                    ""
                };
                gradle.InsertRange( line, compileOptions );
                line += compileOptions.Length;
                isChanged = true;
                Utils.Log( "Appended Compile options to use Java Version 1.8 in " + filePath + Utils.logAutoFeature );
            }
#endif
            // Find multidexEnable in defaultConfig{} scope
            while (line < gradle.Count && !gradle[line].Contains("buildTypes"))
            {
                if (gradle[line].Contains("multiDexEnabled"))
                {
                    gradle.RemoveAt(line);
                    isChanged = true;
                    break;
                }
                line++;
            }
            return isChanged;
        }

        private static Version FindLineWithAndroidGradlePluginVersion(List<string> gradle, out int lineIndex)
        {
            // Extracts an Android Gradle Plugin version number from the contents of a *.gradle file for
            // Unity 2022.2+ or 2023.1+.
            // Example:
            //   id 'com.android.application' version '7.1.2' apply false
            const string gradlePluginVersion = "id 'com.android.application' version '";

            // Extracts an Android Gradle Plugin version number from the contents of a *.gradle file.
            // This should work for Unity 2022.1 and below.
            // Example:
            //   classpath 'com.android.tools.build:gradle:4.0.1'
            const string gradlePluginVersionLegacy = "classpath 'com.android.tools.build:gradle:";

            int beginIndex;
            lineIndex = 0;
            try
            {
                do
                {
                    ++lineIndex;
                    if (lineIndex >= gradle.Count)
                    {
                        LogWhenGradleLineNotFound(gradlePluginVersion, Utils.projectGradlePath);
                        return null;
                    }
                    beginIndex = gradle[lineIndex].IndexOf(gradlePluginVersion);
                    if (beginIndex < 0)
                    {
                        beginIndex = gradle[lineIndex].IndexOf(gradlePluginVersionLegacy);
                        if (beginIndex > 0)
                            beginIndex += gradlePluginVersionLegacy.Length;
                    }
                    else
                    {
                        beginIndex += gradlePluginVersion.Length;
                    }
                } while (beginIndex < 0);

                var currVerStr = gradle[lineIndex].Substring(beginIndex,
                    gradle[lineIndex].IndexOf('\'', beginIndex) - beginIndex);

                return new Version(currVerStr);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return null;
        }

        private static bool UpdateGradlePluginVersion(List<string> gradle, string filePath)
        {
#if UpdateGradleToolsMinorVersion || CASDeveloper
            try
            {
                int lineIndex = 0;
                Version version = FindLineWithAndroidGradlePluginVersion(gradle, out lineIndex);
                Version target = null;
#if UpdateGradleToolsForUsedWrapper
                // https://developer.android.com/studio/releases/gradle-plugin#updating-gradle
                Version wrapper = GetGradleWrapperVersion();
                if (wrapper != null)
                {
                    if (wrapper.Major == 5)
                    {
                        if (wrapper.Minor < 4)
                            target = new Version(3, 4, 3);
                        else if (wrapper.Minor < 6)
                            target = new Version(3, 5, 4);
                        else
                            target = new Version(3, 6, 4);
                    }
                    else if (wrapper.Major == 6)
                    {
                        if (wrapper.Minor < 5)
                            target = new Version(4, 0, 2);
                        else if (wrapper.Minor < 7)
                            target = new Version( 4, 1, 3 );
                        else
                            target = new Version( 4, 2, 2 );
                    }
                }
#endif
                if (target == null && version.Major == 4)
                {
                    switch (version.Minor)
                    {
                        case 0:
                        case 2:
                            if (version.Build < 2)
                                target = new Version(4, version.Minor, 2);
                            break;
                        case 1:
                            if (version.Build < 3)
                                target = new Version(4, 1, 3);
                            break;
                    }
                }
                else if (target == null && version.Major == 3)
                {
                    switch (version.Minor)
                    {
                        case 3:
                        case 4:
                            target = new Version(3, version.Minor, 3);
                            break;
                        case 5:
                        case 6:
                            target = new Version(3, version.Minor, 4);
                            break;
                    }
                }

                if (target == null)
                    return false;

                if (version.Major != target.Major
                    || version.Minor != target.Minor
                    || version.Build < target.Build)
                {
                    var oldLine = gradle[lineIndex];
                    gradle[lineIndex] = gradle[lineIndex].Replace(version.ToString(), target.ToString());
                    Utils.Log("Updated Gradle Build Tools Plugin version.\n" +
                                "From: " + oldLine + "\nTo:" + gradle[lineIndex] + Utils.logAutoFeature);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
            return false;
        }

        private static bool UpdateBaseGradleRepositories(List<string> gradle, string filePath)
        {
            var isChanged = false;
#if ReplaceJCenterToMavenCentral || CASDeveloper
            const string mavenCentralLine = "mavenCentral()";

            var isFoundCentralRepo = false;
            for (int line = 0; line < gradle.Count; line++)
            {
                // Find repositories scope
                if (!gradle[line].Contains("repositories"))
                    continue;
                line++; // Move in repositories
                isFoundCentralRepo = false;
                var beginReposLine = line;
                var scopeLevel = 1;
                while (line < gradle.Count && scopeLevel > 0)
                {
                    if (gradle[line].Contains(mavenCentralLine))
                    {
                        isFoundCentralRepo = true;
                    }
                    else if (gradle[line].Contains("jcenter()"))
                    {
                        gradle.RemoveAt(line--);
                        Utils.Log("Deprecated jCenter repository removed from " + filePath);
                        isChanged = true;
                    }

                    if (gradle[line].Contains('{'))
                        scopeLevel++;
                    if (gradle[line].Contains('}'))
                        scopeLevel--;
                    line++;
                }

                if (!isFoundCentralRepo)
                {
                    gradle.Insert(beginReposLine, "        " + mavenCentralLine);
                    Utils.Log("Maven Central repository appended to " + filePath);
                    isChanged = true;
                }
            }

            if (!isChanged && !isFoundCentralRepo)
                LogWhenGradleLineNotFound("repositories{}", filePath);
#endif
            return isChanged;
        }
        #endregion

        #region Utils
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

        private static void LogWhenGradleLineNotFound(string line, string inFile)
        {
            Debug.LogWarning(Utils.logTag + "Not found " + line + " in Gradle template.\n" +
                            "Please try to remove `" + inFile + "` and enable gradle template in Player Settings.\n");
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
        #endregion

        private class GradleProperty
        {
            public readonly string name;
            public readonly string enabled;
            public bool exist = false;
            public bool remove = false;
            public bool existByCAS = false;

            public GradleProperty(string name, string enabled)
            {
                this.name = name;
                this.enabled = enabled;
            }

            public GradleProperty(string name, string enabled, bool remove)
            {
                this.enabled = enabled;
                this.name = name;
                this.remove = remove;
                exist = remove;
            }
        }
    }
}
#endif