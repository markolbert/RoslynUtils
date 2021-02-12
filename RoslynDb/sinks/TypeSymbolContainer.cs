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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSymbolContainer : IEnumerable<ITypeSymbol>
    {
        private readonly Nodes<ISymbol> _interfaces;
        private readonly IJ4JLogger _logger;

        private readonly Nodes<ISymbol> _nonInterfaces;

        public TypeSymbolContainer(
            IJ4JLogger logger
        )
        {
            var comparer = new TypeSymbolComparer();

            _nonInterfaces = new Nodes<ISymbol>( comparer );
            _interfaces = new Nodes<ISymbol>( comparer );

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public IEnumerator<ITypeSymbol> GetEnumerator()
        {
            // we loop through the non-interfaces first, and always sort when we start
            if( !_nonInterfaces.Sort( out var nonInterfaces, out var remainingEdges ) )
            {
                _logger.Error( "Couldn't topologically sort the non-interface ITypeSymbols" );
                yield break;
            }

            if( !_interfaces.Sort( out var interfaces, out _ ) )
            {
                _logger.Error( "Couldn't topologically sort the interface ITypeSymbols" );
                yield break;
            }

            // not sure why the topological sorts come out backwards but they do...
            nonInterfaces!.Reverse();
            interfaces!.Reverse();

            var junkNon = nonInterfaces.Cast<ITypeSymbol>().ToList();
            var junkInt = interfaces.Cast<ITypeSymbol>().ToList();

            foreach( var ntSymbol in nonInterfaces!.Cast<ITypeSymbol>() ) yield return ntSymbol;

            foreach( var ntSymbol in interfaces!.Cast<ITypeSymbol>() ) yield return ntSymbol;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _nonInterfaces.Clear();
            _interfaces.Clear();
        }

        public bool AddNonInterfaceConnection( ITypeSymbol parentSymbol, ITypeSymbol? symbol = null )
        {
            if( parentSymbol.TypeKind == TypeKind.Interface )
            {
                _logger.Error( "Trying to add an interface to the non-interface connections collection" );
                return false;
            }

            if( ( symbol?.TypeKind ?? TypeKind.Class ) != TypeKind.Interface )
            {
                _nonInterfaces.AddDependentNode( parentSymbol, symbol! );
                return true;
            }

            _logger.Error<string>( "Target {0} is an interface", symbol!.ToFullName() );

            return false;
        }

        public bool AddInterfaceConnection( INamedTypeSymbol parentSymbol )
        {
            if( parentSymbol.TypeKind != TypeKind.Interface )
            {
                _logger.Error( "Trying to add a non-interface to the interface connections collection" );
                return false;
            }

            _interfaces.AddIndependentNode( parentSymbol );

            return true;
        }

        private class TypeSymbolComparer : IEqualityComparer<ISymbol>
        {
            public bool Equals( ISymbol? x, ISymbol? y )
            {
                if( x == null && y == null )
                    return true;

                if( x == null || y == null )
                    return false;

                return string.Equals( x.ToFullName(),
                    y.ToFullName(),
                    StringComparison.Ordinal );
            }

            public int GetHashCode( ISymbol obj )
            {
                return obj.GetHashCode();
            }
        }
    }
}