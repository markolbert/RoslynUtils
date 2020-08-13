using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ITypeDefinitionProcessors
    {
        bool Process( List<ITypeSymbol> typeSymbols );
    }
}