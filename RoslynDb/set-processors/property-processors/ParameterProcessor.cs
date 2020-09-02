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
    public class ParameterProcessor : BaseProcessorDb<IPropertySymbol, IParameterSymbol>
    {
        public ParameterProcessor( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolNamer, 
            IJ4JLogger logger ) 
            : base( dbContext, symbolNamer, logger )
        {
        }

        protected override IEnumerable<IParameterSymbol> ExtractSymbols( object item )
        {
            if (!(item is IParameterSymbol propSymbol) )
            {
                Logger.Error("Supplied item is not an IParameterSymbol");
                yield break;
            }

            yield return propSymbol;
        }

        protected override bool ProcessSymbol( IParameterSymbol symbol )
        {
            var propSymbol = (IPropertySymbol) symbol.ContainingSymbol;

            if( !GetByFullyQualifiedName<PropertyDb>( propSymbol, out var propDb ) )
                return false;

            var typeDb = GetTypeByFullyQualifiedName( symbol.Type );

            if( typeDb == null )
            {
                Logger.Error<string>( "Couldn't find type for IParameterSymbol '{0}'",
                    SymbolNamer.GetFullyQualifiedName( symbol ) );

                return false;
            }

            var propParams = GetDbSet<PropertyParameterDb>();

            var propParamDb = propParams
                .FirstOrDefault( pp => pp.PropertyID == propDb!.ID && pp.Ordinal == symbol.Ordinal );

            if( propParamDb == null )
            {
                propParamDb = new PropertyParameterDb
                {
                    PropertyID = propDb!.ID,
                    Ordinal = symbol.Ordinal
                };

                propParams.Add( propParamDb );
            }

            propParamDb.Synchronized = true;
            propParamDb.Name = SymbolNamer.GetName( symbol );
            propParamDb.ParameterTypeID = typeDb.ID;
            propParamDb.IsAbstract = symbol.IsAbstract;
            propParamDb.IsExtern = symbol.IsExtern;
            propParamDb.IsOverride = symbol.IsOverride;
            propParamDb.IsSealed = symbol.IsSealed;
            propParamDb.IsStatic = symbol.IsStatic;
            propParamDb.IsVirtual = symbol.IsVirtual;

            return true;
        }
    }
}
