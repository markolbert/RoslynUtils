using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FieldProcessor : BaseProcessorDb<IFieldSymbol, IFieldSymbol>
    {
        public FieldProcessor( 
            EntityFactories factories,
            IJ4JLogger logger ) 
            : base( factories, logger )
        {
        }

        protected override IEnumerable<IFieldSymbol> ExtractSymbols( ISymbol item )
        {
            if (!(item is IFieldSymbol fieldSymbol) )
            {
                Logger.Error("Supplied item is not an IFieldSymbol");
                yield break;
            }

            yield return fieldSymbol;
        }

        protected override bool ProcessSymbol( IFieldSymbol symbol )
        {
            if( !EntityFactories.Get<ImplementableTypeDb>(symbol.ContainingType, out var typeDb  ))
            {
                Logger.Error<string>( "Couldn't find containing type for IFieldSymbol '{0}'",
                    EntityFactories.GetFullName( symbol ) );

                return false;
            }

            if(!EntityFactories.Get<TypeDb>(symbol.Type, out var fieldTypeDb))
            {
                Logger.Error<string, string>( "Couldn't find return type '{0}' in database for field '{1}'",
                    EntityFactories.GetFullName( symbol.Type ),
                    EntityFactories.GetFullName(symbol) );

                return false;
            }

            if( !EntityFactories.Create<FieldDb>( symbol, out var fieldDb ) )
                return false;

            EntityFactories.MarkSynchronized( fieldDb! );

            fieldDb!.DefiningTypeID = typeDb!.SharpObjectID;
            fieldDb.FieldTypeID = fieldTypeDb!.SharpObjectID;

            return true;
        }
    }
}
