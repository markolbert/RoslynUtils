using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParameterProcessor : BaseProcessorDb<IPropertySymbol, IParameterSymbol>
    {
        public ParameterProcessor(
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger)
            : base(dataLayer, logger)
        {
        }

        protected override IEnumerable<IParameterSymbol> ExtractSymbols( ISymbol item )
        {
            if (!(item is IPropertySymbol propSymbol) )
            {
                Logger.Error<string>( "Supplied item is not an IPropertySymbol ({0})",
                    item.ToFullName() );

                yield break;
            }

            foreach( var paramSymbol in propSymbol.Parameters )
            {
                yield return paramSymbol;
            }
        }

        protected override bool ProcessSymbol( IParameterSymbol symbol ) =>
            DataLayer.GetPropertyParameter( symbol, true ) != null;
    }
}
