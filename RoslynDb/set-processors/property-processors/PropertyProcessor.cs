using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class PropertyProcessor : BaseProcessorDb<IPropertySymbol, IPropertySymbol>
    {
        public PropertyProcessor( 
            RoslynDbContext dbContext, 
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger ) 
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override IEnumerable<IPropertySymbol> ExtractSymbols( object item )
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
            if( !EntityFactories.Retrieve<ImplementableTypeDb>(symbol.ContainingType, out var typeDb  ))
            {
                Logger.Error<string>( "Couldn't find containing type for IProperty '{0}'",
                    SymbolNamer.GetFullyQualifiedName( symbol ) );

                return false;
            }

            if(!EntityFactories.Retrieve<TypeDb>(symbol.Type, out var propTypeDb))
            {
                Logger.Error<string, string>( "Couldn't find return type '{0}' in database for property '{1}'",
                    SymbolNamer.GetFullyQualifiedName( symbol.Type ),
                    SymbolNamer.GetFullyQualifiedName(symbol) );

                return false;
            }

            if( !EntityFactories.Retrieve<PropertyDb>( symbol, out var propDb, true ) )
                return false;

            propDb!.DefiningTypeID = typeDb!.SharpObjectID;
            propDb.PropertyTypeID = propTypeDb!.SharpObjectID;

            return true;
        }
    }
}
