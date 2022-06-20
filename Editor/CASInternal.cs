//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;
using Utils = CAS.UEditor.CASEditorUtils;

namespace CAS.UEditor
{
    public partial class DependencyManager
    {
        #region Internal implementation
        [SerializeField]
        private Dependency[] simple;
        [SerializeField]
        private Dependency[] advanced;
        private List<AdDependency> other = new List<AdDependency>();

        internal GUILayoutOption columnWidth;

        internal bool installedAny;

        private AnimBool solutionsFoldout = null;
        private AnimBool advancedFoldout = null;
        private AnimBool otherFoldout = null;

        internal void Init( BuildTarget platform, bool deepInit = true )
        {
            installedAny = false;
            for (int i = 0; i < simple.Length; i++)
                simple[i].Reset();
            for (int i = 0; i < advanced.Length; i++)
                advanced[i].Reset();

            for (int i = 0; i < simple.Length; i++)
                simple[i].Init( this, platform, deepInit );
            for (int i = 0; i < advanced.Length; i++)
                advanced[i].Init( this, platform, deepInit );

            if (!deepInit)
                return;
            other.Clear();
            var depsAssets = AssetDatabase.FindAssets( "Dependencies" );
            for (int i = 0; i < depsAssets.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath( depsAssets[i] );
                if (!path.EndsWith( ".xml" ) || !path.StartsWith( "Assets/" )
                    || !path.Contains( "/Editor/" )
                    || !File.Exists( path ))
                    continue;
                var fileName = Path.GetFileName( path );
                if (fileName.StartsWith( "CAS" ))
                    continue;
                try
                {
                    var source = File.ReadAllText( path );
                    string begin = platform == BuildTarget.Android ? "<androidPackage spec=\"" : "<iosPod name=\"";
                    var beginIndex = source.IndexOf( begin );
                    while (beginIndex > 0)
                    {
                        beginIndex += begin.Length;
                        var depName = source.Substring( beginIndex, source.IndexOf( '\"', beginIndex ) - beginIndex );
                        other.Add( new AdDependency( depName, path ) );
                        beginIndex = source.IndexOf( begin, beginIndex );
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException( e );
                }
            }
        }

        internal bool IsNewerVersionFound()
        {
            for (int i = 0; i < solutions.Length; i++)
            {
                if (solutions[i].isNewer)
                    return true;
            }
            for (int i = 0; i < networks.Length; i++)
            {
                if (networks[i].isNewer)
                    return true;
            }
            return false;
        }

        internal string GetInstalledVersion()
        {
            string version = "";
            var casDep = Find( Dependency.adOptimalName );
            if (casDep != null)
                version = casDep.installedVersion;
            if (!string.IsNullOrEmpty( version ))
                return version;
            casDep = Find( Dependency.adFamiliesName );
            if (casDep != null)
                version = casDep.installedVersion;

            if (!string.IsNullOrEmpty( version ))
                return version;
            casDep = Find( Dependency.adBaseName );
            if (casDep != null)
                version = casDep.version;

            return version;
        }

        internal int GetInstalledBuildCode()
        {
            var version = GetInstalledVersion();
            if (!string.IsNullOrEmpty( version ))
            {
                try
                {
                    var parsesV = new System.Version( version );
                    return parsesV.Major * 1000 + parsesV.Minor * 100 + parsesV.Build;
                }
                catch { }
            }
            return 0;
        }

        private void OnHeaderGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label( "Name", EditorStyles.largeLabel );
            GUILayout.Label( "Version", EditorStyles.largeLabel, columnWidth );
            GUILayout.Label( "Latest", EditorStyles.largeLabel, columnWidth );
            GUILayout.Space( 25 );
            EditorGUILayout.EndHorizontal();
        }

