using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSink : RoslynDbSink<ITypeSymbol>
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

                return string.Equals( _factories.GetFullName( x ), _factories.GetFullName( y ),
                    StringComparison.Ordinal );
            }

            public int GetHashCode( ISymbol obj )
            {
                return obj.GetHashCode();
            }
        }

        private readonly TopologicallySortableCollection<ISymbol> _symbols;
        private readonly EntityFactories _factories;

        public TypeSink(
            UniqueSymbols<ITypeSymbol> uniqueSymbols,
            IJ4JLogger logger,
            EntityFactories factories,
            IProcessorCollection<ITypeSymbol>? processors = null )
            : base( uniqueSymbols, logger, processors)
        {
            _factories = factories;

            _symbols = new TopologicallySortableCollection<ISymbol>( new TypeSymbolComparer( _factories ) );
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker, bool stopOnFirstError = false )
        {
            if( !base.InitializeSink( syntaxWalker, stopOnFirstError ) )
                return false;

            _symbols.Clear();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !_symbols.Sort( out var sorted, out var remainingEdges ) )
            {
                Logger.Error("Couldn't topologically sort ITypeSymbols");
                return false;
            }

            // having to do this makes no sense, but it's necessary
            sorted!.Reverse();

            if (_processors == null)
            {
                Logger.Error<Type>("No processors defined for {0}", this.GetType());
                return false;
            }

            return _processors.Process( sorted!.Cast<ITypeSymbol>(), StopOnFirstError );

            //return base.FinalizeSink( syntaxWalker );
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, ITypeSymbol symbol )
        {
            var fullName = _factories.GetFullName( symbol );

            // add our ancestor chain
            if( symbol.BaseType == null )
                _symbols.Add( symbol );
            else AddAncestors( symbol.BaseType, symbol );

            // add the symbol's interfaces
            foreach( var endOfEdge in symbol.AllInterfaces )
            {
                _symbols.Add( symbol, endOfEdge );

                // add our type arguments, if any
                foreach( var endOfEdge2 in endOfEdge.TypeArguments )
                {
                    AddAncestors( endOfEdge2, endOfEdge );
                }

                foreach (var endOfEdge3 in endOfEdge.TypeParameters)
                {
                    AddAncestors(endOfEdge3, endOfEdge);
                }
            }

            //// we don't call the base implementation because it tries to add the symbol
            //// which is fine but we need to drill into it's parentage and we only want to
            //// do that if we haven't visited it before
            //StoreTypeTree( symbol );

            return true;
        }

        private void AddAncestors( ITypeSymbol startOfEdge, ITypeSymbol endOfEdge )
        {
            if( SymbolEqualityComparer.Default.Equals( startOfEdge, endOfEdge ) )
                return;

            _symbols.Add( startOfEdge, endOfEdge );

            if( startOfEdge.BaseType == null )
                return;

            AddAncestors( startOfEdge.BaseType, startOfEdge );
        }

        private void StoreTypeTree( ITypeSymbol symbol )
        {
            var junk = symbol.ToDisplayString( EntityFactories.FullNameFormat );

            // if we've visited this symbol go no further
            if( !Symbols.Add( symbol ) )
                return;

            _symbols.Add( symbol, symbol.BaseType );

            if( symbol is INamedTypeSymbol ntSymbol )
                StoreTypeParameters( ntSymbol.TypeParameters );

            foreach ( var interfaceSymbol in symbol.Interfaces )
            {
                if( interfaceSymbol == null )
                    continue;

                StoreTypeTree( interfaceSymbol );
            }

            // array symbols have two ancestry paths, one pointing to Array
            // and the other pointing to whatever type of element they contain
            if( symbol is IArrayTypeSymbol arraySymbol )
                StoreTypeTree(arraySymbol.ElementType);

            var baseSymbol = symbol.BaseType;

            while( baseSymbol != null )
            {
                if (baseSymbol.BaseType != null)
                    StoreTypeTree(baseSymbol.BaseType);

                baseSymbol = baseSymbol.BaseType;
            }
        }

        private void StoreImplementableTypeArguments( ImmutableArray<ITypeSymbol> typeArgs )
        {
            foreach (var implTypeSymbol in typeArgs
                .Where(x => !(x is ITypeParameterSymbol)))
            {
                StoreTypeTree(implTypeSymbol);
            }
        }

        private void StoreTypeParameters( ImmutableArray<ITypeParameterSymbol> typeParameters )
        {
            foreach (var typeParam in typeParameters)
            {
                StoreTypeTree( typeParam );
            }
        }
    }
}
