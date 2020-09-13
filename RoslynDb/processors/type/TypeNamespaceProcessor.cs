using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeNamespaceProcessor : BaseProcessorDb<ITypeSymbol, INamespaceSymbol>
    {
        public TypeNamespaceProcessor(
            EntityFactories factories,
            IJ4JLogger logger
        )
            : base( factories, logger )
        {
        }

        protected override IEnumerable<INamespaceSymbol> ExtractSymbols( ISymbol item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error( "Supplied item is not an ITypeSymbol" );
                yield break;
            }

            if( typeSymbol.ContainingNamespace == null )
            {
                Logger.Information<string>( "ITypeSymbol '{0}' does not have a ContainingNamespace", typeSymbol.Name );
                yield break;
            }

            // ignore any namespaces already on file
            if( !EntityFactories.Get<NamespaceDb>( typeSymbol.ContainingNamespace, out _ ) )
                yield return typeSymbol.ContainingNamespace!;
        }

        protected override bool ProcessSymbol(INamespaceSymbol symbol)
        {
            if (!EntityFactories.Get<AssemblyDb>(symbol.ContainingAssembly, out var assemblyDb))
                return false;

            if (!EntityFactories.Create<NamespaceDb>(symbol, out var nsDb))
                return false;

            EntityFactories.MarkSynchronized(nsDb!);

            var m2mDb = EntityFactories.DbContext.AssemblyNamespaces
                .FirstOrDefault(x => x.AssemblyID == assemblyDb!.SharpObjectID && x.NamespaceID == nsDb!.SharpObjectID);

            if (m2mDb != null)
                return true;

            m2mDb = new AssemblyNamespaceDb
            {
                Assembly = assemblyDb!,
                Namespace = nsDb!
            };

            EntityFactories.DbContext.AssemblyNamespaces.Add(m2mDb);

            return true;
        }
    }
}
