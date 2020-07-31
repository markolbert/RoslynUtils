using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeProcessorContext
    {
        public TypeProcessorContext( ISyntaxWalker syntaxWalker, List<INamedTypeSymbol> typeSymbols )
        {
            SyntaxWalker = syntaxWalker;
            TypeSymbols = typeSymbols;
        }

        public List<INamedTypeSymbol> TypeSymbols { get; }
        public ISyntaxWalker SyntaxWalker { get; }
    }
}