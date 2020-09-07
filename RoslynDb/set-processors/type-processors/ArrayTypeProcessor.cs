﻿using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ArrayTypeProcessor : BaseProcessorDb<ITypeSymbol, IArrayTypeSymbol>
    {
        public ArrayTypeProcessor(
            RoslynDbContext dbContext,
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override IEnumerable<IArrayTypeSymbol> ExtractSymbols(object item)
        {
            if (!(item is ITypeSymbol typeSymbol))
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if (typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol)
            {
                Logger.Error<string>("Unhandled ITypeSymbol '{0}'", typeSymbol.Name);
                yield break;
            }

            if (typeSymbol is IErrorTypeSymbol)
            {
                Logger.Error("ITypeSymbol is an IErrorTypeSymbol, ignored");
                yield break;
            }

            // we handle IArrayTypeSymbols, provided they aren't based on an ITypeParameterSymbol
            if (typeSymbol is IArrayTypeSymbol arraySymbol )
                yield return arraySymbol;
        }

        protected override bool ProcessSymbol( IArrayTypeSymbol symbol )
        {
            if (!RetrieveAssembly(symbol.ContainingAssembly, out var assemblyDb))
                return false;

            if (!RetrieveNamespace(symbol.ContainingNamespace, out var nsDb))
                return false;

            if( !EntityFactories.Retrieve<TypeDb>( symbol, out var dbSymbol, true ) )
            {
                Logger.Error<string>("Could not retrieve TypeDb entity for '{0}'",
                    EntityFactories.GetFullyQualifiedName(symbol));

                return false;
            }

            MarkSynchronized(dbSymbol!);

            dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
            dbSymbol.NamespaceID = nsDb!.SharpObjectID;

            return true;
        }
    }
}
