using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceProcessor : BaseProcessorDb<INamespaceSymbol, INamespaceSymbol>
    {
        public NamespaceProcessor(
            RoslynDbContext dbContext,
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override IEnumerable<INamespaceSymbol> ExtractSymbols( object item )
        {
            if( !( item is INamespaceSymbol nsSymbol ) )
            {
                Logger.Error( "Supplied item is not an INamespaceSymbol" );
                yield break;
            }

            yield return nsSymbol!;
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol )
        {
            if( !RetrieveAssembly( symbol.ContainingAssembly, out var assemblyDb ) )
                return false;

            if ( !EntityFactories.Retrieve<NamespaceDb>( symbol, out var nsDb, true ) )
                return false;

            MarkSynchronized( nsDb! );

            var m2mDb = DbContext.AssemblyNamespaces
                .FirstOrDefault( x => x.AssemblyID == assemblyDb!.SharpObjectID && x.NamespaceID == nsDb!.SharpObjectID );

            if( m2mDb != null )
                return true;

            m2mDb = new AssemblyNamespaceDb
            {
                Assembly = assemblyDb!,
                Namespace = nsDb!
            };

            DbContext.AssemblyNamespaces.Add( m2mDb );

            return true;
        }
    }
}