        private void CheckDependencyUpdates( BuildTarget platform )
        {
            bool updatesFound = false;
            for (int i = 0; !updatesFound && i < simple.Length; i++)
                updatesFound = simple[i].isNewer;

            for (int i = 0; !updatesFound && i < advanced.Length; i++)
                updatesFound = advanced[i].isNewer;

            if (updatesFound)
            {
                if (HelpStyles.WarningWithButton( "Found one or more updates for native dependencies.",
                    "Update all", MessageType.Info ))
                {
                    UpdateDependencies( platform );
                }
            }
        }

        internal void UpdateDependencies( BuildTarget platform )
        {
            for (int i = 0; i < simple.Length; i++)
            {
                if (simple[i].filter == -1) // remove deprecated
                    simple[i].DisableDependencies( platform, this );
                else if (simple[i].isNewer)
                    simple[i].ActivateDependencies( platform, this );
            }

            for (int i = 0; i < advanced.Length; i++)
            {
                if (advanced[i].filter == -1) // remove deprecated
                    advanced[i].DisableDependencies( platform, this );
                else if (advanced[i].isNewer)
                    advanced[i].ActivateDependencies( platform, this );
            }
        }

        internal void OnGUI( BuildTarget platform, Editor mainWindow )
        {
            if (!installedAny)
                EditorGUILayout.HelpBox( "Dependencies of native SDK were not found. " +
                    "Please use the following options to integrate solutions or any SDK separately.",
                    MessageType.Error );

            if (solutionsFoldout == null)
            {
                solutionsFoldout = new AnimBool( true, mainWindow.Repaint );
                advancedFoldout = new AnimBool( false, mainWindow.Repaint );
                otherFoldout = new AnimBool( false, mainWindow.Repaint );
            }

            CheckDependencyUpdates( platform );

            columnWidth = GUILayout.MaxWidth( EditorGUIUtility.currentViewWidth * 0.15f );


            if (simple.Length > 0)
            {
                HelpStyles.BeginBoxScope();
                solutionsFoldout.target = GUILayout.Toggle( solutionsFoldout.target, "Mediation Solutions", EditorStyles.foldout );

                if (EditorGUILayout.BeginFadeGroup( solutionsFoldout.faded ))
                {
                    OnHeaderGUI();
                    for (int i = 0; i < simple.Length; i++)
                        simple[i].OnGUI( this, platform );
                }
                EditorGUILayout.EndFadeGroup();
                HelpStyles.EndBoxScope();
            }

            if (advanced.Length > 0)
            {
                HelpStyles.BeginBoxScope();
                var advancedVisible = advancedFoldout.target;
                if (advancedVisible)
                {
                    advancedFoldout.target = GUILayout.Toggle( true, "Mediation Adapters", EditorStyles.foldout );
                }
                else
                {
                    int installed = 0;

                    for (int i = 0; i < advanced.Length; i++)
                    {
                        if (advanced[i].installedVersion.Length > 0)
                        {
                            installed++;
                            if (advanced[i].notSupported && Event.current.type == EventType.Repaint)
                            {
                                advancedFoldout.target = true;
                                Debug.LogError( Utils.logTag + advanced[i].name +
                                    " Dependencies found that are not valid for the applications of the selected audience." );
                            }
                        }
                    }
                    advancedFoldout.target = GUILayout.Toggle( false, "Mediation Adapters (" + installed + ")", EditorStyles.foldout );
                }

                if (EditorGUILayout.BeginFadeGroup( advancedFoldout.faded ))
                {
                    OnHeaderGUI();
                    for (int i = 0; i < advanced.Length; i++)
                        advanced[i].OnGUI( this, platform );
                }
                EditorGUILayout.EndFadeGroup();
                HelpStyles.EndBoxScope();
            }
            if (other.Count > 0)
            {
                HelpStyles.BeginBoxScope();

                solutionsFoldout.target = GUILayout.Toggle( solutionsFoldout.target,
                    "Other project dependencies: " + other.Count, EditorStyles.foldout );

                if (EditorGUILayout.BeginFadeGroup( otherFoldout.faded ))
                {
                    for (int i = 0; i < other.Count; i++)
                        other[i].OnGUI( this );
                }
                EditorGUILayout.EndFadeGroup();
                HelpStyles.EndBoxScope();
            }
        }

