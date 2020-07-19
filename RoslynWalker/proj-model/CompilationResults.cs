using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class CompilationResults : IEnumerable<CompilationResult>
    {
        public static bool Create( ProjectModel projModel, CSharpCompilation compilation, out CompilationResults? result, out string? error )
        {
            result = null;
            error = null;

            var retVal = new CompilationResults
            {
                AssemblySymbol = compilation.Assembly,
                ProjectModel = projModel
            };


            foreach( var tree in compilation.SyntaxTrees )
            {
                if( !tree.TryGetRoot( out var rootNode ) )
                {
                    error = $"Couldn't get root of {typeof(SyntaxTree)}";
                    return false;
                }

                try
                {
                    retVal._results.Add( new CompilationResult( rootNode, compilation.GetSemanticModel( tree ), retVal ) );
                }
                catch( Exception e )
                {
                    error =
                        $"Couldn't get {typeof(SemanticModel)} for {typeof(SyntaxTree)}. Exception message was {e.Message}";

                    return false;
                }
            }

            result = retVal;

            return true;
        }

        private readonly List<CompilationResult> _results = new List<CompilationResult>();

#pragma warning disable 8618
        private CompilationResults()
#pragma warning restore 8618
        {
        }

        public IAssemblySymbol AssemblySymbol { get; private set; }
        public ProjectModel ProjectModel { get; private set; }
        public ReadOnlyCollection<CompilationResult> Results => _results.AsReadOnly();

        public IEnumerator<CompilationResult> GetEnumerator()
        {
            foreach( var result in _results )
            {
                yield return result;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}