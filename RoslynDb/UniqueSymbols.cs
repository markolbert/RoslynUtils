#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class UniqueSymbols<TSymbol> : IEnumerable<TSymbol>
        where TSymbol : ISymbol
    {
        private readonly Dictionary<string, TSymbol> _symbols = new();

        public IEnumerator<TSymbol> GetEnumerator()
        {
            foreach( var value in _symbols.Select( kvp => kvp.Value ).ToList() ) yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _symbols.Clear();
        }

        public bool Add( TSymbol symbol )
        {
            var fqn = symbol.ToFullName();

            if( _symbols.ContainsKey( fqn ) )
                return false;

            _symbols.Add( fqn, symbol );

            return true;
        }
    }
}