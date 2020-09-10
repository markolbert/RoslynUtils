using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class PropertyProcessor : BaseProcessorDb<IPropertySymbol, IPropertySymbol>
    {
        public PropertyProcessor( 
            IEntityFactories factories,
            IJ4JLogger logger ) 
            : base( factories, logger )
        {
        }

        protected override IEnumerable<IPropertySymbol> ExtractSymbols( ISymbol item )
        {
            if (!(item is IPropertySymbol propSymbol) )
            {
                Logger.Error("Supplied item is not an IPropertySymbol");
                yield break;
            }

            yield return propSymbol;
        }

        protected override bool InitializeProcessor( IEnumerable<IPropertySymbol> inputData )
        {
            if( !base.InitializeProcessor( inputData ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<PropertyDb>();
            EntityFactories.MarkUnsynchronized<PropertyParameterDb>( true );

            return true;
        }

        protected override bool ProcessSymbol( IPropertySymbol symbol )
        {
            if( !EntityFactories.Retrieve<ImplementableTypeDb>(symbol.ContainingType, out var typeDb  ))
            {
                Logger.Error<string>( "Couldn't find containing type for IPropertySymbol '{0}'",
                    EntityFactories.GetFullName( symbol ) );

                return false;
            }

            if(!EntityFactories.Retrieve<TypeDb>(symbol.Type, out var propTypeDb))
            {
                Logger.Error<string, string>( "Couldn't find return type '{0}' in database for property '{1}'",
                    EntityFactories.GetFullName( symbol.Type ),
                    EntityFactories.GetFullName(symbol) );

                return false;
            }

            if( !EntityFactories.Retrieve<PropertyDb>( symbol, out var propDb, true ) )
                return false;

            EntityFactories.MarkSynchronized( propDb! );

            propDb!.DefiningTypeID = typeDb!.SharpObjectID;
            propDb.PropertyTypeID = propTypeDb!.SharpObjectID;

            return true;
        }
    }
}
