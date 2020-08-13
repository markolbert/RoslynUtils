using System;
using System.Collections.Generic;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public sealed class SymbolEqualityComparer<TSymbol> : IEqualityComparer<TSymbol>
        where TSymbol : class, ISymbol
    {
        private readonly ISymbolInfoFactory _si;

        public SymbolEqualityComparer(ISymbolInfoFactory si)
        {
            _si = si;
        }

        public bool Equals(TSymbol? x, TSymbol? y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            return string.Equals(_si.GetFullyQualifiedName(x), _si.GetFullyQualifiedName(y),
                StringComparison.Ordinal);
        }

        public int GetHashCode( TSymbol obj )
        {
            return obj.GetHashCode();
        }
    }
}