//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || CASDeveloper

// Unity 2023.3 use Gradle Wrapper 7.6 and plugin 7.3.1
// Unity 2022.2 use Gradle Wrapper 7.2 and plugin 7.1.2
// Unity 2020.1-2022.1 use Gradle Wrapper 6.1.1 and plugin 4.0.1

//#define CAS_DISABLE_PACKAGING_OPTIONS
//#define CAS_DISABLE_VALIDATE_DEPS

#if !UNITY_2022_2_OR_NEWER
// Known issue with jCenter repository where repository is not responding
// and gradle build stops with timeout error.
#define CAS_REMOVE_JCENTER
#endif

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Android;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;

namespace CAS.UEditor
{
    using Utils = CASEditorUtils;

    public class CASPostGenerateGradle : IPostGenerateGradleAndroidProject
    {
        private const string buildOptionsFile = "cas_android_build_options.gradle";
        private const string validateGMAFile = "cas_android_validate_gma.gradle";

        private readonly XNamespace androidNamespace = "http://schemas.android.com/apk/res/android";
        private readonly Regex versionRegex = new Regex("\\b\\d+(?:\\.\\d+){1,2}\\b");

#if UNITY_2022_3_OR_NEWER
        private Version gradlePluginVersion = new Version(7, 1, 2);
#else
        private Version gradlePluginVersion = new Version(4, 0, 1);
#endif

        public int callbackOrder { get { return 1000; } }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // Path is 'root/unityLibrary/' with 'build.gradle' for Unity 2019.3+
            // Path is 'root/' with 'build.gradle' for Unity 2018

            var initSettings = Utils.GetSettingsAsset(BuildTarget.Android, false);
            var editorSettings = CASEditorSettings.Load();
            var depManager = DependencyManager.Create(BuildTarget.Android, Audience.Mixed, true);

            UpdateGradleWrapper(path, editorSettings);
            UpdateRootGradleBuild(path, editorSettings);
            UpdateGradleBuild(path, gradlePluginVersion);
            UpdateGradleProperties(path);
            UpdateAppManifest(path, initSettings, editorSettings, depManager);
            AddCASConfigResources(path, initSettings);
        }

