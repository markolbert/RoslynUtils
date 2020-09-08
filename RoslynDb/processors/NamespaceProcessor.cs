using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceProcessor : BaseProcessorDb<INamespaceSymbol, INamespaceSymbol>
    {
        public NamespaceProcessor(
            IEntityFactories factories,
            IJ4JLogger logger
        )
            : base( factories, logger )
        {
        }

        protected override bool InitializeProcessor( IEnumerable<INamespaceSymbol> inputData )
        {
            if( !base.InitializeProcessor( inputData ) )
                return false;

            EntityFactories.MarkUnsynchronized<NamespaceDb>( true );

            return true;
        }

        protected override IEnumerable<INamespaceSymbol> ExtractSymbols( ISymbol item )
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

            EntityFactories.MarkSynchronized( nsDb! );

            var m2mDb = EntityFactories.DbContext.AssemblyNamespaces
                .FirstOrDefault( x => x.AssemblyID == assemblyDb!.SharpObjectID && x.NamespaceID == nsDb!.SharpObjectID );

            if( m2mDb != null )
                return true;

            m2mDb = new AssemblyNamespaceDb
            {
                Assembly = assemblyDb!,
                Namespace = nsDb!
            };

            EntityFactories.DbContext.AssemblyNamespaces.Add( m2mDb );

            return true;
        }
    }
}
