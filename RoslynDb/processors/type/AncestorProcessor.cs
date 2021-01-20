using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AncestorProcessor : BaseProcessorDb<ITypeSymbol, ITypeSymbol>
    {
        public AncestorProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Type Ancestors to the database", dataLayer, context, logger)
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
                        Logger.Error<string>("IDynamicTypeSymbols are not supported ('{0}')", symbol.Name);
                        break;

                    case IPointerTypeSymbol ptSymbol:
                        Logger.Error<string>("IPointerTypeSymbols are not supported ('{0}')", symbol.Name);
                        break;

                    case IErrorTypeSymbol errSymbol:
                        Logger.Error<string>("IErrorTypeSymbols are not supported ('{0}')", symbol.Name);
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
            var typeDb = DataLayer.GetUnspecifiedType( typeSymbol );

            if( typeDb == null )
                return false;

            // if typeSymbol is System.Object, which has no base type, we're done
            if( typeSymbol.BaseType == null )
                return true;

            if( !ProcessAncestor( typeDb!, typeSymbol.BaseType ) )
                return false;

            var allOkay = true;

            foreach( var interfaceSymbol in typeSymbol.Interfaces )
            {
                allOkay &= ProcessAncestor( typeDb!, interfaceSymbol );
            }

            return allOkay;
        }

        private bool ProcessAncestor( BaseTypeDb typeDb, INamedTypeSymbol ancestorSymbol )
        {
            var ancestorDb = DataLayer.GetImplementableType( ancestorSymbol );

            if( ancestorDb == null )
                return false;

            var typeAncestorDb = DataLayer.GetTypeAncestor( typeDb, ancestorDb!, true );

            if( typeAncestorDb == null )
                return false;

            typeAncestorDb.Synchronized = true;

            return true;
        }
    }
}
