using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodProcessor : BaseProcessorDb<IMethodSymbol, IMethodSymbol>
    {
        public MethodProcessor( 
            IEntityFactories factories,
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

        protected override bool InitializeProcessor( IEnumerable<IMethodSymbol> inputData )
        {
            if( !base.InitializeProcessor( inputData ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<MethodDb>();
            EntityFactories.MarkUnsynchronized<ArgumentDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<MethodParametricTypeDb>(true);

            return true;
        }

        protected override bool ProcessSymbol( IMethodSymbol symbol )
        {
            if( !EntityFactories.Retrieve<ImplementableTypeDb>( symbol.ContainingType, out var typeDb ) )
            {
                Logger.Error<string>("Couldn't find containing type for IMethodSymbol '{0}'",
                    EntityFactories.GetFullName(symbol));

                return false;
            }

            if( !EntityFactories.Retrieve<TypeDb>(symbol.ReturnType, out var retValDb  ))
            {
                Logger.Error<string, string>("Couldn't find return type '{0}' in database for IMethodSymbol '{1}'",
                    EntityFactories.GetFullName(symbol.ReturnType),
                    EntityFactories.GetFullName(symbol));

                return false;
            }

            if( !EntityFactories.Retrieve<MethodDb>( symbol, out var methodDb, true ) )
                return false;

            EntityFactories.MarkSynchronized( methodDb! );

            methodDb!.DefiningTypeID = typeDb!.SharpObjectID;
            methodDb.ReturnTypeID = retValDb!.SharpObjectID;

            return true;
        }
    }
}
