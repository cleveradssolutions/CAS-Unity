#if UNITY_ANDROID || CASDeveloper

#define ExcludeAndroidxAnnotations
#define DeclareJavaVersion

// Enabled by default Dexing artifact transform causes issues for ExoPlayer with Gradle plugin 3.5.0+
#define DisableDexingArtifactTransform

// Many SDKs use the new <queries> element for Android 11 in their bundled Android Manifest files.
// The Android Gradle plugin version should support new elements, else this will cause build errors:
// Android resource linking failed
// error: unexpected element <queries> found in <manifest>.
#define UpdateGradleToSupportAndroid11

// Known issue with jCenter repository where repository is not responding
// and gradle build stops with timeout error.
#define ReplaceJCenterToMavenCentral

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Utils = CAS.UEditor.CASEditorUtils;

namespace CAS.UEditor
{
    internal static class CASPreprocessGradle
    {
        internal static void Configurate( CASEditorSettings settings )
        {
            bool multidexRequired = settings.multiDexEnabled;

#if UNITY_2019_3_OR_NEWER
            UpdateProjectGradleFileForUnity2019();
            UpdateGradlePropertiesFileForUnity2019();

            const string gradlePath = Utils.launcherGradlePath;
#else
            const string gradlePath = Utils.mainGradlePath;
#endif

            if (!File.Exists( gradlePath ))
            {
#if UNITY_2019_3_OR_NEWER
                if (!multidexRequired)
                    return;
                Utils.StopBuildWithMessage(
                    "Build failed because CAS could not enable MultiDEX in Unity 2019.3 without " +
                    "Custom Launcher Gradle Template. Please enable 'Custom Launcher Gradle Template' found under " +
                    "'Player Settings -> Settings for Android -> Publishing Settings' menu.", BuildTarget.NoTarget );
#else
                Debug.LogWarning( Utils.logTag + "We recommended enable 'Custom Gradle Template' found under " +
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu to allow CAS update Gradle plugin version." );
                return;
#endif
            }
            List<string> gradle = new List<string>( File.ReadAllLines( gradlePath ) );

#if !UNITY_2019_3_OR_NEWER
            gradle = UpdateBaseGradleFile( gradle, gradlePath );
#endif

            gradle = UpdateLauncherGradleFile( gradle, multidexRequired, gradlePath );

            File.WriteAllLines( gradlePath, gradle.ToArray() );
            AssetDatabase.ImportAsset( gradlePath );
        }

        private static void UpdateGradlePropertiesFileForUnity2019()
        {
#if DisableDexingArtifactTransform
            const string dexingPropertyName = "android.enableDexingArtifactTransform";

            const string propertiesPath = Utils.propertiesGradlePath;
            if (!File.Exists( propertiesPath ))
            {
                Utils.StopBuildWithMessage(
                    "Please enable 'Custom Gradle Properties Template' found under " +
                    "'Player Settings > Settings for Android -> Publishing Settings' menu " +
                    "to allow CAS enable Jetifier, AndroidX.", BuildTarget.NoTarget );
                return;
            }
            var dexingExist = false;
            var properties = File.ReadAllLines( propertiesPath );
            for (int i = 0; i < properties.Length; i++)
            {
                var line = properties[i];
                if (line.Contains( dexingPropertyName ))
                {
                    if (!line.EndsWith( "false" ))
                    {
                        properties[i] = dexingPropertyName + "=false";
                        File.WriteAllLines( propertiesPath, properties );
                    }
                    dexingExist = true;
                    break;
                }
            }
            if (!dexingExist)
            {
                var propsList = new List<string>( properties );
                propsList.Add( dexingPropertyName + "=false" );
                File.WriteAllLines( propertiesPath, propsList.ToArray() );
            }
#endif
        }

        private static void UpdateProjectGradleFileForUnity2019()
        {
            const string projectGradlePath = Utils.projectGradlePath;
            if (!File.Exists( projectGradlePath ))
            {
                Utils.StopBuildWithMessage(
                    "Build failed because CAS could not update Gradle plugin version in Unity 2019.3 without " +
                    "Custom Base Gradle Template. Please enable 'Custom Base Gradle Template' found under " +
                    "'Player Settings -> Settings for Android -> Publishing Settings' menu.", BuildTarget.NoTarget );
                return;
            }
            try
            {
                var projectGradle = new List<string>( File.ReadAllLines( projectGradlePath ) );
                projectGradle = UpdateBaseGradleFile( projectGradle, projectGradlePath );
                File.WriteAllLines( projectGradlePath, projectGradle.ToArray() );
                AssetDatabase.ImportAsset( projectGradlePath );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        private static List<string> UpdateLauncherGradleFile( List<string> gradle, bool required, string filePath )
        {
            int line = 0;

#if DisableDexingArtifactTransform
            const string dexingPropertyName = "android.enableDexingArtifactTransform";
            var dexingPropertyDeclaration = new[] {
                "([rootProject] + (rootProject.subprojects as List)).each {",
                "    ext {",
                "        it.setProperty(\"" + dexingPropertyName + "\", false)",
                "    }",
                "}"
            };

            bool existDexingArtifact = false;
#if !UNITY_2019_3_OR_NEWER
            // For Unity 2019+ Gradle properties are written in the template file
            // by UpdateGradlePropertiesFileForUnity2019()
            existDexingArtifact = true;
#endif
#endif

            // Find dependencies{} scope
            do
            {
                ++line;
                if (line >= gradle.Count)
                {
                    if (required)
                        logWhenGradleLineNotFound( "dependencies{} scope", filePath );
                    return gradle;
                }
#if DisableDexingArtifactTransform
                if (!existDexingArtifact)
                {
                    if (gradle[line].Contains( dexingPropertyName ))
                    {
                        existDexingArtifact = true;
                    }
                    else if (gradle[line].Contains( "apply plugin" ))
                    {
                        gradle.InsertRange( line, dexingPropertyDeclaration );
                        line += dexingPropertyDeclaration.Length;
                        existDexingArtifact = true;
                    }
                }
#endif
            } while (!gradle[line].Contains( "implementation" ));

#if DisableDexingArtifactTransform
            if (!existDexingArtifact)
            {
                // Insert before dependencies{} scope.
                // Line on first implementation
                gradle.InsertRange( line - 1, dexingPropertyDeclaration );
                line += dexingPropertyDeclaration.Length;
                existDexingArtifact = true;
            }
#endif

            // Find Multidex dependency in scope
            bool multidexExist = false;
            const string multidexAndroidSupport = "'com.android.support:multidex:'";
            const string multidexAndroidX = "'androidx.multidex:multidex:2.0.1'";
            const string miltidexAndroidXLine = "    implementation " + multidexAndroidX + "// Add by CAS settings";
            do
            {
                ++line;
                if (line >= gradle.Count)
                {
                    if (required)
                        logWhenGradleLineNotFound( "dependencies{} scope", filePath );
                    return gradle;
                }
                if (gradle[line].Contains( multidexAndroidSupport ))
                {
                    if (required)
                    {
                        gradle[line] = miltidexAndroidXLine;
                        Debug.Log( Utils.logTag + "Updated " + multidexAndroidSupport +
                            " to " + multidexAndroidX );
                    }
                    else
                    {
                        gradle.RemoveAt( line );
                    }
                    multidexExist = true;
                    break;
                }
                if (gradle[line].Contains( multidexAndroidX ))
                {
                    if (!required)
                        gradle.RemoveAt( line );
                    multidexExist = true;
                    break;
                }
            } while (!gradle[line].Contains( '}' ));

            if (required)
            {
                gradle.Insert( line, miltidexAndroidXLine );
                Debug.Log( Utils.logTag + "Appended " + multidexAndroidX );
                multidexExist = true;
                ++line;
            }

#if DeclareJavaVersion
            const string javaVersion = "JavaVersion.VERSION_1_8";
            var existJavaDeclaration = false;
#endif

#if ExcludeAndroidxAnnotations
            const string excludeOption = "exclude 'META-INF/proguard/androidx-annotations.pro'";
            var packagingOptExist = false;
#endif

            do // while defaultConfig scope
            {
                ++line;
                if (line >= gradle.Count)
                {
                    if (required)
                        logWhenGradleLineNotFound( "defaultConfig{} scope", filePath );
                    return gradle;
                }
#if DeclareJavaVersion
                if (!existJavaDeclaration && gradle[line].Contains( javaVersion ))
                    existJavaDeclaration = true;
#endif
#if ExcludeAndroidxAnnotations
                if (!packagingOptExist && gradle[line].Contains( excludeOption ))
                    packagingOptExist = true;
#endif
            } while (!gradle[line].Contains( "defaultConfig" ));

#if DeclareJavaVersion
            if (!existJavaDeclaration && required)
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
                Debug.Log( Utils.logTag + "Appended Compile options to use Java Version 1.8." );
            }
#endif

#if ExcludeAndroidxAnnotations
            if (!packagingOptExist && required)
            {
                var packagingOptions = new[] {
                    "	packagingOptions {",
                    "        " + excludeOption,
                    "	}",
                    ""
                };
                gradle.InsertRange( line, packagingOptions );
                line += packagingOptions.Length;
                Debug.Log( Utils.logTag + "Appended Packaging options to exclude duplicate files." );
            }
#endif

            // Find multidexEnable in defaultConfig{} scope
            const string multidexConfig = "multiDexEnabled true";
            if (multidexExist)
            {
                var firstLineInDefaultConfigScope = line + 1;
                multidexExist = false;
                while (line < gradle.Count && !gradle[line].Contains( "buildTypes" ))
                {
                    if (gradle[line].Contains( multidexConfig ))
                    {
                        if (!required)
                            gradle.RemoveAt( line );
                        multidexExist = true;
                        break;
                    }
                    line++;
                }

                if (!multidexExist && required)
                {
                    gradle.Insert( firstLineInDefaultConfigScope,
                        "        " + multidexConfig + "// Enabled by CAS settings" );
                    Debug.Log( Utils.logTag + "Enable Multidex in Default Config" );
                }
            }
            return gradle;
        }

        private static List<string> UpdateBaseGradleFile( List<string> gradle, string filePath )
        {
            int line = 0;
#if UpdateGradleToSupportAndroid11
            const string gradlePluginVersion = "classpath 'com.android.tools.build:gradle:";
            // Find Gradle Plugin Version
            do
            {
                ++line;
                if (gradle.Count - 1 < line)
                {
                    logWhenGradleLineNotFound( "com.android.tools.build:gradle", filePath );
                    return gradle;
                }
            } while (!gradle[line].Contains( gradlePluginVersion ));

            var versionLine = gradle[line];
            var index = versionLine.IndexOf( gradlePluginVersion );
            if (index > 0)
            {
                try
                {
                    var version = new Version( versionLine.Substring( index + gradlePluginVersion.Length, 5 ) );
                    if (version.Major == 4)
                    {
                        if (version.Minor == 0 && version.Build < 1)
                            UpdateGradleBuildToolsVersion( gradle, line, '1' );
                    }
                    else if (version.Major == 3)
                    {
                        switch (version.Minor)
                        {
                            case 3:
                            case 4:
                                if (version.Build < 3)
                                    UpdateGradleBuildToolsVersion( gradle, line, '3' );
                                break;
                            case 5:
                            case 6:
                                if (version.Build < 4)
                                    UpdateGradleBuildToolsVersion( gradle, line, '4' );
                                break;
                        }
                    }
                }
                catch { }
            }
#endif

#if ReplaceJCenterToMavenCentral
            // Find allprojects { repositories { } }
            do
            {
                ++line;
                if (gradle.Count - 1 < line)
                {
                    logWhenGradleLineNotFound( "repositories{}", filePath );
                    return gradle;
                }
            } while (!gradle[line].Contains( "repositories" ));

            ++line; // Move in repositories
            // Add MavenCentral repo and remove deprecated jcenter repo
            const string mavenCentralLine = "mavenCentral()";
            var mavenCentralExist = false;
            var beginReposLine = line;
            while (line < gradle.Count)
            {
                if (gradle[line].Contains( "jcenter()" ))
                {
                    gradle.RemoveAt( line );
                    Debug.Log( Utils.logTag + "Deprecated jCenter repository removed" );
                }
                else if (gradle[line].Contains( mavenCentralLine ))
                {
                    mavenCentralExist = true;
                }
                else if (gradle[line].Contains( '}' ))
                {
                    if (!mavenCentralExist)
                    {
                        gradle.Insert( beginReposLine, "        " + mavenCentralLine );
                        Debug.Log( Utils.logTag + "Maven Central repository appended" );
                    }
                    break;
                }
                ++line;
            }
#endif
            return gradle;
        }

        private static void UpdateGradleBuildToolsVersion( List<string> gradle, int inLine, char version )
        {
            var versionLine = gradle[inLine];
            var builder = new StringBuilder( versionLine );
            builder[builder.Length - 2] = version;
            var newLine = builder.ToString();
            Debug.Log( Utils.logTag + "Updated Gradle Build Tools version to support Android 11.\n" +
                "From: " + gradle[inLine] + "\nTo:" + newLine );
            gradle[inLine] = newLine;
        }

        private static void logWhenGradleLineNotFound( string line, string inFile )
        {
            Debug.LogWarning( Utils.logTag + "Not found " + line + " in Gradle template.\n" +
                            "Please try to remove `" + inFile + "` and enable gradle template in Player Settings." );
        }
    }
}
#endif