        private void UpdateGradleWrapper(string path, CASEditorSettings editorSettings)
        {
            if (string.IsNullOrEmpty(editorSettings.overrideGradleWrapperVersion))
                return;

#if UNITY_2019_3_OR_NEWER
            path = Path.Combine(path, "..");
#endif
            var propertiesPath = Path.Combine(path, "gradle/wrapper/gradle-wrapper.properties");
            if (!File.Exists(propertiesPath))
            {
                Debug.LogError(Utils.logTag + "Required file is missied: " + propertiesPath);
                return;
            }
            var lines = File.ReadAllLines(propertiesPath);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("distributionUrl="))
                {
                    if (editorSettings.overrideGradleWrapperVersion.StartsWith("http"))
                    {
                        lines[i] = editorSettings.overrideGradleWrapperVersion;
                    }
                    else
                    {
                        lines[i] = versionRegex.Replace(lines[i], editorSettings.overrideGradleWrapperVersion);
                    }
                    Utils.Log("Android Gradle Wrapper distribution url set to: " + lines[i] + "\nIn file: " + propertiesPath + Utils.logAutoFeature);
                }
            }
            File.WriteAllLines(propertiesPath, lines);
        }

        private void UpdateRootGradleBuild(string path, CASEditorSettings editorSettings)
        {
            // Extracts an Android Gradle Plugin version number from the contents of a *.gradle file for
            // Unity 2022.2+ or 2023.1+.
            // Example:
            //   id 'com.android.application' version '7.1.2' apply false
            //   id 'com.android.library' version '7.1.2' apply false
            const string pluginVersionLine = "id 'com.android.application' version '";

            // Extracts an Android Gradle Plugin version number from the contents of a *.gradle file.
            // This should work for Unity 2022.1 and below.
            // Example:
            //   classpath 'com.android.tools.build:gradle:4.0.1'
            const string pluginVersionLineLegacy = "classpath 'com.android.tools.build:gradle:";

#if UNITY_2019_3_OR_NEWER
            path = Path.Combine(path, "..");
#endif
            var gradlePath = Path.Combine(path, "build.gradle");
            if (!File.Exists(gradlePath))
            {
                Debug.LogError(Utils.logTag + "Required file is missied: " + gradlePath);
                return;
            }

            var lines = File.ReadAllLines(gradlePath);
            var updated = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(pluginVersionLine) || lines[i].Contains(pluginVersionLineLegacy))
                {
                    try
                    {
                        gradlePluginVersion = new Version(versionRegex.Match(lines[i]).Value);
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            Version newVersion = null;
            if (!string.IsNullOrEmpty(editorSettings.overrideGradlePluginVersion))
            {
                try
                {
                    newVersion = new Version(editorSettings.overrideGradlePluginVersion);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (newVersion == null)
                newVersion = GetFixedGradlePluginVersion(gradlePluginVersion);
            if (newVersion != null)
            {
                Utils.Log("Updated Android Gradle Plugin version to: " + newVersion.ToString() +
                                   " from: " + gradlePluginVersion.ToString() + "\nIn file: " + gradlePath);
                gradlePluginVersion = newVersion;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                if (newVersion != null && lines[i].Contains(" 'com.android."))
                {
                    lines[i] = versionRegex.Replace(lines[i], newVersion.ToString());
                    updated = true;
                }
#if CAS_REMOVE_JCENTER
                else if (lines[i].Contains("jcenter()"))
                {
                    lines[i] = lines[i].Replace("jcenter()", "mavenCentral()");
                    updated = true;
                }
#endif
            }

            if (updated)
                File.WriteAllLines(gradlePath, lines);
        }

        private void UpdateGradleBuild(string path, Version gradlePluginVersion)
        {
            var gradlePath = Path.Combine(path, "build.gradle");
            if (!File.Exists(gradlePath))
            {
                Debug.LogError(Utils.logTag + "Required file is missied: " + gradlePath);
                return;
            }
            var mainGradle = File.ReadAllText(gradlePath);
            var gradleChanged = false;

#if !CAS_DISABLE_PACKAGING_OPTIONS
            const string applyBuildOptions = "apply from: '" + buildOptionsFile + "'";
            if (!mainGradle.Contains(applyBuildOptions))
            {
                var optionsPath = Utils.GetPluginComponentPath(buildOptionsFile);
                if (optionsPath != null)
                {
                    File.Copy(optionsPath, Path.Combine(path, buildOptionsFile), true);
                    mainGradle += Environment.NewLine + applyBuildOptions + Environment.NewLine;
                    gradleChanged = true;
                }
            }
#endif

#if !CAS_DISABLE_VALIDATE_DEPS
            const string applyValidateGMA = "gradle.projectsEvaluated { apply from: '" + validateGMAFile + "' }";

            // if Android Gradle Plugin version is 4.2.2+
            var isSupportNewGradleTag = gradlePluginVersion.Major > 4
                    || (gradlePluginVersion.Major == 4 && gradlePluginVersion.Minor >= 2 && gradlePluginVersion.Build >= 2);

            if (!isSupportNewGradleTag && !mainGradle.Contains(applyValidateGMA))
            {
                var validationPath = Utils.GetPluginComponentPath(validateGMAFile);
                if (validationPath != null)
                {
                    File.Copy(validationPath, Path.Combine(path, validateGMAFile), true);
                    mainGradle += applyValidateGMA + Environment.NewLine;
                    gradleChanged = true;
                }
            }
#endif

            if (gradleChanged)
                File.WriteAllText(gradlePath, mainGradle);
        }

        private void UpdateGradleProperties(string path)
        {
#if UNITY_2019_3_OR_NEWER
            path = Path.Combine(path, "..");
#endif
            var propertiesPath = Path.Combine(path, "gradle.properties");
            var properties = new Dictionary<string, string>();
            if (File.Exists(propertiesPath))
            {
                var file = File.ReadAllLines(propertiesPath);
                foreach (var line in file)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 1)
                        properties[line] = "";
                    else
                        properties[parts[0]] = parts[1];
                }
            }

            properties["android.useAndroidX"] = "true";
            properties["android.enableJetifier"] = "true";
#if !UNITY_2022_2_OR_NEWER
            // Unity 6 uses Gradle version 8.4, but enableDexingArtifactTransform was removed in version 8.2.
            // Enabled by default Dexing artifact transform causes issues for ExoPlayer with Gradle plugin 3.5.0+
            properties["android.enableDexingArtifactTransform"] = "false";
#endif

            var fileContent = new StringBuilder();
            foreach (var prop in properties)
            {
                fileContent.Append(prop.Key);
                if (prop.Value.Length > 0)
                    fileContent.Append('=');
                fileContent.AppendLine(prop.Value);
            }

            try
            {
                File.WriteAllText(propertiesPath, fileContent.ToString());
            }
            catch
            {
                Debug.LogError(Utils.logTag + "Failed to update file: " + propertiesPath);
            }
        }


        private void AddCASConfigResources(string path, CASInitSettings initSettings)
        {
            if (!initSettings) return;

            var rootPath = Path.Combine(path, "src/main/res/raw");
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            for (int i = 0; i < initSettings.managersCount; i++)
            {
                string cachePath = AdRemoteConfig.GetCachePath(BuildTarget.Android, initSettings.GetManagerId(i));
                if (File.Exists(cachePath))
                {
                    try
                    {
                        string fileName = AdRemoteConfig.GetResourcesFileName(initSettings.GetManagerId(i));
                        File.Copy(cachePath, Path.Combine(rootPath, fileName), true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(Utils.logTag + "Copy Config resources to Android Project failed: " + e.ToString());
                    }
                }
                else
                {
                    Utils.Log("Not found config file: " + cachePath);
                }
            }
        }

        private void UpdateAppManifest(string path, CASInitSettings initSettings, CASEditorSettings editorSettings, DependencyManager depManager)
        {
            string manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError(Utils.logTag + "Application AndroidManifest.xml is missing in path: " + manifestPath);
                return;
            }

            XDocument manifest = XDocument.Load(manifestPath);
            var manifestElem = manifest.Element("manifest");
            if (manifestElem == null)
            {
                Debug.LogError(Utils.logTag + "Application AndroidManifest.xml is invalid, <manifest> tag not found.");
                return;
            }

            var appElem = manifestElem.Element("application");
            if (appElem == null)
            {
                Debug.LogError(Utils.logTag + "Application AndroidManifest.xml is invalid, <application> tag not found.");
                return;
            }

            // elemApplication.Add(new XElement("uses-library",
            //         new XAttribute(ns + "required", "false"),
            //         new XAttribute(nameAttribute, "org.apache.http.legacy")));


            const string permission_AD_ID = "com.google.android.gms.permission.AD_ID";
            XNamespace nsTools = "http://schemas.android.com/tools";
            XName toolsNode = nsTools + "node";
            var permissionAdIdElem = manifestElem.Descendants()
                .FirstOrDefault(element => element.Name.LocalName.Equals("uses-permission")
                    && IsElementName(element, permission_AD_ID));
            if (permissionAdIdElem == null)
            {
                permissionAdIdElem = new XElement("uses-permission",
                    new XAttribute(androidNamespace + "name", permission_AD_ID));
                manifestElem.Add(permissionAdIdElem);
            }

            if (editorSettings.isUseAdvertiserIdLimited(initSettings.defaultAudienceTagged))
            {
                manifestElem.SetAttributeValue(XNamespace.Xmlns + "tools", nsTools);
                permissionAdIdElem.SetAttributeValue(toolsNode, "remove");
            }
            else
            {
                permissionAdIdElem.SetAttributeValue(toolsNode, null);
            }

            var metaDataElems = appElem.Descendants()
                .Where(element => element.Name.LocalName.Equals("meta-data"));

            var googleAppId = AdRemoteConfig.FindGADAppId(initSettings, depManager);
            if (!string.IsNullOrEmpty(googleAppId))
            {
                UpdateMetaData(appElem, metaDataElems, "com.google.android.gms.ads.APPLICATION_ID", googleAppId);
            }

            if (editorSettings.optimizeGADLoading)
            {
                UpdateMetaData(appElem, metaDataElems, "com.google.android.gms.ads.flag.OPTIMIZE_INITIALIZATION", "true");
                UpdateMetaData(appElem, metaDataElems, "com.google.android.gms.ads.flag.OPTIMIZE_AD_LOADING", "true");
            }

            manifest.Save(manifestPath);
        }

        private bool IsElementName(XElement element, string name)
        {
            var attribute = element.Attribute(androidNamespace + "name");
            return attribute != null && attribute.Value == name;
        }

        private void UpdateMetaData(XElement appElem, IEnumerable<XElement> metaDataElements, string name, string value)
        {
            foreach (var element in metaDataElements)
            {
                if (IsElementName(element, name))
                {
                    element.SetAttributeValue(androidNamespace + "value", value);
                    return;
                }
            }
            appElem.Add(
                new XElement("meta-data",
                    new XAttribute(androidNamespace + "name", name),
                    new XAttribute(androidNamespace + "value", value))
            );
        }


        private Version GetFixedGradlePluginVersion(Version current)
        {
#if UpdateGradleToolsMinorVersion || CASDeveloper
            Version target = null;

            if (current.Major == 4)
            {
                switch (current.Minor)
                {
                    case 0:
                    case 2:
                        if (current.Build < 2)
                            target = new Version(4, current.Minor, 2);
                        break;
                    case 1:
                        if (current.Build < 3)
                            target = new Version(4, 1, 3);
                        break;
                }
            }
            else if (current.Major == 3)
            {
                switch (current.Minor)
                {
                    case 3:
                    case 4:
                        target = new Version(3, current.Minor, 3);
                        break;
                    case 5:
                    case 6:
                        target = new Version(3, current.Minor, 4);
                        break;
                }
            }

            if (target != null)
            {
                if (current.Major != target.Major
                    || current.Minor != target.Minor
                    || current.Build < target.Build)
                {
                    return target;
                }
            }
#endif
            return null;
        }
    }
}

#endif
