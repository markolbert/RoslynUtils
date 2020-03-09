using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssets : ProjectAssetsBase
    {
        private class ProjectAssetsAssemblyLoadContext : AssemblyLoadContext
        {
            protected override Assembly Load( AssemblyName assemblyName )
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
            IJ4JLogger<ProjectAssets> logger 
            )
            : base(logger)
        {
            _paConverter = paConverter ?? throw new NullReferenceException( nameof( paConverter ) );
            _tgtCreator = tgtCreator ?? throw new NullReferenceException( nameof( tgtCreator ) );
            _pkgLibCreator = pkgLibCreator ?? throw new NullReferenceException( nameof(pkgLibCreator) );
            _projLibCreator = projLibCreator ?? throw new NullReferenceException( nameof(projLibCreator) );
            _pfdgCreator = pfdgCreator ?? throw new NullReferenceException( nameof(pfdgCreator) );
            _projCreator = projCreator ?? throw new NullReferenceException( nameof(projCreator) );
        }

        public int Version { get; private set; }
        public string ProjectFile { get; private set; }
        public List<TargetInfo> Targets { get; private set; }
        public List<ILibraryInfo> Libraries { get; private set; }
        public List<ProjectFileDependencyGroup> ProjectFileDependencyGroups { get; private set; }
        public List<string> PackageFolders { get; private set; }
        public ProjectInfo Project { get; private set; }
        public string ProjectFilePath => Project?.Restore?.ProjectPath;
        public ProjectLibrary ProjectLibrary { get; private set; }

        public bool InitializeFromProjectFile( string projectFilePath, [CallerMemberName] string callerName = "" )
        {
            if( String.IsNullOrEmpty( projectFilePath ) )
            {
                Logger.Error($"Undefined or empty {nameof(projectFilePath)} (called from {GetCallerPath(callerName)})");

                return false;
            }

            if( !File.Exists( projectFilePath ) )
            {
                Logger.Error( $"File '{projectFilePath}' doesn't exist (called from {GetCallerPath( callerName )})" );
                
                return false;
            }

            var projectFolder = Path.GetDirectoryName( projectFilePath );
            if( String.IsNullOrEmpty( projectFolder ) )
            {
                Logger.Error( $"File '{projectFilePath}' doesn't have a directory (called from {GetCallerPath( callerName )})" );

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
                    Logger.Error( $"Couldn't find project.assets.json file within the project directory '{projectFolder}'" );
                    return false;

                case 1:
                    // no op; hoped for case
                    break;

                default:
                    Logger.Error( $"Found multiple project.assets.json files within the project directory '{projectFolder}'" );
                    return false;
            }

            return Initialize( projectFilePath, projectAssetsFiles[ 0 ] );
        }

        public bool Initialize( string projFilePath, string projectAssetsPath )
        {
            if( String.IsNullOrEmpty( projectAssetsPath ) )
            {
                Logger.Error( $"Empty or undefined {projectAssetsPath}" );

                return false;
            }

            if( !File.Exists( projectAssetsPath ) )
            {
                Logger.Error( $"File '{projectAssetsPath}' not accessible" );

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

            var okay = GetProperty<ExpandoObject>( container, "targets", context, out var tgtDict );
            okay &= GetProperty<ExpandoObject>( container, "libraries", context, out var libDict );
            okay &= GetProperty<ExpandoObject>( container, "projectFileDependencyGroups", context,
                out var projFileDepDict );
            okay &= GetProperty<ExpandoObject>( container, "packageFolders", context, out var pkgDict );
            okay &= GetProperty<ExpandoObject>( container, "project", context, out var projDict );
            okay &= GetProperty<int>( container, "version", context, out var version );
            if( !okay ) return false;

            // separate the libraries into package libraries and project libraries so we can process
            // them separately
            if( !FilterLibraries( libDict, ReferenceType.Package, context, out var pkgLibDict ) )
                return false;

            if( !FilterLibraries( libDict, ReferenceType.Project, context, out var projLibDict ) )
                return false;

            var project = _projCreator();

            okay = LoadFromContainer<TargetInfo, ExpandoObject>( tgtDict, _tgtCreator, context, out var tgtList );
            okay &= LoadFromContainer<PackageLibrary, ExpandoObject>( pkgLibDict, _pkgLibCreator, context,
                out var pkgLibList );
            okay &= LoadFromContainer<ProjectLibrary, ExpandoObject>( projLibDict, _projLibCreator, context,
                out var projLibList );
            okay &= LoadFromContainer<ProjectFileDependencyGroup, List<string>>( projFileDepDict, _pfdgCreator, context,
                out var pfdgList );
            okay &= LoadNamesFromContainer( pkgDict, out var pkgList );
            okay &= project.Initialize( projDict, context );

            if( !okay ) return false;

            var projLib = _projLibCreator();

            if( !projLib.InitializeFromProjectFile( projFilePath ) )
                return false;

            Version = version;
            Targets = tgtList;

            Libraries = new List<ILibraryInfo>();
            Libraries.AddRange( pkgLibList );
            Libraries.AddRange( projLibList );

            ProjectFileDependencyGroups = pfdgList;
            Project = project;
            PackageFolders = pkgList;
            ProjectLibrary = projLib;

            return true;
        }

        public bool GetLibraryPaths( TargetFramework tgtFramework, out List<MetadataReference> result )
        {
            result = null;

            if( tgtFramework == null )
            {
                Logger.Error( $"Undefined {nameof(tgtFramework)}" );

                return false;
            }

            result = new List<MetadataReference>();
            var okay = true;

            // add the basic/core libraries
            //var alc = new ProjectAssetsAssemblyLoadContext();

            switch( tgtFramework.Framework )
            {
                case CSharpFrameworks.Net:
                    Logger.Error( "net framework is not supported" );
                    return false;

                case CSharpFrameworks.NetCoreApp:
                    //@TODO what goes here?
                    Logger.Error( "netcoreapp framework is not supported" );
                    return false;

                case CSharpFrameworks.NetStandard:
                    okay &= TryAddMetadataReference( "netstandard", result );
                    okay &= TryAddMetadataReference( "System.Private.CoreLib", result );
                    okay &= TryAddMetadataReference( "System.Private.Uri", result );
                    okay &= TryAddMetadataReference( "System.Runtime.Extensions", result );

                    if( !okay )
                        Logger.Error(
                            $"Problems encountered loading standard {CSharpFrameworks.NetStandard} assemblies" );

                    break;
            }

            okay = true;

            foreach( var libInfo in Libraries.Where(lib=>lib is PackageLibrary  ) )
            {
                // first try loading library by name
                if( TryAddMetadataReference( libInfo.Assembly, result ) )
                    continue;

                // if that fails, try loading by absolute path
                var absPath = libInfo.GetAbsolutePath( PackageFolders, tgtFramework );

                if( String.IsNullOrEmpty( absPath ) )
                    continue;

                if( !TryAddMetadataReference( absPath, result, isPath: true ) )
                {
                    //okay = false;
                    Logger.Warning( $"Couldn't load assembly '{absPath}'" );
                }
            }

            return okay;
        }

        public bool GetSourceFiles( out List<string> srcFiles )
        {
            srcFiles = null;

            if( ProjectLibrary == null )
            {
                Logger.Error($"Undefined {nameof(ProjectLibrary)}");

                return false;
            }

            srcFiles = ProjectLibrary.SourceFiles;

            foreach( var libInfo in Libraries.Where( lib => lib.Type == ReferenceType.Project )
                .Cast<ProjectLibrary>() )
            {
                srcFiles.AddRange(libInfo.SourceFiles);
            }

            srcFiles = srcFiles.Distinct( StringComparer.OrdinalIgnoreCase ).ToList();

            return true;
        }

        private bool FilterLibraries( ExpandoObject libDict, ReferenceType refType, ProjectAssetsContext context, out ExpandoObject result )
        {
            result = null;

            if( libDict == null )
            {
                Logger.Error( $"Undefined {nameof(libDict)}" );

                return false;
            }

            result = new ExpandoObject();
            var allOKay = true;

            foreach( var kvp in libDict )
            {
                if( kvp.Value is ExpandoObject child )
                {
                    if( GetProperty<string>( child, "type", context, out var typeText )
                        && Enum.TryParse<ReferenceType>( typeText, true, out var childRefType )
                        && childRefType == refType )
                    {
                        if( !result.TryAdd( kvp.Key, kvp.Value ) )
                        {
                            Logger.Error($"Couldn't add {kvp.Key} to new {nameof(ExpandoObject)} in {nameof(FilterLibraries)}");

                            allOKay = false;
                        }
                    }
                }
            }

            if( !allOKay ) result = null;

            return allOKay;
        }

        private bool TryAddMetadataReference( 
            string libInfo, 
            List<MetadataReference> references, 
            bool isPath = false,
            [CallerMemberName] string callerName = "" )
        {
            if( String.IsNullOrEmpty(libInfo) )
            {
                Logger.Error($"Empty or undefined {nameof(libInfo)} (called from {GetCallerPath(callerName)})");

                return false;
            }

            try
            {
                Assembly assembly;

                if( isPath ) assembly = Assembly.LoadFile( libInfo );
                else assembly = Assembly.Load( libInfo );

                var mdRef = MetadataReference.CreateFromFile( assembly.Location );
                references.Add( mdRef );
            }
            catch( Exception e )
            {
                //Logger.Warning(
                //    $"Couldn't add assembly '{libInfo}' (called from {GetCallerPath( callerName )})" );

                return false;
            }

            return true;
        }
    }
}