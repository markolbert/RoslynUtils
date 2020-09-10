using System.Collections.Immutable;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSink : RoslynDbSink<ITypeSymbol>
    {
        public class TypeInfo
        {
            public TypeInfo( TypeSink typeSink, ITypeSymbol originalSymbol )
            {
                OriginalSymbol = originalSymbol;
            }

            public ITypeSymbol OriginalSymbol { get; }
            public ITypeSymbol SymbolToProcess { get; }
            public SharpObjectType Type { get; }
            public bool InDocumentationScope { get; }
            public bool IsTopLevel { get; }
        }

        private TopologicallySortableCollection<ISymbol> _symbols = new TopologicallySortableCollection<ISymbol>();

        public TypeSink(
            UniqueSymbols<ITypeSymbol> uniqueSymbols,
            IJ4JLogger logger,
            ISymbolProcessors<ITypeSymbol>? processors = null )
            : base( uniqueSymbols, logger, processors)
        {
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
            var junk = _symbols.Sort(out var sorted);

            if ( !base.FinalizeSink( syntaxWalker ) )
                return false;

            return true;
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, ITypeSymbol symbol )
        {
            // we don't call the base implementation because it tries to add the symbol
            // which is fine but we need to drill into it's parentage and we only want to
            // do that if we haven't visited it before
            StoreTypeTree( symbol );

            return true;
        }

        private void StoreTypeTree( ITypeSymbol symbol )
        {
            // if we've visited this symbol go no further
            if( /*symbol is ITypeParameterSymbol ||*/ !Symbols.Add( symbol ) )
                return;

            if( symbol.BaseType != null )
                _symbols.Add( symbol, symbol.BaseType );

            // if this is an INamedTypeSymbol, store any implementable type symbols
            if( symbol is INamedTypeSymbol ntSymbol )
                StoreTypeParameters( ntSymbol.TypeParameters );
            //StoreImplementableTypeArguments( ntSymbol.TypeArguments );

            foreach ( var interfaceSymbol in symbol.Interfaces )
            {
                if( interfaceSymbol == null )
                    continue;

                _symbols.Add( symbol, interfaceSymbol );

                if( !Symbols.Add( interfaceSymbol ) )
                    continue;

                // add ancestors related to the interface symbol
                StoreTypeTree( interfaceSymbol );

                // add ancestors related to closed generic types, if any, in
                // the interface
                StoreTypeParameters(interfaceSymbol.TypeParameters);
                //StoreImplementableTypeArguments( interfaceSymbol.TypeArguments );
            }

            // array symbols have two ancestry paths, one pointing to Array
            // and the other pointing to whatever type of element they contain
            if( symbol is IArrayTypeSymbol arraySymbol )
                StoreTypeTree(arraySymbol.ElementType);

            var baseSymbol = symbol.BaseType;

            while( baseSymbol != null )
            {
                Symbols.Add( baseSymbol );

                if (baseSymbol.BaseType != null)
                    _symbols.Add(baseSymbol, baseSymbol.BaseType);

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
