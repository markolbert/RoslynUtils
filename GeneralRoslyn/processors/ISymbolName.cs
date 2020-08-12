﻿using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
    public interface ISymbolName
    {
        string GetFullyQualifiedName( ISymbol symbol );
        string GetName( ISymbol symbol );
    }
}