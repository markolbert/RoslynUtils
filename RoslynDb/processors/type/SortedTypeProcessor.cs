using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class SortedTypeProcessor : BaseProcessorDb<ITypeSymbol, ITypeSymbol>
    {
        public SortedTypeProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base("adding basic Types to the database", dataLayer, context, logger)
        {
        }

        protected override List<ITypeSymbol> ExtractSymbols( IEnumerable<ITypeSymbol> inputData )
        {
            var retVal = new List<ITypeSymbol>();

            foreach( var symbol in inputData )
            {
                switch( symbol )
                {
                    case IDynamicTypeSymbol dtSymbol:
                        Logger.Error<string>( "IDynamicTypeSymbols are not supported ('{0}')", symbol.Name );
                        break;

                    case IPointerTypeSymbol ptSymbol:
                        Logger.Error<string>( "IPointerTypeSymbols are not supported ('{0}')", symbol.Name );
                        break;

                    case IErrorTypeSymbol errSymbol:
                        Logger.Error<string>( "IErrorTypeSymbols are not supported ('{0}')", symbol.Name );
                        break;

                    default:
                        retVal.Add( symbol );
                        break;
                }
            }

            return retVal;
        }

        protected override bool ProcessSymbol( ITypeSymbol typeSymbol )
        {
            Logger.Information<string>("Processing ITypeSymbol {0}", typeSymbol.ToUniqueName());

            if( DataLayer.GetUnspecifiedType( typeSymbol, true, true ) == null )
                return false;

            DataLayer.SaveChanges();

            return true;
        }
    }
}
