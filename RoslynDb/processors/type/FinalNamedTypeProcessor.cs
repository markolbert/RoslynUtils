using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FinalNamedTypeProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public FinalNamedTypeProcessor(
            EntityFactories factories,
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

            if( !( typeSymbol is INamedTypeSymbol ntSymbol ) )
                yield break;

            // we handle INamedTypeSymbols provided they have a named type argument that
            // is a parametric type somewhere in their family tree
            var visitedNames = new List<string>();

            var crap = EntityFactories.GetFullName( ntSymbol );

            if (HasParametricTypes(ntSymbol, ref visitedNames))
                yield return ntSymbol;
        }

        // INamedTypeSymbol is guaranteed to have parametric types in its
        // type arguments
        protected override bool ProcessSymbol( INamedTypeSymbol symbol )
        {
            if( !EntityFactories.Get<AssemblyDb>( symbol.ContainingAssembly, out var assemblyDb ) )
                return false;

            if( !EntityFactories.Get<NamespaceDb>( symbol.ContainingNamespace, out var nsDb ) )
                return false;

            ImplementableTypeDb? dbSymbol = null;

            if( symbol.IsGenericType && EntityFactories.Create<GenericTypeDb>( symbol, out var dbTemp ) )
                dbSymbol = dbTemp;
            else
            {
                if( EntityFactories.Create<FixedTypeDb>( symbol, out var dbTemp2 ) )
                    dbSymbol = dbTemp2;
            }

            if( dbSymbol == null )
            {
                Logger.Error<string>( "Could not retrieve ImplementationTypeDb entity for '{0}'",
                    EntityFactories.GetFullName( symbol ) );

                return false;
            }

            EntityFactories.MarkSynchronized( dbSymbol );

            dbSymbol.AssemblyID = assemblyDb!.SharpObjectID;
            dbSymbol.NamespaceID = nsDb!.SharpObjectID;

            return true;
        }
    }
}
