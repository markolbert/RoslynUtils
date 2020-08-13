using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
    public class TypeProcessorContext
    {
        public TypeProcessorContext( ISyntaxWalker syntaxWalker, List<ITypeSymbol> typeSymbols )
        {
            SyntaxWalker = syntaxWalker;
            TypeSymbols = typeSymbols;
        }

        public List<ITypeSymbol> TypeSymbols { get; }
        public ISyntaxWalker SyntaxWalker { get; }
    }
}