        internal void SetAudience( Audience audience )
        {
            for (int i = 0; i < simple.Length; i++)
                simple[i].FilterAudience( audience );
            for (int i = 0; i < advanced.Length; i++)
                advanced[i].FilterAudience( audience );
        }

        private struct AdDependency
        {
            public string name;
            public string path;

            public AdDependency( string name, string path )
            {
                this.name = name;
                this.path = path;
            }

            public void OnGUI( DependencyManager mediation )
            {
                HelpStyles.Devider();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label( name );
                //if (GUILayout.Button( "Select", EditorStyles.miniButton, mediation.columnWidth ))
                //{
                //    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path );
                //    EditorGUIUtility.PingObject( asset );
                //}
                if (GUILayout.Button( "Remove", EditorStyles.miniButton, mediation.columnWidth ))
                {
                    AssetDatabase.MoveAssetToTrash( path );
                    for (int i = mediation.other.Count - 1; i >= 0; i--)
                    {
                        if (mediation.other[i].path == path)
                            mediation.other.RemoveAt( i );
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField( path, EditorStyles.wordWrappedMiniLabel, GUILayout.ExpandHeight( false ) );
                EditorGUI.indentLevel--;
            }
        }
        #endregion
    }

    public partial class Dependency
    {
        #region Internal implementation
        internal void Reset()
        {
            isNewer = false;
            locked = false;
            isRequired = false;
            installedVersion = "";
        }

        internal void Init( DependencyManager mediation, BuildTarget platform, bool updateContaines )
        {
            var path = Utils.GetDependencyPath( name, platform );
            if (File.Exists( path ))
            {
                var dependency = File.ReadAllText( path );
                const string line = "version=\"";
                var beginIndex = dependency.IndexOf( line, 45 );
                if (beginIndex > 0)
                {
                    beginIndex += line.Length;
                    installedVersion = dependency.Substring( beginIndex, dependency.IndexOf( '\"', beginIndex ) - beginIndex );
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
                    mediation.installedAny = true;
                }

                if (updateContaines)
                {
                    for (int i = 0; i < contains.Length; i++)
                    {
                        var item = mediation.Find( contains[i] );
                        if (item != null)
                            item.locked = true;
                    }

                    var requiredItem = mediation.Find( require );
                    if (requiredItem != null)
                        requiredItem.isRequired = true;
                }
            }
        }

        internal void FilterAudience( Audience audience )
        {
            if (filter < 0)
                notSupported = true;
            else if (audience == Audience.Mixed)
                notSupported = false;
            else if (audience == Audience.Children)
                notSupported = filter < 1;
            else
                notSupported = filter > 1;
        }

        private void OnLabelGUI( Label label )
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
            if (( label & Label.Beta ) == Label.Beta)
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
            GUILayout.Label( HelpStyles.GetContent( title, null, tooltip ), "AssetLabel Partial" );
        }

