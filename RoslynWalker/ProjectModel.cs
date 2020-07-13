using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Buildalyzer;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace J4JSoftware.Roslyn
{
    public class ProjectModelCompilationOptions
    {
        private int _warningLevel = 4;

        public ProjectModelCompilationOptions()
        {
            Suppress = new List<string>( new string[] { "CS1701", "CS1702" } );
        }

        public int WarningLevel
        {
            get => _warningLevel;
            set => _warningLevel = value < 0 ? 4 : value;
        }

        public List<string> Suppress { get; }
        public string? CompilationName { get; set; }
        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;
        public ReportDiagnostic DiagnosticLevel { get; set; } = ReportDiagnostic.Error;

        public Dictionary<string, ReportDiagnostic> GetSuppressedDiagnostics() =>
            Suppress.ToDictionary( s => s, s => ReportDiagnostic.Suppress );
    }

    public class ProjectModel
    {
        private readonly IJ4JLogger _logger;
        private readonly Func<IJ4JLogger> _loggerFactory;
        private readonly ProjectAssets _projAssets;

        private bool _nonCompOkay = true;

        public ProjectModel( 
            ProjectAssets projAssets,
            Func<IJ4JLogger> loggerFactory 
            )
        {
            _projAssets = projAssets;

            _loggerFactory = loggerFactory;

            _logger = loggerFactory();
            _logger.SetLoggedType(this.GetType());

            ExternalAssemblies = new RequiredAssemblies( loggerFactory );
        }

        public bool IsValid => _nonCompOkay && !HasCompilationErrors;

        public List<CompilationResult> CompilationResults { get; } = new List<CompilationResult>();
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public bool HasCompilationProblems( DiagnosticSeverity severity ) =>
            Diagnostics.Any( d => d.Severity == severity );

        public bool HasCompilationErrors => HasCompilationProblems( DiagnosticSeverity.Error );

        public RequiredAssemblies ExternalAssemblies { get; }

        public bool Compile( ProjectAssets projAssets, TargetFramework tgtFramework )
        {
            if( !projAssets.IsValid )
            {
                _logger.Error<string>( "{0} is not validly configured", nameof(projAssets) );
                return false;
            }

            if( !projAssets.GetSourceFiles( out var srcFiles ) )
            {
                _logger.Error( "Couldn't get source files for project" );
                return false;
            }

            CompilationResults.Clear();
            Diagnostics.Clear();

            // build the identification/location information for the list of assemblies we'll be using
            var reqdAssemblies = ExternalAssemblies.Clone();

            // add certain universal assemblies
            reqdAssemblies.Add(name: "System.Runtime");

            // add the ones defined for our target framework
            switch ( tgtFramework.Framework )
            {
                case CSharpFramework.NetStandard:
                    //reqdAssemblies.Add( name : "netstandard" );
                    reqdAssemblies.Add(name: "System.Private.CoreLib");
                    break;

                case CSharpFramework.NetCoreApp:
                    reqdAssemblies.Add(name: "System.Private.CoreLib");
                    break;
            }

            // finally, add the assemblies defined in the project file
            foreach ( var libInfo in projAssets.Libraries.Where( lib => lib is PackageLibrary )
                .Cast<PackageLibrary>() )
            {
                if( !projAssets.Repositories.ResolvePackagePath( libInfo.Path, tgtFramework, out var pkgAbsPath ) )
                {
                    _logger.Error<string>( "Couldn't resolve path to nuget package '{0}'", libInfo.Path );
                    return false;
                }

                reqdAssemblies.Add( libInfo.Assembly, pkgAbsPath!.DllPath );
            }

            // create the syntax trees by parsing the source code files
            var trees = new List<SyntaxTree>();

            foreach( var srcFile in srcFiles! )
            {
                try
                {
                    var tree = CSharpSyntaxTree.ParseText( File.ReadAllText(srcFile) );
                    trees.Add( tree );
                }
                catch( Exception e )
                {
                    _logger.Error<string, string>(
                        "Failed to parse file '{0}' (Exception was {1})",
                        srcFile,
                        e.Message );

                    return false;
                }
            }

            // compile the project
            var options = new CSharpCompilationOptions( outputKind : projAssets.ProjectLibrary.OutputKind );

            options.WithNullableContextOptions( projAssets.ProjectLibrary.NullableContextOptions );

            CSharpCompilation compilation;

            _nonCompOkay = false;

            try
            {
                compilation = CSharpCompilation.Create( projAssets.Name, options : options )
                    .AddReferences( reqdAssemblies.GetMetadataReferences() )
                    .AddSyntaxTrees( trees );
            }
            catch( Exception e )
            {
                _logger.Error<string, string>(
                    "Failed to compile project '{0}' (Exception was {1})",
                    projAssets.Name,
                    e.Message );

                return false;
            }

            // create the syntax/semantic info we'll be searching
            foreach( var tree in trees )
            {
                if( !tree.TryGetRoot( out var root ) )
                {
                    CompilationResults.Clear();

                    _logger.Error<string, string>(
                        "Failed to get {0} for project {1}",
                        nameof(CompilationUnitSyntax),
                        projAssets.Name );

                    return false;
                }

                try
                {
                    CompilationResults.Add( new CompilationResult(
                        root,
                        compilation.GetSemanticModel( tree ) )
                    );
                }
                catch( Exception e )
                {
                    CompilationResults.Clear();

                    _logger.Error<string, string, string>(
                        "Failed to get {0} for project {1} (Exception was {2})",
                        nameof(SemanticModel),
                        projAssets.Name,
                        e.Message );

                    return false;
                }
            }

            _nonCompOkay = true;

            Diagnostics.AddRange( compilation.GetDiagnostics() );

            return IsValid;
        }

        public bool Compile( string csProjFile, ProjectModelCompilationOptions? pmOptions = null )
        {
            pmOptions ??= new ProjectModelCompilationOptions();

            CompilationResults.Clear();
            Diagnostics.Clear();

            if( !_projAssets.InitializeFromProjectFile( csProjFile ) )
            {
                _logger.Error<string, string>( "Failed to initialize {0} from '{1}'", nameof(ProjectAssets),
                    csProjFile );
                return false;
            }

            pmOptions.CompilationName ??= Path.GetFileNameWithoutExtension( csProjFile );

            var analyzerMgr = new AnalyzerManager();
            var projAnalyzer = analyzerMgr.GetProject( csProjFile );
            var buildResults = projAnalyzer.Build();

            if( !buildResults.OverallSuccess )
            {
                _logger.Error( "Project analysis failed" );
                return false;
            }

            var projResults = buildResults.Results.First();

            // create the syntax trees by parsing the source code files
            var trees = new List<SyntaxTree>();

            foreach( var srcFile in projResults.SourceFiles )
            {
                try
                {
                    var tree = CSharpSyntaxTree.ParseText( File.ReadAllText( srcFile ) );
                    trees.Add( tree );
                }
                catch( Exception e )
                {
                    _logger.Error<string, string>(
                        "Failed to parse file '{0}' (Exception was {1})",
                        srcFile,
                        e.Message );

                    return false;
                }
            }

            // compile the project
            var options = new CSharpCompilationOptions( outputKind : _projAssets.ProjectLibrary.OutputKind )
                .WithNullableContextOptions( _projAssets.ProjectLibrary.NullableContextOptions )
                .WithWarningLevel( pmOptions.WarningLevel )
                .WithOptimizationLevel( pmOptions.OptimizationLevel )
                .WithSpecificDiagnosticOptions( pmOptions.GetSuppressedDiagnostics() )
                .WithGeneralDiagnosticOption( pmOptions.DiagnosticLevel );

            CSharpCompilation compilation;

            _nonCompOkay = false;

            try
            {
                compilation = CSharpCompilation.Create( pmOptions.CompilationName, options : options )
                    .AddReferences( projResults.References.Select( r => MetadataReference.CreateFromFile( r ) ) )
                    .AddSyntaxTrees( trees );
            }
            catch( Exception e )
            {
                _logger.Error<string, string>(
                    "Failed to compile project '{0}' (Exception was {1})",
                    pmOptions.CompilationName,
                    e.Message );

                return false;
            }

            // create the syntax/semantic info we'll be searching
            foreach( var tree in trees )
            {
                if( !tree.TryGetRoot( out var root ) )
                {
                    CompilationResults.Clear();

                    _logger.Error<string, string>(
                        "Failed to get {0} for project {1}",
                        nameof(CompilationUnitSyntax),
                        pmOptions.CompilationName );

                    return false;
                }

                try
                {
                    CompilationResults.Add( new CompilationResult(
                        root,
                        compilation.GetSemanticModel( tree ) )
                    );
                }
                catch( Exception e )
                {
                    CompilationResults.Clear();

                    _logger.Error<string, string, string>(
                        "Failed to get {0} for project {1} (Exception was {2})",
                        nameof(SemanticModel),
                        pmOptions.CompilationName,
                        e.Message );

                    return false;
                }
            }

            _nonCompOkay = true;

            Diagnostics.AddRange( compilation.GetDiagnostics() );

            return IsValid;
        }
    }
}
