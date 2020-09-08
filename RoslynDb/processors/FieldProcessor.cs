using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FieldProcessor : BaseProcessorDb<IFieldSymbol, IFieldSymbol>
    {
        public FieldProcessor( 
            IEntityFactories factories,
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

        protected override bool InitializeProcessor( IEnumerable<IFieldSymbol> inputData )
        {
            if( !base.InitializeProcessor( inputData ) )
                return false;

            EntityFactories.MarkUnsynchronized<FieldDb>();

            return true;
        }

        protected override bool ProcessSymbol( IFieldSymbol symbol )
        {
            if( !EntityFactories.Retrieve<ImplementableTypeDb>(symbol.ContainingType, out var typeDb  ))
            {
                Logger.Error<string>( "Couldn't find containing type for IFieldSymbol '{0}'",
                    EntityFactories.GetFullyQualifiedName( symbol ) );

                return false;
            }

            if(!EntityFactories.Retrieve<TypeDb>(symbol.Type, out var fieldTypeDb))
            {
                Logger.Error<string, string>( "Couldn't find return type '{0}' in database for field '{1}'",
                    EntityFactories.GetFullyQualifiedName( symbol.Type ),
                    EntityFactories.GetFullyQualifiedName(symbol) );

                return false;
            }

            if( !EntityFactories.Retrieve<FieldDb>( symbol, out var fieldDb, true ) )
                return false;

            EntityFactories.MarkSynchronized( fieldDb! );

            fieldDb!.DefiningTypeID = typeDb!.SharpObjectID;
            fieldDb.FieldTypeID = fieldTypeDb!.SharpObjectID;

            return true;
        }
    }
}
