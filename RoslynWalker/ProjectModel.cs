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
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
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

        public bool Analyze( string csProjFile )
        {
            IsAnalyzed = false;
            TargetFrameworks.Clear();

            if ( !File.Exists( csProjFile ) )
            {
                _logger.Error<string>("Project file '{0}' doesn't exist", csProjFile  );
                return false;
            }

            var analyzerMgr = new AnalyzerManager();
            var projAnalyzer = analyzerMgr.GetProject( csProjFile );
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

            ProjectFile = csProjFile;
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

            var temp = AnalyzerResults!.Results
                .Select( r =>
                {
                    if( TargetFramework.Create( r.TargetFramework, TargetFrameworkTextStyle.Simple, out var result ) )
                        return new
                        {
                            TargetFramework = result,
                            Result = r
                        };

                    throw new ArgumentException( $"Couldn't create {nameof(TargetFramework)} from '{r}'" );
                } )
                .Where( x => x.TargetFramework.Framework == tgtFW.Framework )
                .OrderBy( x => x.TargetFramework.Version )
                .FirstOrDefault( x => x.TargetFramework.Version >= tgtFW.Version );

            var projResults = temp.Result;

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
            var options = new CSharpCompilationOptions(outputKind: OutputKind)
                .WithNullableContextOptions(NullableContextOptions)
                .WithWarningLevel(WarningLevel)
                .WithOptimizationLevel(OptimizationLevel)
                .WithSpecificDiagnosticOptions(IgnoredWarnings)
                .WithPlatform(Platform);

            CSharpCompilation compilation;

            try
            {
                compilation = CSharpCompilation.Create(ProjectName, options: options)
                    .AddReferences(projResults.References.Select(r => MetadataReference.CreateFromFile(r)))
                    .AddSyntaxTrees(trees);
            }
            catch (Exception e)
            {
                _logger.Error<string, string>(
                    "Failed to compile project '{0}' (Exception was {1})",
                    ProjectName,
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
                        ProjectName);

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
                        ProjectName,
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
