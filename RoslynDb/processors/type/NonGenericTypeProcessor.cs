using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NonGenericTypeProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public NonGenericTypeProcessor(
            IEntityFactories factories,
            IJ4JLogger logger
        )
            : base( factories, logger )
        {
        }

        protected override IEnumerable<INamedTypeSymbol> ExtractSymbols( ISymbol item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error( "Supplied item is not an ITypeSymbol" );
                yield break;
            }

            if( typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol )
            {
                Logger.Error<string>( "Unhandled ITypeSymbol '{0}'", typeSymbol.Name );
                yield break;
            }

            if( typeSymbol is IErrorTypeSymbol )
            {
                Logger.Error( "ITypeSymbol is an IErrorTypeSymbol, ignored" );
                yield break;
            }

            // we handle INamedTypeSymbols that have no type arguments
            if( typeSymbol is INamedTypeSymbol ntSymbol
                && !ntSymbol.IsGenericType
                && ntSymbol.TypeArguments.Length == 0 )
                yield return ntSymbol;
        }

        // INamedTypeSymbol is guaranteed not to have any TypeArguments and to not be a generic type
        protected override bool ProcessSymbol( INamedTypeSymbol symbol )
        {
            if( !RetrieveAssembly( symbol.ContainingAssembly, out var assemblyDb ) )
                return false;

            if( !RetrieveNamespace( symbol.ContainingNamespace, out var nsDb ) )
                return false;

            if( !EntityFactories.Retrieve<FixedTypeDb>( symbol, out var dbSymbol, true ) )
            {
                Logger.Error<string>( "Could not create entity for '{0}'",
                    EntityFactories.GetFullName( symbol ) );

                return false;
            }

            EntityFactories.MarkSynchronized( dbSymbol! );

            dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
            dbSymbol.NamespaceID = nsDb!.SharpObjectID;

            return true;
        }
    }
}
