using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceProcessor : BaseProcessorDb<INamespaceSymbol, INamespaceSymbol>
    {
        public NamespaceProcessor(
            EntityFactories factories,
            IJ4JLogger logger
        )
            : base( factories, logger )
        {
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
            if ( !EntityFactories.Get<AssemblyDb>( symbol.ContainingAssembly, out var assemblyDb ) )
                return false;

            if ( !EntityFactories.Create<NamespaceDb>( symbol, out var nsDb ) )
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

            EntityFactories.DbContext.SaveChanges();

            return true;
        }
    }
}
