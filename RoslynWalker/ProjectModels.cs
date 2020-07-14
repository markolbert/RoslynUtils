using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectModels : IEnumerable<ProjectModel>
    {
        private readonly Func<IJ4JLogger> _loggerFactory;
        private readonly IJ4JLogger _logger;
        private readonly JsonProjectAssetsConverter _projAssetsConv;

        public ProjectModels(
            JsonProjectAssetsConverter projAssetsConv,
            Func<IJ4JLogger> loggerFactory
        )
        {
            _projAssetsConv = projAssetsConv;

            _loggerFactory = loggerFactory;

            _logger = loggerFactory();
            _logger.SetLoggedType(this.GetType());
        }

        public bool IsValid => TargetFramework != null
                               && Models.Count > 0 
                               && Models.All( m => m.IsValid );

        public ProjectModelCompilationOptions ProjectModelCompilationOptions { get; private set; } =
            new ProjectModelCompilationOptions();

        public TargetFramework? TargetFramework { get; private set; }
        public List<ProjectModel> Models { get; } = new List<ProjectModel>();

        public bool AddProject( string csProjFile )
        {
            var projAsset = new ProjectAssets( _projAssetsConv, _loggerFactory );

            if( !projAsset.InitializeFromProjectFile( csProjFile ) )
            {
                _logger.Error<string, string>( "Couldn't initialize {0} from '{1}'",
                    nameof(ProjectAssets),
                    csProjFile );

                return false;
            }

            var newModel = new ProjectModel( _projAssetsConv, _loggerFactory() );

            if( !newModel.Analyze( csProjFile ) )
                return false;

            Models.Add( newModel );

            return true;
        }

        public bool AddProjects(IEnumerable<string> csProjFiles)
        {
            var retVal = true;

            foreach (var csProjFile in csProjFiles)
            {
                retVal &= AddProject(csProjFile);
            }

            return retVal;
        }

        public bool AddProjects( params string[] csProjFiles ) => AddProjects( csProjFiles.ToList() );

        public bool AddSolutionProjects( string solFile )
        {
            if( String.IsNullOrEmpty( solFile ) )
            {
                _logger.Error<string>( "Empty or undefined {0}", nameof(solFile) );
                return false;
            }

            if( !File.Exists( solFile ) )
            {
                _logger.Error<string>( "File '{0}' doesn't exist", solFile );
                return false;
            }

            if( !Path.GetExtension( solFile ).Equals( ".sln", StringComparison.OrdinalIgnoreCase ) )
            {
                _logger.Error<string>(
                    "'{0}' isn't a .sln file",
                    solFile );

                return false;
            }

            var lines = File.ReadAllLines( solFile ).ToList();

            var nestedLine = lines.Select( ( x, i ) => new { x, i } )
                .Where( y => y.x.StartsWith( "GlobalSection(NestedProjects)" ) )
                .Select( y => y.i )
                .ToList();

            var nestedGuids = new List<Guid>();
            var folderGuids = new List<Guid>();

            switch( nestedLine.Count )
            {
                case 0:
                    // no op; no nested projects
                    break;

                case 1:
                    var curLine = nestedLine[ 0 ] + 1;

                    while( !lines[ curLine ].Equals( "EndGlobalSection" ) )
                    {
                        var match = Regex.Match( lines[ curLine ], "(.*?)=(.*?)" );
                        nestedGuids.Add( new Guid( match.Groups[ 0 ].Value ) );

                        var folderID = new Guid( match.Groups[ 2 ].Value );
                        if( !folderGuids.Contains( folderID ) )
                            folderGuids.Add( folderID );

                        curLine++;
                    }

                    break;

                default:
                    // error; too many nested project sections
                    _logger.Error( "More than 1 nested project section" );
                    return false;
            }

            var projects = lines.Where( x => x.StartsWith( "Project(", StringComparison.OrdinalIgnoreCase ) );

            var solutionDir = Path.GetDirectoryName( solFile ) ?? string.Empty;

            var allOkay = true;

            foreach( var project in projects )
            {
                var parts = project.Split( '=' );

                if( parts.Length != 2 )
                {
                    _logger.Error<string>( "Couldn't parse project string from solution file ('{0}')", project );
                    continue;
                }

                var args = parts[ 1 ].Split( ',' );

                if( args.Length != 3 )
                {
                    _logger.Error<string>( "Couldn't parse project string from solution file ('{0}')", project );
                    continue;
                }

                _logger.Verbose<string>( "Found project {0}", args[ 0 ] );

                var cleaned = args[ 2 ].Replace( "\"{", "" ).Replace( "}\"", "" );
                var projectID = new Guid( cleaned );

                if( !nestedGuids.Contains( projectID ) && !folderGuids.Contains( projectID ) )
                    allOkay &= AddProject( Path.Combine( solutionDir, args[ 1 ].Replace( "\"", "" ).Trim() ) );
            }

            return allOkay;
        }

        public bool Compile( TargetFramework tgtFW, bool recompile = false )
        {
            var allOkay = true;

            foreach( var model in Models )
            {
                if( !model.IsCompiled || recompile )
                    allOkay &= model.Compile( tgtFW );
            }

            return allOkay;
        }

        public IEnumerator<ProjectModel> GetEnumerator()
        {
            foreach( var retVal in Models )
            {
                yield return retVal;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}