        internal void OnGUI( DependencyManager mediation, BuildTarget platform )
        {
            bool installed = !string.IsNullOrEmpty( installedVersion );
            if (notSupported && filter < 0 && !installed)
                return;
            HelpStyles.Devider();
            EditorGUILayout.BeginHorizontal();

            string sdkVersion = null;
            for (int i = 0; i < depsSDK.Count; i++)
            {
                if (depsSDK[i].version != version)
                {
                    sdkVersion = depsSDK[i].version;
                    break;
                }
            }

            if (installed || locked)
            {
                EditorGUI.BeginDisabledGroup( ( !installed && locked ) || ( installed && isRequired && !locked ) );
                if (!GUILayout.Toggle( true, " " + name + altName, GUILayout.ExpandWidth( false ) ))
                    DisableDependencies( platform, mediation );
                if (sdkVersion != null)
                    GUILayout.Label( sdkVersion, EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth( false ) );
                OnLabelGUI( labels );
                GUILayout.FlexibleSpace();

                if (notSupported || ( locked && installed ))
                {
                    GUILayout.Label( version, mediation.columnWidth );
                    if (GUILayout.Button( "Remove", EditorStyles.miniButton, mediation.columnWidth ))
                    {
                        DisableDependencies( platform, mediation );
                        GUIUtility.ExitGUI();
                    }
                }
                else if (locked)
                {
                    GUILayout.Label( version, mediation.columnWidth );
                    GUILayout.Label( "-", mediation.columnWidth );
                }
                else
                {
                    GUILayout.Label( installedVersion, mediation.columnWidth );
                    if (isNewer)
                    {
                        if (GUILayout.Button( version, EditorStyles.miniButton, mediation.columnWidth ))
                        {
                            ActivateDependencies( platform, mediation );
                            GUIUtility.ExitGUI();
                        }
                    }
                    else
                    {
                        GUILayout.Label( isRequired ? "Required" : "-", mediation.columnWidth );
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.BeginDisabledGroup( notSupported || ( dependencies.Length == 0 && depsSDK.Count == 0 ) );

                if (GUILayout.Toggle( false, " " + name + altName, GUILayout.ExpandWidth( false ) ))
                {
                    ActivateDependencies( platform, mediation );
                    GUIUtility.ExitGUI();
                }
                if (sdkVersion != null)
                    GUILayout.Label( sdkVersion, EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth( false ) );
                OnLabelGUI( labels );
                GUILayout.FlexibleSpace();
                GUILayout.Label( "none", mediation.columnWidth );
                if (isRequired)
                {
                    if (GUILayout.Button( version, EditorStyles.miniButton, mediation.columnWidth ))
                    {
                        ActivateDependencies( platform, mediation );
                        GUIUtility.ExitGUI();
                    }
                }
                else
                {
                    GUILayout.Label( version, mediation.columnWidth );
                }
                EditorGUI.EndDisabledGroup();
            }

            if (( notSupported && installed ) || ( installed && locked ) || ( isRequired && !locked && !installed ))
                GUILayout.Label( HelpStyles.errorIconContent, GUILayout.Width( 20 ) );
            else if (string.IsNullOrEmpty( url ))
                GUILayout.Space( 25 );
            else if (GUILayout.Button( HelpStyles.helpIconContent, EditorStyles.label, GUILayout.Width( 20 ) ))
                Application.OpenURL( url );

            EditorGUILayout.EndHorizontal();
            if (contains.Length > 0)
            {
                var footerText = new StringBuilder();
                for (int i = 0; i < contains.Length; i++)
                {
                    if (contains[i] != adBase)
                    {
                        if (footerText.Length > 0)
                            footerText.Append( ", " );
                        footerText.Append( contains[i] );
                    }
                }

                if (footerText.Length > 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField( footerText.ToString(), EditorStyles.wordWrappedMiniLabel );
                    //EditorGUILayout.HelpBox( string.Join( ", ", contains ), MessageType.None );
                    EditorGUI.indentLevel--;
                }
            }

            if (!string.IsNullOrEmpty( comment ))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField( comment, EditorStyles.wordWrappedMiniLabel );
                EditorGUI.indentLevel--;
            }
        }

        public void DisableDependencies( BuildTarget platform, DependencyManager mediation )
        {
            var destination = Utils.GetDependencyPath( name, platform );
            if (File.Exists( destination ))
            {
                AssetDatabase.DeleteAsset( destination );
                installedVersion = "";
                mediation.Init( platform );
            }
        }

        public void ActivateDependencies( BuildTarget platform, DependencyManager mediation )
        {
            if (dependencies.Length == 0 && depsSDK.Count == 0)
            {
                Debug.LogError( Utils.logTag + name + " have no dependencies. Please try reimport CAS package." );
                return;
            }
            if (locked)
            {
                return;
            }

            string depTagName = platform == BuildTarget.Android ? "androidPackage" : "iosPod";
            var destination = Utils.GetDependencyPathOrDefault( name, platform );
            EditorUtility.DisplayProgressBar( "Create dependency", destination, 0.2f );

            try
            {
                var builder = new StringBuilder();
                builder.AppendLine( "<?xml version=\"1.0\" encoding=\"utf-8\"?>" )
                       .AppendLine( "<dependencies>" )
                       .Append( "  <" ).Append( depTagName ).Append( "s>" ).AppendLine();

                for (int i = 0; i < dependencies.Length; i++)
                {
                    AppendDependency( mediation, new SDK( dependencies[i], version ), platform, builder );
                }

                // EDM4U have a bug.
                // Dependencies that will be added For All Targets must be at the end of the list of dependencies.
                // Otherwise, those dependencies that should not be for all targets will be tagged for all targets.
                AppendSDK( platform, mediation, builder, false );
                AppendSDK( platform, mediation, builder, true );

                builder.Append( "  </" ).Append( depTagName ).Append( "s>" ).AppendLine()
                       .AppendLine( "</dependencies>" );

                var replace = File.Exists( destination );
                if (!replace)
                {
                    var destDir = Path.GetDirectoryName( destination );
                    if (!Directory.Exists( destDir ))
                        Directory.CreateDirectory( destDir );
                }

                File.WriteAllText( destination, builder.ToString() );
                if (!replace)
                    AssetDatabase.ImportAsset( destination );

                Init( mediation, platform, true );
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            var requireItem = mediation.Find( require );
            if (requireItem != null)
            {
                requireItem.isRequired = true;
                if (!requireItem.IsInstalled())
                    requireItem.ActivateDependencies( platform, mediation );
            }
        }

        private void AppendSDK( BuildTarget platform, DependencyManager mediation, StringBuilder builder, bool allowAllTargets )
        {
            for (int i = 0; i < depsSDK.Count; i++)
            {
                if (allowAllTargets == depsSDK[i].addToAllTargets)
                    AppendDependency( mediation, depsSDK[i], platform, builder );
            }

            for (int i = 0; i < contains.Length; i++)
            {
                var item = mediation.Find( contains[i] );
                if (item != null)
                    item.AppendSDK( platform, mediation, builder, allowAllTargets );
            }
        }

        private void AppendDependency( DependencyManager mediation, SDK sdk, BuildTarget platform, StringBuilder builder )
        {
            var depTagName = platform == BuildTarget.Android ? "androidPackage" : "iosPod";
            var depAttrName = platform == BuildTarget.Android ? "spec" : "name";
            var sourcesTagName = platform == BuildTarget.Android ? "repositories" : "sources";

            builder.Append( "    <" ).Append( depTagName ).Append( ' ' )
                                    .Append( depAttrName ).Append( "=\"" ).Append( sdk.name );
            if (platform == BuildTarget.Android)
                builder.Append( sdk.version );
            builder.Append( "\" version=\"" ).Append( sdk.version ).Append( "\"" );
            if (sdk.addToAllTargets)
                builder.Append( " addToAllTargets=\"true\"" );

            if (source.Length > 0)
            {
                builder.Append( ">" ).AppendLine();
                builder.Append( "      <" ).Append( sourcesTagName ).Append( '>' ).AppendLine();

                AppendSources( platform, builder );

                //for (int i = 0; i < contains.Length; i++)
                //{
                //    var item = mediation.Find( contains[i] );
                //    if (item != null)
                //    {
                //        item.locked = true;
                //        item.AppendSources( platform, builder );
                //    }
                //}

                builder.Append( "      </" ).Append( sourcesTagName ).Append( '>' ).AppendLine();
                builder.Append( "    </" ).Append( depTagName ).Append( '>' ).AppendLine();
            }
            else
            {
                builder.Append( "/>" ).AppendLine();
            }
        }

        private void AppendSources( BuildTarget platform, StringBuilder builder )
        {
            var sourceTagName = platform == BuildTarget.Android ? "repository" : "source";
            for (int i = 0; i < source.Length; i++)
                builder.Append( "        <" ).Append( sourceTagName ).Append( '>' )
                    .Append( source[i] )
                    .Append( "</" ).Append( sourceTagName ).Append( '>' ).AppendLine();
        }
        #endregion
    }
}
