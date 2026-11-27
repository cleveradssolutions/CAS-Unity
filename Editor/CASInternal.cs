//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace CAS.UEditor
{
    using Utils = CASEditorUtils;

    public partial class DependencyManager
    {
#pragma warning disable CS0414
        [SerializeField]
        private string version;
#pragma warning restore CS0414
        [SerializeField]
        private Dependency[] simple;
        [SerializeField]
        private Dependency[] adapters;
        [SerializeField]
        private string[] deprecated;
        private List<OtherDependency> other = new List<OtherDependency>();

        private BuildTarget platform;

        internal GUILayoutOption columnWidth;
        internal static bool isDirt;

        private AnimBool solutionsFoldout = null;
        private AnimBool advancedFoldout = null;
        private AnimBool otherFoldout = null;

        internal BuildTarget buildTarget
        {
            get { return platform; }
        }

        internal void Init(BuildTarget platform, bool deepInit = true)
        {
            this.platform = platform;
            var managerToInit = deepInit ? this : null;
            for (int i = 0; i < simple.Length; i++)
            {
                simple[i].Reset();
            }
            for (int i = 0; i < adapters.Length; i++)
            {
                adapters[i].Reset();
            }
            // Cannot be merged Reset and Init in single loop.
            for (int i = 0; i < simple.Length; i++)
            {
                simple[i].Init(platform, managerToInit);
            }
            for (int i = 0; i < adapters.Length; i++)
            {
                adapters[i].Init(platform, managerToInit);
            }

            if (managerToInit == null)
                return;

            EnsureConsolidatedDependencies(platform);

            for (int i = 0; i < deprecated.Length; i++)
            {
                var destination = Utils.GetDependencyPath(deprecated[i], platform);
                if (File.Exists(destination))
                {
                    AssetDatabase.DeleteAsset(destination);
                    Debug.LogWarning(Utils.logTag + " The " + deprecated[i] +
                        " Adapter is no longer supported, it may have been renamed. Don't forget to include the new adapter.");
                }
            }

            other.Clear();
            var depsAssets = AssetDatabase.FindAssets("Dependencies");
            for (int i = 0; i < depsAssets.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(depsAssets[i]);
                if (!path.EndsWith(".xml") || !path.StartsWith("Assets/")
                    || !path.Contains("/Editor/")
                    || !File.Exists(path))
                    continue;
                var fileName = Path.GetFileName(path);
                if (fileName.StartsWith("CAS"))
                    continue;
                try
                {
                    var source = File.ReadAllText(path);
                    string begin = platform == BuildTarget.Android ? "<androidPackage spec=\"" : "<iosPod name=\"";
                    var beginIndex = source.IndexOf(begin);
                    while (beginIndex > 0)
                    {
                        beginIndex += begin.Length;
                        var depName = source.Substring(beginIndex, source.IndexOf('\"', beginIndex) - beginIndex);
                        other.Add(new OtherDependency(depName, path));
                        beginIndex = source.IndexOf(begin, beginIndex);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        internal void WriteConsolidated(BuildTarget platform)
        {
            string depTag = platform == BuildTarget.Android ? "androidPackage" : "iosPod";
            string sourcesTag = platform == BuildTarget.Android ? "repositories" : "sources";

            var sources = new StringBuilder();
            var packages = new StringBuilder();

            AppendBaseDependency(platform, sources, packages);

            for (int i = 0; i < simple.Length; i++)
                if (!string.IsNullOrEmpty(simple[i].installedVersion))
                    simple[i].AppendTo(platform, this, sources, packages);
            for (int i = 0; i < adapters.Length; i++)
                if (!string.IsNullOrEmpty(adapters[i].installedVersion))
                    adapters[i].AppendTo(platform, this, sources, packages);

            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>")
                   .AppendLine("<dependencies>")
                   .Append("  <").Append(depTag).Append("s>").AppendLine();
            if (platform != BuildTarget.iOS)
            {
                if (sources.Length > 0)
                {
                    builder.Append("    <").Append(sourcesTag).Append('>').AppendLine()
                           .Append(sources)
                           .Append("    </").Append(sourcesTag).Append('>').AppendLine();
                }
                builder.Append(packages);
            }
            builder.Append("  </").Append(depTag).Append("s>").AppendLine();

            if (platform == BuildTarget.iOS)
                AppendSwiftPackages(packages.ToString(), builder);

            builder.AppendLine("</dependencies>");

            Utils.WriteToAsset(Utils.GetConsolidatedDependencyPathOrDefault(platform), builder.ToString());
        }

        private void AppendSwiftPackages(string iosPodsContent, StringBuilder builder)
        {
            var libByPod = new Dictionary<string, Dependency.SDK>();
            CollectLibs(simple, libByPod);
            CollectLibs(adapters, libByPod);

            // One remoteSwiftPackage per distinct package (url + version), in first-seen order,
            // so pods hosted outside the CAS-iOS package (e.g. Tenjin) land in their own block.
            var order = new List<string>();
            var groups = new Dictionary<string, StringBuilder>();

            const string marker = "<iosPod name=\"";
            var index = iosPodsContent.IndexOf(marker);
            while (index >= 0)
            {
                index += marker.Length;
                var end = iosPodsContent.IndexOf('"', index);
                if (end < 0)
                    break;
                var pod = iosPodsContent.Substring(index, end - index);
                index = iosPodsContent.IndexOf(marker, end);

                string product, url, ver;
                bool isBase = pod == "CleverAdsSolutions-Base";
                if (isBase)
                {
                    product = "CleverAdsSolutionsSPM";
                    url = CASEditorUtils.casSpmUrl;
                    ver = version;
                }
                else
                {
                    Dependency.SDK sdk;
                    if (!libByPod.TryGetValue(pod, out sdk))
                        continue;
                    // A pod ships an SPM product when it names one (spm) or lives in its own
                    // package (spmUrl). Pods without either have no SPM alternative.
                    if (string.IsNullOrEmpty(sdk.spm) && string.IsNullOrEmpty(sdk.spmUrl))
                        continue;
                    product = string.IsNullOrEmpty(sdk.spm) ? pod : sdk.spm;
                    // Pods in their own package carry their own version; CAS pods use the plugin version.
                    url = string.IsNullOrEmpty(sdk.spmUrl) ? CASEditorUtils.casSpmUrl : sdk.spmUrl;
                    ver = string.IsNullOrEmpty(sdk.spmUrl) ? version : sdk.version;
                }

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(ver))
                    continue;

                var key = url + "\n" + ver;
                StringBuilder group;
                if (!groups.TryGetValue(key, out group))
                {
                    group = new StringBuilder();
                    groups[key] = group;
                    order.Add(key);
                }
                group.Append("    <swiftPackage name=\"").Append(product)
                     .Append("\" replacesPod=\"").Append(pod).Append('"');
                if (!isBase)
                    group.Append(" target=\"Main\"");
                group.Append("/>").AppendLine();
            }

            for (int i = 0; i < order.Count; i++)
            {
                var key = order[i];
                var sep = key.IndexOf('\n');
                builder.Append("  <remoteSwiftPackage url=\"").Append(key.Substring(0, sep))
                       .Append("\" version=\"").Append(key.Substring(sep + 1)).Append("\">").AppendLine();
                builder.Append(groups[key]);
                builder.AppendLine("  </remoteSwiftPackage>");
            }
        }

        private static void CollectLibs(Dependency[] deps, Dictionary<string, Dependency.SDK> map)
        {
            for (int i = 0; i < deps.Length; i++)
            {
                var libs = deps[i].libs;
                if (libs == null)
                    continue;
                for (int j = 0; j < libs.Length; j++)
                    map[libs[j].name] = libs[j];
            }
        }

        private void AppendBaseDependency(BuildTarget platform, StringBuilder sources, StringBuilder packages)
        {
            packages.Append("    ").Append(Dependency.consolidatedMarkerPrefix)
                    .Append(Dependency.adBaseName).Append(" version=\"").Append(version).Append("\" -->").AppendLine();
            if (platform == BuildTarget.Android)
            {
                packages.Append("    <androidPackage spec=\"com.cleveradssolutions:cas-sdk:")
                        .Append(version).Append("\" version=\"").Append(version).Append("\" />").AppendLine();
                packages.Append("    <androidPackage spec=\"androidx.lifecycle:lifecycle-process:2.6.2\" version=\"2.6.2\" />").AppendLine();
            }
            else
            {
                sources.Append("      <source>https://github.com/cleveradssolutions/CAS-Specs.git</source>").AppendLine();
                packages.Append("    <iosPod name=\"CleverAdsSolutions-Base\" version=\"")
                        .Append(version).Append("\" minTargetSdk=\"").Append(Utils.targetIOSVersion).Append(".0\" />").AppendLine();
            }
        }

        private void EnsureConsolidatedDependencies(BuildTarget platform)
        {
            if (Utils.GetConsolidatedDependencyPath(platform) != null)
                return;

            WriteConsolidated(platform);
            RemoveLegacyDependencyFiles(platform);
        }

        private void RemoveLegacyDependencyFiles(BuildTarget platform)
        {
            var consolidatedName = Utils.GetConsolidatedDependencyName(platform);
            var platformPrefix = "CAS" + platform.ToString();
            var baseName = "CAS" + Dependency.adBaseName + "Dependencies";
            foreach (var guid in AssetDatabase.FindAssets("Dependencies"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("Dependencies.xml") || !path.StartsWith("Assets/"))
                    continue;
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName == consolidatedName)
                    continue;
                if (fileName.StartsWith(platformPrefix) || fileName == baseName)
                    AssetDatabase.DeleteAsset(path);
            }
        }

        private void OnHeaderGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name", EditorStyles.largeLabel);
            GUILayout.Label("Version", EditorStyles.largeLabel, columnWidth);
            GUILayout.Label("Latest", EditorStyles.largeLabel, columnWidth);
            GUILayout.Space(25);
            EditorGUILayout.EndHorizontal();
        }

        private void CheckDependencyUpdates()
        {
            bool updatesFound = false;
            for (int i = 0; !updatesFound && i < solutions.Length; i++)
                updatesFound = solutions[i].isNewer;

            for (int i = 0; !updatesFound && i < adapters.Length; i++)
                updatesFound = adapters[i].isNewer;

            if (updatesFound)
            {
                if (HelpStyles.WarningWithButton("Found one or more updates for native dependencies.",
                    "Update all", MessageType.Error))
                {
                    UpdateDependencies();
                }
            }
        }

        internal void OnGUI(BuildTarget platform, Editor mainWindow)
        {
            if (solutionsFoldout == null)
            {
                solutionsFoldout = new AnimBool(true, mainWindow.Repaint);
                advancedFoldout = new AnimBool(false, mainWindow.Repaint);
                otherFoldout = new AnimBool(false, mainWindow.Repaint);
            }

            CheckDependencyUpdates();

            columnWidth = GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth * 0.15f);


            if (solutions.Length > 0)
            {
                HelpStyles.BeginBoxScope();
                solutionsFoldout.target = GUILayout.Toggle(solutionsFoldout.target, "Mediation Solutions", EditorStyles.foldout);

                if (EditorGUILayout.BeginFadeGroup(solutionsFoldout.faded))
                {
                    OnHeaderGUI();
                    for (int i = 0; i < solutions.Length; i++)
                        solutions[i].OnGUI(this, platform);
                }
                EditorGUILayout.EndFadeGroup();
                HelpStyles.EndBoxScope();
            }

            if (adapters.Length > 0)
            {
                HelpStyles.BeginBoxScope();
                var advancedVisible = advancedFoldout.target;
                if (advancedVisible)
                {
                    advancedFoldout.target = GUILayout.Toggle(true, "Mediation Adapters", EditorStyles.foldout);
                }
                else
                {
                    int installed = 0;
                    var forceOpen = false;

                    for (int i = 0; i < adapters.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(adapters[i].installedVersion))
                        {
                            installed++;
                            if (adapters[i].notSupported)
                            {
                                forceOpen = true;
                                Debug.LogError(Utils.logTag + adapters[i].name +
                                    " Dependencies found that are not valid to use.");
                            }
                        }
                    }
                    advancedFoldout.target = GUILayout.Toggle(forceOpen, "Mediation Adapters (" + installed + ")", EditorStyles.foldout) || forceOpen;
                }

                if (EditorGUILayout.BeginFadeGroup(advancedFoldout.faded))
                {
                    OnHeaderGUI();
                    for (int i = 0; i < adapters.Length; i++)
                        adapters[i].OnGUI(this, platform);
                }
                EditorGUILayout.EndFadeGroup();
                HelpStyles.EndBoxScope();
            }
            if (other.Count > 0)
            {
                HelpStyles.BeginBoxScope();

                otherFoldout.target = GUILayout.Toggle(otherFoldout.target,
                    "Other project dependencies: " + other.Count, EditorStyles.foldout);

                if (EditorGUILayout.BeginFadeGroup(otherFoldout.faded))
                {
                    for (int i = 0; i < other.Count; i++)
                        other[i].OnGUI(this);
                }
                EditorGUILayout.EndFadeGroup();
                HelpStyles.EndBoxScope();
            }
        }

        internal void SetAudience(Audience audience)
        {
            for (int i = 0; i < solutions.Length; i++)
                solutions[i].FilterAudience(audience);
            for (int i = 0; i < adapters.Length; i++)
                adapters[i].FilterAudience(audience);
        }

        private struct OtherDependency
        {
            public string name;
            public string path;

            public OtherDependency(string name, string path)
            {
                this.name = name;
                this.path = path;
            }

            public void OnGUI(DependencyManager mediation)
            {
                HelpStyles.Devider();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(name);
                //if (GUILayout.Button( "Select", EditorStyles.miniButton, mediation.columnWidth ))
                //{
                //    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path );
                //    EditorGUIUtility.PingObject( asset );
                //}
                if (GUILayout.Button("Remove", EditorStyles.miniButton, mediation.columnWidth))
                {
                    AssetDatabase.MoveAssetToTrash(path);
                    for (int i = mediation.other.Count - 1; i >= 0; i--)
                    {
                        if (mediation.other[i].path == path)
                            mediation.other.RemoveAt(i);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(path, EditorStyles.wordWrappedMiniLabel, GUILayout.ExpandHeight(false));
                EditorGUI.indentLevel--;
            }
        }
    }

    public partial class Dependency
    {
        internal void Reset()
        {
            isNewer = false;
            locked = false;
            isRequired = false;
            installedVersion = "";
        }

        internal static string FindInstalledVersion(string name, BuildTarget platform)
        {
            var consolidated = Utils.GetConsolidatedDependencyPath(platform);
            if (consolidated != null && File.Exists(consolidated))
                return FindVersionInConsolidated(File.ReadAllText(consolidated), name);

            var path = Utils.GetDependencyPath(name, platform);
            if (path != null && File.Exists(path))
            {
                var dependency = File.ReadAllText(path);
                const string line = "version=\"";
                var beginIndex = dependency.Length > 45 ? dependency.IndexOf(line, 45) : -1;
                if (beginIndex > 0)
                {
                    beginIndex += line.Length;
                    return dependency.Substring(beginIndex, dependency.IndexOf('\"', beginIndex) - beginIndex);
                }
            }
            return null;
        }

        internal const string consolidatedMarkerPrefix = "<!-- CAS ";

        internal static string FindVersionInConsolidated(string content, string name)
        {
            var marker = consolidatedMarkerPrefix + name + " version=\"";
            var beginIndex = content.IndexOf(marker);
            if (beginIndex < 0)
                return null;
            beginIndex += marker.Length;
            var endIndex = content.IndexOf('\"', beginIndex);
            if (endIndex < 0)
                return null;
            return content.Substring(beginIndex, endIndex - beginIndex);
        }

        internal void Init(BuildTarget platform, DependencyManager mediation)
        {
            installedVersion = FindInstalledVersion(name, platform);

            if (string.IsNullOrEmpty(installedVersion))
                return;

            try
            {
                //var currVer = new Version( installedVersion );
                //var targetVer = new Version( version );
                //isNewer = currVer < targetVer;
                isNewer = installedVersion != version;
            }
            catch
            {
                isNewer = true;
            }

            if (mediation == null)
                return;

            for (int i = 0; i < contains.Length; i++)
            {
                var item = mediation.Find(contains[i]);
                if (item != null)
                    item.locked = true;
            }

            var requiredItem = mediation.Find(require);
            if (requiredItem != null)
                requiredItem.isRequired = true;
        }

        internal void FilterAudience(Audience audience)
        {
            if (filter == Filter.None)
                notSupported = true;
            else if (audience == Audience.Mixed)
                notSupported = false;
            else if (audience == Audience.Children)
                notSupported = filter == Filter.Adult;
            else
                notSupported = filter == Filter.Children;
        }

        private void OnLabelGUI(Label label)
        {
            if (label == Label.None)
                return;
            string title = string.Empty;
            string tooltip = string.Empty;
            //if (( label & Label.Banner ) == Label.Banner)
            //{
            //    title += "b ";
            //    tooltip += "'b' - Support Banner Ad\n";
            //}
            //if (( label & Label.Inter ) == Label.Inter)
            //{
            //    title += "i ";
            //    tooltip += "'i' - Support Interstitial Ad\n";
            //}
            //if (( label & Label.Reward ) == Label.Reward)
            //{
            //    title += "r ";
            //    tooltip += "'r' - Support Rewarded Ad\n";
            //}
            if ((label & Label.Beta) == Label.Beta)
            {
                title += "beta";
                tooltip += "'beta' - Adapter in closed beta and available upon invite only. " +
                    "If you would like to be considered for the beta, please contact Support.";
            }
            //if (( label & Label.Obsolete ) == Label.Obsolete)
            //{
            //    title += "obsolete";
            //    tooltip += "'obsolete' - The mediation of the network is considered obsolete and not recommended for install.";
            //}
            if (title.Length == 0)
                return;
            GUILayout.Label(HelpStyles.GetContent(title, null, tooltip), "AssetLabel Partial");
        }

        internal void OnGUI(DependencyManager mediation, BuildTarget platform)
        {
            bool installed = !string.IsNullOrEmpty(installedVersion);
            if (!installed && filter == Filter.None)
                return;
            HelpStyles.Devider();
            EditorGUILayout.BeginHorizontal();

            string netTitle = " " + name;
            if (altName.Length != 0)
                netTitle += "/" + altName;

            if (installed || locked)
            {
                EditorGUI.BeginDisabledGroup((!installed && locked) || (installed && isRequired && !locked && !isNewer));
                if (!GUILayout.Toggle(true, netTitle, GUILayout.ExpandWidth(false)))
                    DisableDependencies(platform, mediation);
                OnLabelGUI(labels);
                GUILayout.FlexibleSpace();

                if (notSupported || (locked && installed))
                {
                    GUILayout.Label(version, mediation.columnWidth);
                    if (GUILayout.Button("Remove", EditorStyles.miniButton, mediation.columnWidth))
                    {
                        DisableDependencies(platform, mediation);
                        GUIUtility.ExitGUI();
                    }
                }
                else if (locked)
                {
                    GUILayout.Label(version, mediation.columnWidth);
                    GUILayout.Label("-", mediation.columnWidth);
                }
                else
                {
                    GUILayout.Label(installedVersion, mediation.columnWidth);
                    if (isNewer)
                    {
                        if (GUILayout.Button(version, EditorStyles.miniButton, mediation.columnWidth))
                        {
                            ActivateDependencies(platform, mediation);
                            GUIUtility.ExitGUI();
                        }
                    }
                    else
                    {
                        GUILayout.Label(isRequired ? "Required" : "-", mediation.columnWidth);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(notSupported);

                if (GUILayout.Toggle(false, netTitle, GUILayout.ExpandWidth(false)))
                {
                    ActivateDependencies(platform, mediation);
                    GUIUtility.ExitGUI();
                }
                OnLabelGUI(labels);
                GUILayout.FlexibleSpace();
                GUILayout.Label("none", mediation.columnWidth);
                if (isRequired)
                {
                    if (GUILayout.Button(version, EditorStyles.miniButton, mediation.columnWidth))
                    {
                        ActivateDependencies(platform, mediation);
                        GUIUtility.ExitGUI();
                    }
                }
                else
                {
                    GUILayout.Label(version, mediation.columnWidth);
                }
                EditorGUI.EndDisabledGroup();
            }

            if ((notSupported && installed) || (installed && locked) || (isRequired && !locked && !installed))
            {
                GUILayout.Label(HelpStyles.errorIconContent, GUILayout.Width(20));
            }
            else
            {
                GUILayout.Space(25);
            }
            EditorGUILayout.EndHorizontal();
            if (contains.Length > 0)
            {
                var footerText = new StringBuilder();
                for (int i = 0; i < contains.Length; i++)
                {
                    if (contains[i] != adBase)
                    {
                        if (footerText.Length > 0)
                            footerText.Append(", ");
                        footerText.Append(mediation.Find(contains[i]).name);
                    }
                }

                if (footerText.Length > 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(footerText.ToString(), EditorStyles.wordWrappedMiniLabel);
                    //EditorGUILayout.HelpBox( string.Join( ", ", contains ), MessageType.None );
                    EditorGUI.indentLevel--;
                }
            }

            if (!string.IsNullOrEmpty(comment))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(comment, EditorStyles.wordWrappedMiniLabel);
                EditorGUI.indentLevel--;
            }
        }

        public void DisableDependencies(BuildTarget platform, DependencyManager mediation = null)
        {
            if (string.IsNullOrEmpty(installedVersion))
                return;
            installedVersion = "";
            if (mediation == null)
                return;
            mediation.WriteConsolidated(platform);
            DependencyManager.isDirt = true;
            mediation.Init(platform);
        }

        public void ActivateDependencies(BuildTarget platform, DependencyManager mediation = null)
        {
            if (locked)
                return;

            installedVersion = version;

            if (mediation == null)
                return;

            var requireItem = mediation.Find(require);
            if (requireItem != null)
            {
                requireItem.isRequired = true;
                if (string.IsNullOrEmpty(requireItem.installedVersion))
                    requireItem.installedVersion = requireItem.version;
            }

            mediation.WriteConsolidated(platform);
            DependencyManager.isDirt = true;
            mediation.Init(platform);
        }

        internal void AppendTo(BuildTarget platform, DependencyManager mediation, StringBuilder sources, StringBuilder packages)
        {
            packages.Append("    ").Append(consolidatedMarkerPrefix)
                    .Append(name).Append(" version=\"").Append(version).Append("\" -->").AppendLine();
            AppendSources(platform, sources, mediation);
            AppendSDK(platform, mediation, packages);
        }

        private void AppendSDK(BuildTarget platform, DependencyManager mediation, StringBuilder builder)
        {
            for (int i = 0; i < libs.Length; i++)
            {
                AppendDependency(mediation, libs[i], platform, builder);
            }

            if (mediation == null)
                return;

            for (int i = 0; i < contains.Length; i++)
            {
                var item = mediation.Find(contains[i]);
                if (item != null)
                    item.AppendSDK(platform, mediation, builder);
            }
        }

        private void AppendDependency(DependencyManager mediation, SDK sdk, BuildTarget platform, StringBuilder builder)
        {
            var depTagName = platform == BuildTarget.Android ? "androidPackage" : "iosPod";
            var depAttrName = platform == BuildTarget.Android ? "spec" : "name";

            builder.Append("    <").Append(depTagName).Append(' ')
                                    .Append(depAttrName).Append("=\"").Append(sdk.name);
            if (platform == BuildTarget.Android)
                builder.Append(sdk.version);
            builder.Append("\" version=\"").Append(sdk.version).Append("\"");
            if (sdk.embed && platform == BuildTarget.iOS)
                builder.Append(" addToAllTargets=\"true\"");
            builder.Append("/>").AppendLine();
        }

        private void AppendSources(BuildTarget platform, StringBuilder builder, DependencyManager mediation)
        {
            if (source.Length > 0)
            {
                var sourceTagName = platform == BuildTarget.Android ? "repository" : "source";
                builder.Append("      <").Append(sourceTagName).Append('>')
                    .Append(source)
                    .Append("</").Append(sourceTagName).Append('>').AppendLine();
            }

            if (mediation != null)
            {
                for (int i = 0; i < contains.Length; i++)
                {
                    var item = mediation.Find(contains[i]);
                    if (item != null)
                    {
                        item.AppendSources(platform, builder, mediation);
                    }
                }
            }
        }
    }
}
