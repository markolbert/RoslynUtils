using System;
using System.Collections;
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
using NuGet.Versioning;

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

            Models.Add( new ProjectModel( projAsset, _projAssetsConv, _loggerFactory() ) );

            return true;
        }

        public bool AddSolution(string csSolFile)
        {
            return true;
        }

        public bool Compile( TargetFramework? tgtFW = null, ProjectModelCompilationOptions? pmOptions = null )
        {
            if (pmOptions != null)
                ProjectModelCompilationOptions = pmOptions;

            TargetFramework = tgtFW;

            foreach( var model in Models )
            {
                model.IsCompiled = false;
            }

            foreach ( var model in Models )
            {
                // if tgtFW is undefined, set it based on the first model being processed
                if( TargetFramework == null )
                {
                    if (model.ProjectAssets.ProjectLibrary.TargetFrameworks.Count > 1)
                    {
                        _logger.Error<string>("'{0}' supports multiple target frameworks but no desired one was specified",
                            model.ProjectAssets.ProjectFile);
                        return false;
                    }

                    TargetFramework ??= model.ProjectAssets.ProjectLibrary.TargetFrameworks.FirstOrDefault();

                    if( TargetFramework == null )
                    {
                        _logger.Error<string>( "'{0}' does not define a target framework and none was specified",
                            model.ProjectAssets.ProjectFile );
                        return false;
                    }
                }

                if (!model.ProjectAssets.Targets.Any(t => t.Target == TargetFramework.Framework && t.Version >= TargetFramework.Version))
                {
                    _logger.Error<string, TargetFramework>("Project '{0}' does not target a framework >= {1}",
                        model.ProjectAssets.ProjectFile,
                        TargetFramework);
                    return false;
                }

                if( model.Compile( TargetFramework, pmOptions ) ) 
                    continue;
                
                _logger.Error<string>( "'{0}' failed to compile", model.ProjectAssets.ProjectFile );
                
                return false;
            }

            return true;
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

    public class ProjectModel
    {
        private readonly IJ4JLogger _logger;
        private readonly JsonProjectAssetsConverter _projAssetsConv;

        public ProjectModel(
            JsonProjectAssetsConverter projAssetsConv,
            IJ4JLogger logger
        )
        {
            _projAssetsConv = projAssetsConv;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public IAnalyzerResults? AnalyzerResults { get; private set; }
        public string ProjectFile { get; private set; }
        public string ProjectName { get; private set; }
        public OutputKind OutputKind { get; private set; }
        public NullableContextOptions NullableContextOptions { get; private set; }
        public int WarningLevel { get; private set; }
        public OptimizationLevel OptimizationLevel { get; private set; }
        public Dictionary<string, ReportDiagnostic> IgnoredWarnings { get; private set; }
        public Platform Platform { get; private set; }

        public bool IsAnalyzed { get; private set; }
        public bool IsCompiled { get; private set; }
        public bool IsValid => IsAnalyzed && IsCompiled && !HasCompilationErrors;
        public List<TargetFramework> TargetFrameworks { get; } = new List<TargetFramework>();
        public List<CompilationResult> CompilationResults { get; } = new List<CompilationResult>();
        
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public bool HasCompilationProblems( DiagnosticSeverity severity ) =>
            Diagnostics.Any( d => d.Severity == severity );

        public bool HasCompilationErrors => HasCompilationProblems( DiagnosticSeverity.Error );

        public bool Analyze( string csProjFile, TargetFramework? tgtFW = null )
        {
            IsAnalyzed = false;
            TargetFrameworks.Clear();

            if ( !File.Exists( csProjFile ) )
            {
                _logger.Error<string>("Project file '{0}' doesn't exist", csProjFile  );
                return false;
            }

            var analyzerMgr = new AnalyzerManager();
            var projAnalyzer = analyzerMgr.GetProject(ProjectFile);
            AnalyzerResults = projAnalyzer.Build();

            if (!AnalyzerResults.OverallSuccess)
            {
                _logger.Error<string>("Project analysis failed for '{0}'", ProjectFile);
                return false;
            }

            try
            {
                TargetFrameworks.AddRange( AnalyzerResults.TargetFrameworks
                    .Select( t =>
                    {
                        if( TargetFramework.Create( t, TargetFrameworkTextStyle.Simple, out var result ) )
                            return result;

                        throw new ArgumentException( $"Couldn't convert '{t}' to a {nameof(TargetFramework)}" );
                    } )
                );
            }
            catch( Exception e )
            {
                _logger.Error<string, string, Exception>( "Couldn't determine {0} for {1}. Exception was {2}",
                    nameof(TargetFrameworks), 
                    csProjFile, 
                    e );

                return false;
            }

            IsAnalyzed = true;

            return true;
        }

        public bool Compile( TargetFramework tgtFW )
        {
            CompilationResults.Clear();
            Diagnostics.Clear();
            IsCompiled = false;

            if ( !IsAnalyzed )
            {
                _logger.Error("Project has not been analyzed"  );
                return false;
            }

            if (!TargetFrameworks.Any(t => t.Framework == tgtFW.Framework && t.Version >= tgtFW.Version))
            {
                _logger.Error<string, CSharpFramework, SemanticVersion>(
                    "Project '{0}' does not target a {1} framework with a version >= {2}",
                    ProjectName,
                    tgtFW.Framework,
                    tgtFW.Version );
                return false;
            }

            IAnalyzerResult? projResults = AnalyzerResults!.Results
                .FirstOrDefault( r =>
                {
                    if( TargetFramework.Create( r.TargetFramework!, 
                        TargetFrameworkTextStyle.Simple,
                        out var supportedFW ) )
                    {
                        return supportedFW.Framework == tgtFW.Framework
                            && supp
                    }
                } );

            if( projResults == null )
            {
                _logger.Error<string, TargetFramework>( "Could not find a configuration for project {0} targeting {1}",
                    ProjectName, 
                    tgtFW );
                return false;
            }

            try
            {
                ProjectName = GetMSBuildProperty<string>(projResults, "ProjectName");

                var outputType = GetMSBuildEnum<OutputType>(projResults, "OutputType");

                OutputKind = outputType switch
                {
                    OutputType.Library => OutputKind.DynamicallyLinkedLibrary,
                    OutputType.Exe => OutputKind.ConsoleApplication,
                    OutputType.Module => OutputKind.NetModule,
                    OutputType.WinExe => OutputKind.WindowsRuntimeApplication,
                    _ => throw new ArgumentOutOfRangeException(
                        $"Unhandled {nameof(Roslyn.OutputType)} '{outputType}'")
                };

                NullableContextOptions = GetMSBuildEnum<NullableContextOptions>(projResults, "Nullable");
                WarningLevel = GetMSBuildProperty<int>(projResults, "WarningLevel");
                OptimizationLevel = GetMSBuildProperty<bool>(projResults, "Optimize")
                    ? OptimizationLevel.Release
                    : OptimizationLevel.Debug;

                IgnoredWarnings = GetMSBuildProperty<string>(projResults, "NoWarn")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .ToDictionary(x => $"CS{x}", x => ReportDiagnostic.Suppress);

                Platform = GetMSBuildEnum<Platform>(projResults, "PlatformName");
            }
            catch
            {
                return false;
            }

            // create the syntax trees by parsing the source code files
            var trees = new List<SyntaxTree>();

            foreach (var srcFile in projResults.SourceFiles)
            {
                try
                {
                    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(srcFile));
                    trees.Add(tree);
                }
                catch (Exception e)
                {
                    _logger.Error<string, string>(
                        "Failed to parse file '{0}' (Exception was {1})",
                        srcFile,
                        e.Message);

                    return false;
                }
            }

            // compile the project
            var options = new CSharpCompilationOptions(outputKind: outputKind)
                .WithNullableContextOptions(nullableContext)
                .WithWarningLevel(warningLevel)
                .WithOptimizationLevel(optimizeLevel)
                .WithSpecificDiagnosticOptions(noWarnings)
                .WithPlatform(platform);

            CSharpCompilation compilation;

            try
            {
                compilation = CSharpCompilation.Create(projName, options: options)
                    .AddReferences(projResults.References.Select(r => MetadataReference.CreateFromFile(r)))
                    .AddSyntaxTrees(trees);
            }
            catch (Exception e)
            {
                _logger.Error<string, string>(
                    "Failed to compile project '{0}' (Exception was {1})",
                    projName,
                    e.Message);

                return false;
            }

            // create the syntax/semantic info we'll be searching
            foreach (var tree in trees)
            {
                if (!tree.TryGetRoot(out var root))
                {
                    CompilationResults.Clear();

                    _logger.Error<string, string>(
                        "Failed to get {0} for project {1}",
                        nameof(CompilationUnitSyntax),
                        projName);

                    return false;
                }

                try
                {
                    CompilationResults.Add(new CompilationResult(
                        root,
                        compilation.GetSemanticModel(tree))
                    );
                }
                catch (Exception e)
                {
                    CompilationResults.Clear();

                    _logger.Error<string, string, string>(
                        "Failed to get {0} for project {1} (Exception was {2})",
                        nameof(SemanticModel),
                        projName,
                        e.Message);

                    return false;
                }
            }

            IsCompiled = true;

            Diagnostics.AddRange(compilation.GetDiagnostics());

            return IsValid;
        }

        private TProp GetMSBuildProperty<TProp>( IAnalyzerResult analyzerResult, string propName )
        {
            if( analyzerResult.Properties.ContainsKey( propName ) )
            {
                string textValue = string.Empty;

                try
                {
                    textValue = analyzerResult.Properties[ propName ];

                    return (TProp) Convert.ChangeType( textValue, typeof(TProp) );
                }
                catch( Exception e )
                {
                    _logger.Error<string, Type, Exception>(
                        "Couldn't retrieve property '{0}' from MSBuild properties and convert it to {1}, exception was: {2} ",
                        textValue,
                        typeof(TProp),
                        e );

                    // ReSharper disable once PossibleIntendedRethrow
                    throw e!;
                }
            }

            _logger.Error<string>( "Couldn't retrieve property '{0}' from MSBuild properties", propName );

            throw new ArgumentException();
        }

        private TProp GetMSBuildEnum<TProp>( IAnalyzerResult analyzerResult, string propName )
            where TProp : Enum
        {
            if( analyzerResult.Properties.ContainsKey( propName ) )
            {
                string textValue = analyzerResult.Properties[ propName ];

                if( Enum.TryParse( typeof(TProp), textValue, true, out var result ) )
                    return (TProp) result!;

                _logger.Error<string, string, Type>( "Couldn't convert '{0}' from MSBuild property '{1}' to {2}",
                    textValue,
                    propName,
                    typeof(TProp) );
            }
            else _logger.Error<string>( "Couldn't retrieve property '{0}' from MSBuild properties", propName );

            throw new ArgumentException();
        }
    }
}
