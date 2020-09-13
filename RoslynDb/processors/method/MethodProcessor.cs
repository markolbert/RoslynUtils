using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodProcessor : BaseProcessorDb<IMethodSymbol, IMethodSymbol>
    {
        public MethodProcessor( 
            EntityFactories factories,
            IJ4JLogger logger ) 
            : base( factories, logger )
        {
        }

        protected override IEnumerable<IMethodSymbol> ExtractSymbols( ISymbol item )
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
            if( !EntityFactories.Get<ImplementableTypeDb>( symbol.ContainingType, out var typeDb ) )
            {
                Logger.Error<string>("Couldn't find containing type for IMethodSymbol '{0}'",
                    EntityFactories.GetFullName(symbol));

                return false;
            }

            if( !EntityFactories.Get<TypeDb>(symbol.ReturnType, out var retValDb  ))
            {
                Logger.Error<string, string>("Couldn't find return type '{0}' in database for IMethodSymbol '{1}'",
                    EntityFactories.GetFullName(symbol.ReturnType),
                    EntityFactories.GetFullName(symbol));

                return false;
            }

            if( !EntityFactories.Create<MethodDb>( symbol, out var methodDb ) )
                return false;

            EntityFactories.MarkSynchronized( methodDb! );

            methodDb!.DefiningTypeID = typeDb!.SharpObjectID;
            methodDb.ReturnTypeID = retValDb!.SharpObjectID;

            return true;
        }
    }
}
