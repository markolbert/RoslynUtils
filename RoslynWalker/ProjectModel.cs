using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace J4JSoftware.Roslyn
{
    public class CompilationResult
    {
        public CompilationResult( SyntaxNode syntax, SemanticModel model )
        {
            Syntax = syntax;
            Model = model;
        }

        public SyntaxNode Syntax { get; }
        public SemanticModel Model { get; }
    }

    public class ProjectModel
    {
        private readonly AssemblyLoadContext _loadContext;
        private readonly IJ4JLogger _logger;

        private bool _nonCompOkay = true;

        public ProjectModel( AssemblyLoadContext loadContext, IJ4JLogger logger )
        {
            _loadContext = loadContext;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public bool IsValid => _nonCompOkay && !HasCompilationErrors;

        public List<CompilationResult> CompilationResults { get; } = new List<CompilationResult>();
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public bool HasCompilationProblems( DiagnosticSeverity severity ) =>
            Diagnostics.Any( d => d.Severity == severity );

        public bool HasCompilationErrors => HasCompilationProblems( DiagnosticSeverity.Error );

        public bool Compile( ProjectAssets projAssets, TargetFramework tgtFramework,
            List<RequiredAssembly>? externalAssemblies = null )
        {
            _nonCompOkay = false;

            if( projAssets.ProjectLibrary == null )
            {
                _logger.Error<string, string>( "{0} undefined in {1}",
                    nameof(projAssets.ProjectLibrary),
                    nameof(projAssets) );

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
            var assemblies = externalAssemblies == null ? new List<RequiredAssembly>() : externalAssemblies.ToList();

            // add the ones defined for netstandard
            if( tgtFramework.Framework == CSharpFramework.NetStandard )
                assemblies.Add( new RequiredAssembly { AssemblyName = "netstandard" } );

            // finally, add the assemblies defined in the project file
            foreach( var libInfo in projAssets.Libraries.Where( lib => lib is PackageLibrary )
                .Cast<PackageLibrary>() )
            {
                var pkgAssembly = new RequiredAssembly { AssemblyName = libInfo.Assembly };

                //if( libInfo.GetAbsolutePath( projAssets.PackageFolders, tgtFramework, out var absPathResult ) )
                //    pkgAssembly.AssemblyPath = absPathResult!.DllPath;

                assemblies.Add( pkgAssembly );
            }

            // now create the MetadataReferences for the assemblies
            var references = new List<MetadataReference>();

            foreach( var reqdAssembly in assemblies )
            {
                // start by trying to load the assembly by name since the framework is much
                // cleverer than me at finding assemblies...
                if( !string.IsNullOrEmpty( reqdAssembly.AssemblyName )
                    && LoadFromAssemblyName( reqdAssembly.AssemblyName!, out var mdRef ) )
                {
                    references.Add( mdRef! );
                    continue;
                }

                // if loading by name didn't work, next try loading from the file path, if one exists
                if( !string.IsNullOrEmpty( reqdAssembly.AssemblyPath )
                    && LoadFromFilePath( reqdAssembly.AssemblyPath, out var mdRef2 ) )
                    references.Add( mdRef2! );
            }

            // create the syntax trees by parsing the source code files
            var trees = new List<SyntaxTree>();

            foreach( var srcFile in srcFiles! )
            {
                try
                {
                    var tree = CSharpSyntaxTree.ParseText( srcFile );
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

            CSharpCompilation compilation;

            try
            {
                compilation = CSharpCompilation.Create( projAssets.Name, options : options )
                    .AddReferences( references )
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

        private bool LoadFromAssemblyName(string aName, out MetadataReference? result)
        {
            result = null;

            try
            {
                var assembly = _loadContext.LoadFromAssemblyName(new AssemblyName(aName));

                if (assembly == null)
                    return false;

                result = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(assembly.Location);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return false;
            }

            return true;
        }

        private bool LoadFromFilePath(string path, out MetadataReference? result)
        {
            result = null;

            try
            {
                var assembly = _loadContext.LoadFromAssemblyPath(path);

                result = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(assembly.Location);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return false;
            }

            return true;
        }
    }
}
