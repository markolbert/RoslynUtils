using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class PropertyProcessor : BaseProcessorDb<IPropertySymbol, IPropertySymbol>
    {
        public PropertyProcessor( 
            EntityFactories factories,
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

        protected override bool ProcessSymbol( IPropertySymbol symbol )
        {
            if( !EntityFactories.Get<ImplementableTypeDb>(symbol.ContainingType, out var typeDb  ))
            {
                Logger.Error<string>( "Couldn't find containing type for IPropertySymbol '{0}'",
                    EntityFactories.GetFullName( symbol ) );

                return false;
            }

            if(!EntityFactories.Get<TypeDb>(symbol.Type, out var propTypeDb))
            {
                Logger.Error<string, string>( "Couldn't find return type '{0}' in database for property '{1}'",
                    EntityFactories.GetFullName( symbol.Type ),
                    EntityFactories.GetFullName(symbol) );

                return false;
            }

            if( !EntityFactories.Create<PropertyDb>( symbol, out var propDb ) )
                return false;

            EntityFactories.MarkSynchronized( propDb! );

            propDb!.DefiningTypeID = typeDb!.SharpObjectID;
            propDb.PropertyTypeID = propTypeDb!.SharpObjectID;

            return true;
        }
    }
}
