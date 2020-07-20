﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssets : ProjectAssetsBase
    {
        private readonly JsonProjectAssetsConverter _paConverter;

#pragma warning disable 8618
        public ProjectAssets(
#pragma warning restore 8618
            JsonProjectAssetsConverter paConverter,
            Func<IJ4JLogger> loggerFactory
        )
            : base( loggerFactory )
        {
            _paConverter = paConverter;
        }

        public bool IsValid { get; internal set; }

        public int Version { get; private set; }
        public string ProjectFile { get; private set; } = string.Empty;
        public string ProjectDirectory => Path.GetDirectoryName( ProjectFile ) ?? string.Empty;
        public string Name => Path.GetFileNameWithoutExtension( ProjectFile ) ?? string.Empty;
        public List<TargetInfo> Targets { get; } = new List<TargetInfo>();
        public List<ILibraryInfo> Libraries { get; } = new List<ILibraryInfo>();

        public List<ProjectFileDependencyGroup> ProjectFileDependencyGroups { get; } =
            new List<ProjectFileDependencyGroup>();

        public NugetRepositories Repositories { get; private set; }
        public ProjectInfo Project { get; private set; }
        public ProjectLibrary ProjectLibrary { get; private set; }

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
            if( string.IsNullOrEmpty( projectFolder ) )
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

            ProjectFile = projFilePath;

            if( !File.Exists( projectAssetsPath ) )
            {
                Logger.Error<string>( "File '{projectAssetsPath}' not accessible", projectAssetsPath );

                return false;
            }

            var opt = new JsonSerializerOptions();
            opt.Converters.Add( _paConverter );

            IsValid = true;

            try
            {
                var configuration =
                    JsonSerializer.Deserialize<ExpandoObject>( File.ReadAllText( projectAssetsPath ), opt );

                Version = GetProperty<int>( configuration, "version" );
                
                Project = new ProjectInfo( 
                    "project", 
                    GetProperty<ExpandoObject>( configuration, "project" ),
                    LoggerFactory );

                ProjectLibrary = new ProjectLibrary( projFilePath, LoggerFactory );

                CreateTargets( configuration );
                CreateLibraries( configuration );
                CreateProjectFileDependencyGroups( configuration );

                Repositories = new NugetRepositories( 
                    GetProperty<ExpandoObject>( configuration, "packageFolders" ),
                    LoggerFactory );
            }
            catch( Exception e )
            {
                Logger.Error( e.Message );

                return false;
            }

            return IsValid;
        }

        private void CreateTargets( ExpandoObject configuration )
        {
            Targets.Clear();

            foreach( var kvp in GetProperty<ExpandoObject>( configuration, "targets" ) )
            {
                if( kvp.Value is ExpandoObject container )
                    Targets.Add( new TargetInfo( kvp.Key, container, LoggerFactory ) );
                else
                    throw ProjectAssetsException.CreateAndLog(
                        $"Couldn't create a {typeof(TargetInfo)} from property '{kvp.Key}'",
                        this.GetType(),
                        Logger);
            }
        }

        private void CreateLibraries( ExpandoObject configuration )
        {
            Libraries.Clear();

            foreach (var kvp in GetProperty<ExpandoObject>(configuration,"libraries"))
            {
                if( kvp.Value is ExpandoObject container )
                {
                    var refType = GetEnum<ReferenceType>( container,"type" );

                    switch ( refType )
                    {
                        case ReferenceType.Package:
                            Libraries.Add( new PackageLibrary( kvp.Key, container, LoggerFactory ) );
                            break;

                        case ReferenceType.Project:
                            Libraries.Add( new ProjectLibrary( kvp.Key, container, ProjectDirectory, LoggerFactory ) );
                            break;

                        default:
                            throw ProjectAssetsException.CreateAndLog(
                                $"Unsupported value '{refType}' for {typeof(ReferenceType)}",
                                this.GetType(),
                                Logger);
                    }
                }
                else
                    throw ProjectAssetsException.CreateAndLog(
                        "Couldn't create a PackageLibrary or CSProjLibrary",
                        this.GetType(),
                        Logger);
            }
        }

        private void CreateProjectFileDependencyGroups( ExpandoObject configuration )
        {
            ProjectFileDependencyGroups.Clear();

            foreach (var kvp in GetProperty<ExpandoObject>(configuration,"projectFileDependencyGroups"))
            {
                if (kvp.Value is List<string> container)
                    ProjectFileDependencyGroups.Add(new ProjectFileDependencyGroup(kvp.Key, container, LoggerFactory));
                else
                    throw ProjectAssetsException.CreateAndLog(
                        $"Couldn't create a {typeof(ProjectFileDependencyGroup)} from property '{kvp.Key}'",
                        this.GetType(),
                        Logger);
            }
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

        public bool GetSourceFiles( out List<string>? result )
        {
            result = null;

            if( !IsValid )
            {
                Logger.Error<string>( "{0} is not validly configured", nameof(ProjectAssets) );
                return false;
            }

            result = ProjectLibrary.SourceFiles;

            foreach( var libInfo in Libraries.Where( lib => lib.Type == ReferenceType.Project )
                .Cast<ProjectLibrary>() )
            {
                result.AddRange( libInfo.SourceFiles );
            }

            result = result.Distinct( StringComparer.OrdinalIgnoreCase ).ToList();

            return true;
        }
    }
}