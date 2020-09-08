using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamedTypeProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public NamedTypeProcessor(
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

            // we handle INamedTypeSymbols
            if( typeSymbol is INamedTypeSymbol ntSymbol )
                yield return ntSymbol;
        }

        protected override bool InitializeProcessor( IEnumerable<ITypeSymbol> inputData )
        {
            if( !base.InitializeProcessor( inputData ) )
                return false;

            EntityFactories.MarkUnsynchronized<FixedTypeDb>();
            EntityFactories.MarkUnsynchronized<GenericTypeDb>();
            EntityFactories.MarkUnsynchronized<TypeParametricTypeDb>();
            EntityFactories.MarkUnsynchronized<MethodParametricTypeDb>();
            EntityFactories.MarkUnsynchronized<ParametricTypeDb>();
            EntityFactories.MarkUnsynchronized<TypeAncestorDb>();
            EntityFactories.MarkUnsynchronized<TypeArgumentDb>( true );

            return true;
        }

        protected override bool ProcessSymbol( INamedTypeSymbol symbol )
        {
            if( !RetrieveAssembly( symbol.ContainingAssembly, out var assemblyDb ) )
                return false;

            if( !RetrieveNamespace( symbol.ContainingNamespace, out var nsDb ) )
                return false;

            ImplementableTypeDb? dbSymbol = null;

            if( symbol.IsGenericType && EntityFactories.Retrieve<GenericTypeDb>( symbol, out var dbTemp, true ) )
                dbSymbol = dbTemp;
            else
            {
                if( EntityFactories.Retrieve<FixedTypeDb>( symbol, out var dbTemp2, true ) )
                    dbSymbol = dbTemp2;
            }

            if( dbSymbol == null )
            {
                Logger.Error<string>( "Could not retrieve ImplementationTypeDb entity for '{0}'",
                    EntityFactories.GetFullyQualifiedName( symbol ) );

                return false;
            }

            EntityFactories.MarkSynchronized( dbSymbol );

            dbSymbol.AssemblyID = assemblyDb!.SharpObjectID;
            dbSymbol.NamespaceID = nsDb!.SharpObjectID;

            return true;
        }
    }
}
