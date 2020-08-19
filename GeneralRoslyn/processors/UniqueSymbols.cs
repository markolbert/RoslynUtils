using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class UniqueSymbols<TSymbol> : IEnumerable<TSymbol>
        where TSymbol : ISymbol
    {
        private readonly Dictionary<string, TSymbol> _symbols = new Dictionary<string, TSymbol>();
        private readonly ISymbolInfoFactory _siFactory;

        public UniqueSymbols( ISymbolInfoFactory symbolInfoFactory )
        {
            _siFactory = symbolInfoFactory;
        }

        public void Clear() => _symbols.Clear();

        public List<TSymbol> Symbols => _symbols.Select( x => x.Value ).ToList();

        public bool Add( TSymbol symbol )
        {
            var fqn = _siFactory.GetFullyQualifiedName( symbol );

            if( _symbols.ContainsKey( fqn ) )
                return false;

            _symbols.Add( fqn, symbol );

            return true;
        }

        public IEnumerator<TSymbol> GetEnumerator()
        {
            foreach( var kvp in _symbols )
            {
                yield return kvp.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
