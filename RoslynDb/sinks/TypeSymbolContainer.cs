using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSymbolContainer : IEnumerable<ITypeSymbol>
    {
        private class TypeSymbolComparer : IEqualityComparer<ISymbol>
        {
            private readonly EntityFactories _factories;

            public TypeSymbolComparer( EntityFactories factories )
            {
                _factories = factories;
            }

            public bool Equals( ISymbol? x, ISymbol? y )
            {
                if( x == null && y == null )
                    return true;

                if( x == null || y == null )
                    return false;

                return string.Equals( _factories.GetFullName( x ), 
                    _factories.GetFullName( y ),
                    StringComparison.Ordinal );
            }

            public int GetHashCode( ISymbol obj )
            {
                return obj.GetHashCode();
            }
        }

        private readonly TopologicallySortableCollection<ISymbol> _nonInterfaces;
        private readonly TopologicallySortableCollection<ISymbol> _interfaces;
        private readonly EntityFactories _factories;
        private readonly IJ4JLogger _logger;

        public TypeSymbolContainer(
            EntityFactories factories,
            IJ4JLogger logger 
            )
        {
            _factories = factories;

            var comparer = new TypeSymbolComparer( _factories );

            _nonInterfaces = new TopologicallySortableCollection<ISymbol>( comparer );
            _interfaces = new TopologicallySortableCollection<ISymbol>( comparer );

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public void Clear()
        {
            _nonInterfaces.Clear();
            _interfaces.Clear();
        }

        public bool AddConnection( ITypeSymbol parentSymbol, ITypeSymbol? symbol = null )
        {
            if (parentSymbol.TypeKind == TypeKind.Interface
                && (symbol?.TypeKind ?? TypeKind.Interface) == TypeKind.Interface)
            {
                _interfaces.Add(parentSymbol, symbol);
                return true;
            }

            if (parentSymbol.TypeKind != TypeKind.Interface
                && (symbol?.TypeKind ?? TypeKind.Class) != TypeKind.Interface)
            {
                _nonInterfaces.Add( parentSymbol, symbol );
                return true;
            }

            _logger.Error<string, string>(
                "Trying to add ITypeSymbols where one is an interface and one is not, which is not allowed ({0}, {1})",
                parentSymbol.Name, symbol!.Name);

            return false;
        }

        public IEnumerator<ITypeSymbol> GetEnumerator()
        {
            // we loop through the non-interfaces first, and always sort when we start
            if( !_nonInterfaces.Sort( out var nonInterfaces, out var remainingEdges ) )
            {
                _logger.Error("Couldn't topologically sort the non-interface ITypeSymbols");
                yield break;
            }

            var junk = _interfaces.Nodes.FirstOrDefault( x => _factories
                .GetFullName(x)
                .IndexOf( "IEnumerable<T>", StringComparison.Ordinal ) >= 0 );

            if (!_interfaces.Sort(out var interfaces, out _))
            {
                _logger.Error("Couldn't topologically sort the interface ITypeSymbols");
                yield break;
            }

            var junk2 = interfaces!.FirstOrDefault(x => _factories
                .GetFullName(x)
                .IndexOf("IEnumerable<T>", StringComparison.Ordinal) >= 0);

            // not sure why the topological sorts come out backwards but they do...
            nonInterfaces!.Reverse();
            interfaces!.Reverse();

            var junkNon = nonInterfaces.Cast<ITypeSymbol>().ToList();
            var junkInt = interfaces.Cast<ITypeSymbol>().ToList();

            foreach( var ntSymbol in nonInterfaces!.Cast<ITypeSymbol>() )
            {
                yield return ntSymbol;
            }

            foreach( var ntSymbol in interfaces!.Cast<ITypeSymbol>() )
            {
                yield return ntSymbol;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}