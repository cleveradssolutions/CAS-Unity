using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
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

        private static bool solutionsFoldout = true;
        private static bool advancedFoldout;
        private static bool otherFoldout;

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
                    || path.Contains( "CAS" ) || !path.Contains( "/Editor/" ))
                    continue;
                if (File.Exists( path ))
                {
                    try
                    {
                        var source = File.ReadAllText( path );
                        string begin = platform == BuildTarget.Android ? "<androidPackage spec=\"" : "<iosPod name=\"";
                        var beginIndex = source.IndexOf( begin );
                        while (beginIndex > 0)
                        {
                            beginIndex += begin.Length;
                            other.Add( new AdDependency(
                                source.Substring( beginIndex, source.IndexOf( '\"', beginIndex ) - beginIndex ), path ) );

                            beginIndex = source.IndexOf( begin, beginIndex );
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException( e );
                    }
                }
            }
        }

        private void OnHeaderGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space( 25 );
            GUILayout.Label( "Dependency", EditorStyles.largeLabel );
            GUILayout.Label( "Version", EditorStyles.largeLabel, columnWidth );
            GUILayout.Label( "Latest", EditorStyles.largeLabel, columnWidth );
            GUILayout.Label( "Action", EditorStyles.largeLabel, columnWidth );
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
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox( "Found one or more updates for native dependencies.", MessageType.Info );
                if (GUILayout.Button( "Update all", GUILayout.ExpandHeight( true ) ))
                {
                    for (int i = 0; i < simple.Length; i++)
                    {
                        if (simple[i].isNewer)
                            simple[i].ActivateDependencies( platform, this );
                    }

                    for (int i = 0; i < advanced.Length; i++)
                    {
                        if (advanced[i].isNewer)
                            advanced[i].ActivateDependencies( platform, this );
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        internal void OnGUI( BuildTarget platform )
        {
            if (!installedAny)
                EditorGUILayout.HelpBox( "Dependencies of native SDK were not found. " +
                    "Please use the following options to integrate solutions or any SDK separately.",
                    MessageType.Warning );

            CheckDependencyUpdates( platform );

            columnWidth = GUILayout.MaxWidth( EditorGUIUtility.currentViewWidth * 0.15f );


            if (simple.Length > 0)
            {
                HelpStyles.BeginBoxScope();
                solutionsFoldout = GUILayout.Toggle( solutionsFoldout, "Solutions", EditorStyles.foldout );
                if (solutionsFoldout)
                {
                    OnHeaderGUI();
                    for (int i = 0; i < simple.Length; i++)
                        simple[i].OnGUI( this, platform );
                }
                HelpStyles.EndBoxScope();
            }

            if (advanced.Length > 0)
            {
                HelpStyles.BeginBoxScope();

                if (advancedFoldout)
                {
                    advancedFoldout = GUILayout.Toggle( advancedFoldout, "Advanced Integration", EditorStyles.foldout );
                    OnHeaderGUI();
                    for (int i = 0; i < advanced.Length; i++)
                        advanced[i].OnGUI( this, platform );
                }
                else
                {
                    int installed = 0;

                    for (int i = 0; i < advanced.Length; i++)
                    {
                        if (advanced[i].installedVersion.Length > 0)
                        {
                            installed++;
                            if (advanced[i].inBan && Event.current.type == EventType.Repaint)
                            {
                                advancedFoldout = true;
                                Debug.LogError( Utils.logTag + advanced[i].name +
                                    " Dependencies found that are not valid for the applications of the selected audience." );
                            }
                        }
                    }
                    advancedFoldout = GUILayout.Toggle( advancedFoldout, "Advanced Integration (" + installed + ")", EditorStyles.foldout );
                }
                HelpStyles.EndBoxScope();
            }
            if (other.Count > 0)
            {
                HelpStyles.BeginBoxScope();
                otherFoldout = GUILayout.Toggle( otherFoldout, "Other Active Dependencies: " + other.Count, EditorStyles.foldout );
                if (otherFoldout)
                {
                    for (int i = 0; i < other.Count; i++)
                        other[i].OnGUI( this );
                }
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

                    if (require != noNetwork)
                    {
                        var item = mediation.Find( require );
                        if (item != null)
                            item.isRequired = true;
                    }
                }
            }
        }

        internal void FilterAudience( Audience audience )
        {
            if (filter < 0)
                inBan = true;
            else if (audience == Audience.Mixed)
                inBan = false;
            else if (audience == Audience.Children)
                inBan = filter < 1;
            else
                inBan = filter > 1;
        }

        private void OnLabelGUI( Label label )
        {
            if (label == Label.None)
                return;
            string title = "";
            string tooltip = "";
            if (( label & Label.Banner ) == Label.Banner)
            {
                title += "b ";
                tooltip += "'b' - Support Banner Ad\n";
            }
            if (( label & Label.Inter ) == Label.Inter)
            {
                title += "i ";
                tooltip += "'i' - Support Interstitial Ad\n";
            }
            if (( label & Label.Reward ) == Label.Reward)
            {
                title += "r ";
                tooltip += "'r' - Support Rewarded Ad\n";
            }
            if (( label & Label.Beta ) == Label.Beta)
            {
                title += "beta";
                tooltip += "'beta' - Dependencies in closed beta and available upon invite only. " +
                    "If you would like to be considered for the beta, please contact Support.";
            }
            if (( label & Label.Obsolete ) == Label.Obsolete)
            {
                title += "obsolete";
                tooltip += "'obsolete' - The mediation of the network is considered obsolete and not recommended for install.";
            }
            GUILayout.Label( HelpStyles.GetContent( title, null, tooltip ), HelpStyles.labelStyle );
        }

        internal void OnGUI( DependencyManager mediation, BuildTarget platform )
        {
            bool installed = !string.IsNullOrEmpty( installedVersion );
            bool isErrorRemove = false;
            if (inBan && filter < 0 && !installed)
                return;
            var dividerRect = EditorGUILayout.GetControlRect( GUILayout.Height( 1 ) );
            if (Event.current.type == EventType.Repaint) //draw the divider
            {
                GUI.skin.box.Draw( dividerRect, GUIContent.none, 0 );
            }
            EditorGUILayout.BeginHorizontal();
            if (( inBan && installed ) || ( installed && locked ) || ( isRequired && !locked && !installed ))
            {
                GUILayout.Label( HelpStyles.errorIconContent, GUILayout.Width( 20 ) );
                isErrorRemove = true;
            }
            else if (string.IsNullOrEmpty( url ))
                GUILayout.Space( 25 );
            else if (GUILayout.Button( HelpStyles.helpIconContent, EditorStyles.label, GUILayout.Width( 20 ) ))
                Application.OpenURL( url );

            //OnLabelGUI( labels );
            
            if (installed)
            {
                GUILayout.Label( name, GUILayout.ExpandWidth( false ) );
                GUILayout.FlexibleSpace();
                if (!isNewer || inBan)
                {
                    GUILayout.Label( version, mediation.columnWidth );
                    GUILayout.Label( "-", mediation.columnWidth );
                }
                else
                {
                    GUILayout.Label( installedVersion, mediation.columnWidth );
                    if (GUILayout.Button( version, EditorStyles.miniButton, mediation.columnWidth ))
                        ActivateDependencies( platform, mediation );
                }

                if(isRequired && !locked)
                {
                    GUILayout.Label( "Required", mediation.columnWidth );
                }
                else if (isErrorRemove)
                {
                    if (GUILayout.Button( "Remove", EditorStyles.miniButton, mediation.columnWidth ))
                        DisableDependencies( platform, mediation );
                }
                else
                {
                    var tempToggle = true;
                    if (tempToggle != GUILayout.Toggle( tempToggle, "", mediation.columnWidth ))
                        DisableDependencies( platform, mediation );
                }
            }
            else if (locked)
            {
                GUILayout.Label( name, GUILayout.ExpandWidth( false ) );
                GUILayout.FlexibleSpace();
                EditorGUI.BeginDisabledGroup( true );
                GUILayout.Label( "-", mediation.columnWidth );
                GUILayout.Label( version, mediation.columnWidth );
                GUILayout.Label( "Required", mediation.columnWidth );
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.BeginDisabledGroup( inBan || dependencies.Length == 0 );
                if (GUILayout.Button( name, EditorStyles.label, GUILayout.ExpandWidth( false ) ))
                    ActivateDependencies( platform, mediation );
                GUILayout.FlexibleSpace();
                GUILayout.Label( "none", mediation.columnWidth );
                GUILayout.Label( version, mediation.columnWidth );
                var tempToggle = false;
                if (tempToggle != GUILayout.Toggle( tempToggle, "", mediation.columnWidth ))
                    ActivateDependencies( platform, mediation );
                EditorGUI.EndDisabledGroup();
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
            if (dependencies.Length == 0)
            {
                Debug.LogError( Utils.logTag + name + " have no dependencies. Please try reimport CAS package." );
                return;
            }
            if (locked)
            {
                return;
            }

            string dependencyName;
            string sourceName;
            string sourcesName;
            string attrName;
            if (platform == BuildTarget.Android)
            {
                dependencyName = "androidPackage";
                sourcesName = "repositories";
                sourceName = "repository";
                attrName = "spec";
            }
            else
            {
                dependencyName = "iosPod";
                sourcesName = "sources";
                sourceName = "source";
                attrName = "name";
            }

            var destination = Utils.GetDependencyPath( name, platform );
            EditorUtility.DisplayProgressBar( "Create dependency", destination, 0.2f );

            try
            {
                var builder = new StringBuilder();
                builder.AppendLine( "<?xml version=\"1.0\" encoding=\"utf-8\"?>" )
                       .AppendLine( "<dependencies>" )
                       .Append( "  <" ).Append( dependencyName ).Append( "s>" ).AppendLine();

                for (int i = 0; i < dependencies.Length; i++)
                {
                    builder.Append( "    <" ).Append( dependencyName ).Append( ' ' )
                        .Append( attrName ).Append( "=\"" ).Append( dependencies[i] )
                        .Append( "\" version=\"" ).Append( version ).Append( "\"" );
                    if (platform == BuildTarget.iOS)
                        builder.Append( " addToAllTargets=\"true\"" );
                    builder.Append( ">" ).AppendLine();
                    if (i == 0 && ( source.Length > 0 || contains.Length > 0 ))
                    {
                        builder.Append( "      <" ).Append( sourcesName ).Append( '>' ).AppendLine();

                        int containsIndex = 0;
                        Dependency item = this;
                        do
                        {
                            for (int sourceIndex = 0; sourceIndex < item.source.Length; sourceIndex++)
                                builder.Append( "        <" ).Append( sourceName ).Append( '>' )
                                    .Append( item.source[sourceIndex] )
                                    .Append( "</" ).Append( sourceName ).Append( '>' ).AppendLine();

                            item = null;
                            while (containsIndex < contains.Length && item == null)
                                item = mediation.Find( contains[containsIndex++] );

                            if (item != null)
                            {
                                item.locked = true;
                            }
                        } while (item != null);

                        builder.Append( "      </" ).Append( sourcesName ).Append( '>' ).AppendLine();
                    }
                    builder.Append( "    </" ).Append( dependencyName ).Append( '>' ).AppendLine();
                }
                builder.Append( "  </" ).Append( dependencyName ).Append( "s>" ).AppendLine()
                       .AppendLine( "</dependencies>" );


                Utils.CreateFolderInAssets( "Editor" );
                var replace = File.Exists( destination );
                File.WriteAllText( destination, builder.ToString() );
                if (!replace)
                    AssetDatabase.ImportAsset( destination );

                Init( mediation, platform, false );
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            if (require != noNetwork && !File.Exists( Utils.GetDependencyPath( require.GetName(), platform ) ))
            {
                var item = mediation.Find( require );
                if (item != null)
                {
                    item.isRequired = true;
                    item.ActivateDependencies( platform, mediation );
                }
            }
        }
        #endregion
    }
}
