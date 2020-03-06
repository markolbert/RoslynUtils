using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssets : ProjectAssetsBase
    {
        private readonly JsonProjectAssetsConverter _paConverter;
        private readonly Func<TargetInfo> _tgtCreator;
        private readonly Func<LibraryInfo> _libCreator;
        private readonly Func<ProjectFileDependencyGroup> _pfdgCreator;
        private readonly Func<ProjectInfo> _projCreator;

        public ProjectAssets(
            JsonProjectAssetsConverter paConverter,
            Func<TargetInfo> tgtCreator,
            Func<LibraryInfo> libCreator,
            Func<ProjectFileDependencyGroup> pfdgCreator,
            Func<ProjectInfo> projCreator,
            IJ4JLogger<ProjectAssets> logger 
            )
            : base(logger)
        {
            _paConverter = paConverter ?? throw new NullReferenceException( nameof( paConverter ) );
            _tgtCreator = tgtCreator ?? throw new NullReferenceException( nameof( tgtCreator ) );
            _libCreator = libCreator ?? throw new NullReferenceException( nameof( libCreator ) );
            _pfdgCreator = pfdgCreator ?? throw new NullReferenceException( nameof(pfdgCreator) );
            _projCreator = projCreator ?? throw new NullReferenceException( nameof(projCreator) );
        }

        public int Version { get; set; }
        public List<TargetInfo> Targets { get; set; }
        public List<LibraryInfo> Libraries { get; set; }
        public List<ProjectFileDependencyGroup> ProjectFileDependencyGroups { get; set; }
        public List<string> PackageFolders { get; set; }
        public ProjectInfo Project { get; set; }

        public bool Load( string projectAssetsPath )
        {
            if( String.IsNullOrEmpty( projectAssetsPath ) )
            {
                Logger.Error($"Empty or undefined {projectAssetsPath}");

                return false;
            }

            if( !File.Exists( projectAssetsPath ) )
            {
                Logger.Error($"File '{projectAssetsPath}' not accessible");

                return false;
            }

            var opt = new JsonSerializerOptions();
            opt.Converters.Add( _paConverter );

            ExpandoObject expando;

            try
            {
                expando = JsonSerializer.Deserialize<ExpandoObject>( File.ReadAllText( projectAssetsPath ), opt );
            }
            catch( Exception e )
            {
                Logger.Error(e.Message);

                return false;
            }

            return Load( expando );
        }

        public bool Load( ExpandoObject container )
        {
            if( container == null )
            {
                Logger.Error( $"Undefined {nameof(container)}" );

                return false;
            }

            if( !GetProperty<ExpandoObject>( container, "targets", out var tgtDict )
                || !GetProperty<ExpandoObject>( container, "libraries", out var libDict )
                || !GetProperty<ExpandoObject>( container, "projectFileDependencyGroups", out var projFileDepDict )
                || !GetProperty<ExpandoObject>( container, "packageFolders", out var pkgDict )
                || !GetProperty<ExpandoObject>( container, "project", out var projDict )
                || !GetProperty<int>( container, "version", out var version )
            )
                return false;


            var project = _projCreator();

            if( !LoadFromContainer<TargetInfo, ExpandoObject>( tgtDict, _tgtCreator, out var tgtList )
                || !LoadFromContainer<LibraryInfo, ExpandoObject>( libDict, _libCreator, out var libList )
                || !LoadFromContainer<ProjectFileDependencyGroup, List<string>>( projFileDepDict, _pfdgCreator,
                    out var pfdgList )
                || !LoadNamesFromContainer( pkgDict, out var pkgList )
                || !project.Load( projDict ) )
                return false;

            Version = version;
            Targets = tgtList;
            Libraries = libList;
            ProjectFileDependencyGroups = pfdgList;
            Project = project;

            PackageFolders = pkgList;

            return true;
        }
    }
}