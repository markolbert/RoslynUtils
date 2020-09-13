﻿using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ArrayTypeProcessor : BaseProcessorDb<ITypeSymbol, IArrayTypeSymbol>
    {
        public ArrayTypeProcessor(
            EntityFactories factories,
            IJ4JLogger logger
        )
            : base( factories, logger )
        {
        }

        protected override IEnumerable<IArrayTypeSymbol> ExtractSymbols( ISymbol item )
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

            // we handle IArrayTypeSymbols, provided they aren't based on an ITypeParameterSymbol
            if( typeSymbol is IArrayTypeSymbol arraySymbol )
                yield return arraySymbol;
        }

        protected override bool ProcessSymbol( IArrayTypeSymbol symbol )
        {
            var fqn = EntityFactories.GetFullName( symbol );

            if (!EntityFactories.Get<AssemblyDb>(symbol.ElementType.ContainingAssembly, out var assemblyDb))
                return false;

            if (!EntityFactories.Get<NamespaceDb>(symbol.ElementType.ContainingNamespace, out var nsDb))
                return false;

            if( !EntityFactories.Create<TypeDb>( symbol, out var dbSymbol ) )
            {
                Logger.Error<string>("Could not retrieve TypeDb entity for '{0}'",
                    EntityFactories.GetFullName(symbol));

                return false;
            }

            EntityFactories.MarkSynchronized(dbSymbol!);

            dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
            dbSymbol.NamespaceID = nsDb!.SharpObjectID;

            return true;
        }
    }
}
