using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParameterProcessor : BaseProcessorDb<IPropertySymbol, IParameterSymbol>
    {
        public ParameterProcessor( 
            IEntityFactories factories,
            IJ4JLogger logger ) 
            : base( factories, logger )
        {
        }

        protected override IEnumerable<IParameterSymbol> ExtractSymbols( ISymbol item )
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
            if( !EntityFactories.Retrieve<PropertyDb>(symbol.ContainingSymbol, out var propDb  ))
                return false;

            if( !EntityFactories.Retrieve<TypeDb>(symbol.Type, out var typeDb))
            {
                Logger.Error<string>( "Couldn't find type for IParameterSymbol '{0}'",
                    EntityFactories.GetFullyQualifiedName( symbol ) );

                return false;
            }

            var propParamDb = EntityFactories.DbContext.PropertyParameters
                .FirstOrDefault( pp => pp.PropertyID == propDb!.SharpObjectID && pp.Ordinal == symbol.Ordinal );

            if( propParamDb == null )
            {
                propParamDb = new PropertyParameterDb
                {
                    PropertyID = propDb!.SharpObjectID,
                    Ordinal = symbol.Ordinal
                };

                EntityFactories.DbContext.PropertyParameters.Add( propParamDb );
            }

            propParamDb.Synchronized = true;
            propParamDb.Name = EntityFactories.GetName( symbol );
            propParamDb.ParameterTypeID = typeDb!.SharpObjectID;
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
