using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class DocumentationWorkspace
    {
        private readonly Func<IJ4JLogger> _loggerFactory;
        private readonly AnalyzerManager _manager = new AnalyzerManager();
        private readonly Dictionary<string, IAnalyzerResults> _buildResults = new Dictionary<string, IAnalyzerResults>();
        private readonly IJ4JLogger _logger;

        public DocumentationWorkspace(
            Func<IJ4JLogger> loggerFactory
        )
        {
            _loggerFactory = loggerFactory;

            _logger = loggerFactory();
            _logger.SetLoggedType(this.GetType());
        }

        public bool AddProject( string csProjFile )
        {
            var projAnalyzer = _manager.GetProject( csProjFile );

            var results = projAnalyzer.Build();

            if( results.OverallSuccess )
            {
                if( _buildResults.ContainsKey( csProjFile ) )
                    _buildResults[ csProjFile ] = results;
                else _buildResults.Add( csProjFile, results );

                return true;
            }
            
            _logger.Error<string>( "Project analysis failed for '{0}'", csProjFile );
            
            return false;
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

        public async Task<List<CompiledProject>?> Compile()
        {
            var ws = _manager.GetWorkspace();

            if( ws.CurrentSolution == null )
            {
                _logger.Error<string>( "{0} is undefined in the workspace", nameof(ws.CurrentSolution) );
                return null;
            }

            var retVal = new List<CompiledProject>();

            foreach( var projectID in ws.CurrentSolution.GetProjectDependencyGraph().GetTopologicallySortedProjects() )
            {
                var project = ws.CurrentSolution.GetProject( projectID );

                if( project == null )
                {
                    _logger.Error<ProjectId>( "Could not retrieve project with ID '{0}' from solution", projectID );
                    continue;
                }

                var compilation = await project.GetCompilationAsync() as CSharpCompilation;

                if( compilation == null )
                {
                    _logger.Error<string>("Could not compile project '{0}' from solution", project.Name);
                    continue;
                }

                var buildResults = project.FilePath != null && _buildResults.ContainsKey( project.FilePath )
                    ? _buildResults[ project.FilePath ]
                    : null;

                retVal.Add( new CompiledProject( buildResults, project, compilation ) );
            }

            return retVal;
        }
    }
}