using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssets : ConfigurationBase
    {
        private class ProjectAssetsAssemblyLoadContext : AssemblyLoadContext
        {
            protected override Assembly? Load( AssemblyName assemblyName )
            {
                return null;
            }
        }

        private readonly JsonProjectAssetsConverter _paConverter;
        private readonly Func<TargetInfo> _tgtCreator;
        private readonly Func<PackageLibrary> _pkgLibCreator;
        private readonly Func<ProjectLibrary> _projLibCreator;
        private readonly Func<ProjectFileDependencyGroup> _pfdgCreator;
        private readonly Func<ProjectInfo> _projCreator;

        public ProjectAssets(
            JsonProjectAssetsConverter paConverter,
            Func<TargetInfo> tgtCreator,
            Func<PackageLibrary> pkgLibCreator,
            Func<ProjectLibrary> projLibCreator,
            Func<ProjectFileDependencyGroup> pfdgCreator,
            Func<ProjectInfo> projCreator,
            IJ4JLogger logger
        )
            : base( logger )
        {
            _paConverter = paConverter;
            _tgtCreator = tgtCreator;
            _pkgLibCreator = pkgLibCreator;
            _projLibCreator = projLibCreator;
            _pfdgCreator = pfdgCreator;
            _projCreator = projCreator;
        }

        public int Version { get; private set; }
        public string ProjectFile { get; private set; } = string.Empty;
        public string Name => Path.GetFileNameWithoutExtension( ProjectFile ) ?? string.Empty;
        public List<TargetInfo> Targets { get; } = new List<TargetInfo>();
        public List<ILibraryInfo> Libraries { get; } = new List<ILibraryInfo>();

        public List<ProjectFileDependencyGroup> ProjectFileDependencyGroups { get; } =
            new List<ProjectFileDependencyGroup>();

        public List<string> PackageFolders { get; } = new List<string>();
        public ProjectInfo? Project { get; private set; }
        public string ProjectFilePath => Project?.Restore?.ProjectPath ?? string.Empty;
        public ProjectLibrary? ProjectLibrary { get; private set; }

        public bool InitializeFromProjectFile( string projectFilePath )
        {
            if( string.IsNullOrEmpty( projectFilePath ) )
            {
                Logger.Error<string>( "Undefined or empty {0}", nameof(projectFilePath) );

                return false;
            }

            if( !File.Exists( projectFilePath ) )
            {
                Logger.Error<string>( "File '{projectFilePath}' doesn't exist", projectFilePath );

                return false;
            }

            var projectFolder = Path.GetDirectoryName( projectFilePath );
            if( String.IsNullOrEmpty( projectFolder ) )
            {
                Logger.Error<string>( "File '{projectFilePath}' doesn't have a directory", projectFilePath );

                return false;
            }

            // we assume the project.assets.json file is located somewhere within the project directory
            // and that there's only one instance of it
            var projectAssetsFiles = Directory.GetFiles(
                projectFolder,
                "project.assets.json",
                SearchOption.AllDirectories );

            switch( projectAssetsFiles.Length )
            {
                case 0:
                    Logger.Error<string>(
                        "Couldn't find project.assets.json file within the project directory '{projectFolder}'",
                        projectFolder );

                    return false;

                case 1:
                    // no op; hoped for case
                    break;

                default:
                    Logger.Error<string>(
                        "Found multiple project.assets.json files within the project directory '{projectFolder}'",
                        projectFolder );

                    return false;
            }

            return Initialize( projectFilePath, projectAssetsFiles[ 0 ] );
        }

        public bool Initialize( string projFilePath, string projectAssetsPath )
        {
            if( String.IsNullOrEmpty( projectAssetsPath ) )
            {
                Logger.Error<string>( "Empty or undefined {0}", nameof(projectAssetsPath) );

                return false;
            }

            if( !File.Exists( projectAssetsPath ) )
            {
                Logger.Error<string>( "File '{projectAssetsPath}' not accessible", projectAssetsPath );

                return false;
            }

            var opt = new JsonSerializerOptions();
            opt.Converters.Add( _paConverter );

            ExpandoObject container;

            try
            {
                container = JsonSerializer.Deserialize<ExpandoObject>( File.ReadAllText( projectAssetsPath ), opt );
            }
            catch( Exception e )
            {
                Logger.Error( e.Message );

                return false;
            }

            var context = new ProjectAssetsContext
            {
                ProjectAssetsJsonPath = projectAssetsPath,
                ProjectPath = projFilePath,
                RootContainer = container
            };

            var okay = container.GetProperty<ExpandoObject>( "targets", out var tgtDict );
            okay &= container.GetProperty<ExpandoObject>( "libraries", out var libDict );
            okay &= container.GetProperty<ExpandoObject>( "projectFileDependencyGroups", out var projFileDepDict );
            okay &= container.GetProperty<ExpandoObject>( "packageFolders", out var pkgDict );
            okay &= container.GetProperty<ExpandoObject>( "project", out var projDict );
            okay &= container.GetProperty<int>( "version", out var version );
            if( !okay ) return false;

            // separate the libraries into package libraries and project libraries so we can process
            // them separately
            if( !FilterLibraries( libDict, ReferenceType.Package, context, out var pkgLibDict ) )
                return false;

            if( !FilterLibraries( libDict, ReferenceType.Project, context, out var projLibDict ) )
                return false;

            okay = tgtDict.LoadFromContainer<TargetInfo, ExpandoObject>( _tgtCreator, context, out var tgtList );
            okay &= pkgLibDict!.LoadFromContainer<PackageLibrary, ExpandoObject>( _pkgLibCreator, context,
                out var pkgLibList );
            okay &= projLibDict!.LoadFromContainer<ProjectLibrary, ExpandoObject>( _projLibCreator, context,
                out var projLibList );
            okay &= projFileDepDict.LoadFromContainer<ProjectFileDependencyGroup, List<string>>( _pfdgCreator, context,
                out var pfdgList );
            okay &= pkgDict.LoadNamesFromContainer( out var pkgList );

            var project = _projCreator();
            okay &= project.Initialize( projDict, context );

            if( !okay ) return false;

            var projLib = _projLibCreator();

            if( !projLib.InitializeFromProjectFile( projFilePath ) )
                return false;

            Version = version;

            Targets.Clear();
            Targets.AddRange( tgtList! );

            Libraries.Clear();
            Libraries.AddRange( pkgLibList! );
            Libraries.AddRange( projLibList! );

            ProjectFileDependencyGroups.Clear();
            ProjectFileDependencyGroups.AddRange( pfdgList! );

            Project = project;

            PackageFolders.Clear();
            PackageFolders.AddRange( pkgList! );

            ProjectLibrary = projLib;

            return true;
        }

        public bool GetLibraryPaths( TargetFramework tgtFramework, out CompilationReferences? result )
        {
            result = null;

            // add the basic/core libraries
            switch( tgtFramework.Framework )
            {
                case CSharpFramework.Net:
                    Logger.Error( "net framework is not supported" );
                    return false;

                case CSharpFramework.NetCoreApp:
                    //@TODO what goes here?
                    Logger.Error( "netcoreapp framework is not supported" );
                    return false;

                case CSharpFramework.NetStandard:
                    result = new CompilationReferences
                    {
                        new NamedReference( "netstandard" ) { IsVirtual = true }
                    };

                    return true;
            }

            Logger.Error<string, CSharpFramework>( "Unsupported {0} '{1}'",
                nameof(CSharpFramework),
                tgtFramework.Framework );

            return false;
        }

        public bool GetSourceFiles( out List<string>? srcFiles )
        {
            srcFiles = null;

            if( ProjectLibrary == null )
            {
                Logger.Error<string>( "Undefined {0}", nameof(ProjectLibrary) );

                return false;
            }

            srcFiles = ProjectLibrary.SourceFiles;

            foreach( var libInfo in Libraries.Where( lib => lib.Type == ReferenceType.Project )
                .Cast<ProjectLibrary>() )
            {
                srcFiles.AddRange( libInfo.SourceFiles );
            }

            srcFiles = srcFiles.Distinct( StringComparer.OrdinalIgnoreCase ).ToList();

            return true;
        }

        private bool FilterLibraries( ExpandoObject libDict, ReferenceType refType, ProjectAssetsContext context,
            out ExpandoObject? result )
        {
            result = null;

            if( libDict == null )
            {
                Logger.Error<string>( "Undefined {0}", nameof(libDict) );

                return false;
            }

            result = new ExpandoObject();
            var allOKay = true;

            foreach( var kvp in libDict )
            {
                if( kvp.Value is ExpandoObject child )
                {
                    if( child.GetProperty<string>( "type", out var typeText )
                        && Enum.TryParse<ReferenceType>( typeText, true, out var childRefType )
                        && childRefType == refType )
                    {
                        if( !result.TryAdd( kvp.Key, kvp.Value ) )
                        {
                            Logger.Error<string, string, string>( "Couldn't add {0} to new {1} in {2}",
                                kvp.Key,
                                nameof(ExpandoObject), nameof(FilterLibraries) );

                            allOKay = false;
                        }
                    }
                }
            }

            if( !allOKay ) result = null;

            return allOKay;
        }
    }
}