using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class UniqueSymbols<TSymbol> : IEnumerable<TSymbol>
        where TSymbol : ISymbol
    {
        private readonly Dictionary<string, TSymbol> _symbols = new Dictionary<string, TSymbol>();

        public void Clear() => _symbols.Clear();

        public bool Add( TSymbol symbol )
        {
            var fqn = symbol.ToFullName();

            if( _symbols.ContainsKey( fqn ) )
                return false;

            _symbols.Add( fqn, symbol );

            return true;
        }

        public IEnumerator<TSymbol> GetEnumerator()
        {
            foreach( var value in _symbols.Select( kvp => kvp.Value ).ToList() )
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
