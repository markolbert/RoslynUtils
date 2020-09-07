using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSink : PostProcessDbSink<ITypeSymbol, FixedTypeDb>
    {
        public TypeSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger,
            ISymbolProcessors<ITypeSymbol>? processors = null )
            : base( dbContext, symbolNamer, sharpObjMapper, logger, processors)
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            MarkUnsynchronized<FixedTypeDb>();
            MarkUnsynchronized<GenericTypeDb>();
            MarkUnsynchronized<TypeParametricTypeDb>();
            MarkUnsynchronized<MethodParametricTypeDb>();
            MarkUnsynchronized<ParametricTypeDb>();
            MarkUnsynchronized<TypeAncestorDb>();
            MarkUnsynchronized<TypeArgumentDb>();

            SaveChanges();

            return true;
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, ITypeSymbol symbol )
        {
            var fqn = SymbolNamer.GetFullyQualifiedName( symbol );

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

            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                if( !Symbols.Add( interfaceSymbol ) )
                    continue;

                // add ancestors related to the interface symbol
                StoreTypeTree( interfaceSymbol );

                // add ancestors related to closed generic types, if any, in
                // the interface
                foreach( var closingSymbol in interfaceSymbol.TypeArguments
                    .Where( x => !( x is ITypeParameterSymbol ) ) )
                {
                    StoreTypeTree( closingSymbol );
                }
            }

            // array symbols have two ancestry paths, one pointing to Array
            // and the other pointing to whatever type of element they contain
            if( symbol is IArrayTypeSymbol arraySymbol )
                StoreTypeTree(arraySymbol.ElementType);

            var baseSymbol = symbol.BaseType;

            while( baseSymbol != null )
            {
                Symbols.Add( baseSymbol );

                baseSymbol = baseSymbol.BaseType;
            }
        }
    }
}
