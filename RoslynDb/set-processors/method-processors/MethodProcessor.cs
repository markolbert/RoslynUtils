using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodProcessor : BaseProcessorDb<IMethodSymbol, IMethodSymbol>
    {
        public MethodProcessor( 
            RoslynDbContext dbContext,
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger ) 
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override IEnumerable<IMethodSymbol> ExtractSymbols( object item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            yield return methodSymbol;
        }

        protected override bool ProcessSymbol( IMethodSymbol symbol )
        {
            if( !EntityFactories.Retrieve<ImplementableTypeDb>( symbol.ContainingType, out var typeDb ) )
            {
                Logger.Error<string>("Couldn't find containing type for IMethod '{0}'",
                    SymbolNamer.GetFullyQualifiedName(symbol));

                return false;
            }

            if( !EntityFactories.Retrieve<TypeDb>(symbol.ReturnType, out var retValDb  ))
            {
                Logger.Error<string, string>("Couldn't find return type '{0}' in database for method '{1}'",
                    SymbolNamer.GetFullyQualifiedName(symbol.ReturnType),
                    SymbolNamer.GetFullyQualifiedName(symbol));

                return false;
            }

            if( !EntityFactories.Retrieve<MethodDb>( symbol, out var methodDb, true ) )
                return false;

            MarkSynchronized( methodDb! );

            methodDb!.DefiningTypeID = typeDb!.SharpObjectID;
            methodDb.ReturnTypeID = retValDb!.SharpObjectID;

            return true;
        }
    }
}
