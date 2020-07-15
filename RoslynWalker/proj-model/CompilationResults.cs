using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class CompilationResults : IEnumerable<CompilationResult>
    {
        public static bool Create( CSharpCompilation compilation, out CompilationResults? result, out string? error )
        {
            result = null;
            error = null;

            var retVal = new CompilationResults( compilation.Assembly );

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

        private CompilationResults( IAssemblySymbol symbol )
        {
            AssemblySymbol = symbol;
        }

        public IAssemblySymbol AssemblySymbol { get; }

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