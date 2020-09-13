﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
    public interface ISymbolProcessors<in TSymbol> 
        where TSymbol : ISymbol
    {
        bool Process( IEnumerable<TSymbol> context, bool stopOnFirstError = false );
    }
}