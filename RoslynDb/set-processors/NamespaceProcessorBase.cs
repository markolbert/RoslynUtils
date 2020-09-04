using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class NamespaceProcessorBase<TSource> : BaseProcessorDb<TSource, INamespaceSymbol>
        where TSource : class, ISymbol
    {
        protected NamespaceProcessorBase(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, docObjMapper, logger )
        {
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol )
        {
            if( !GetByFullyQualifiedName<AssemblyDb>( symbol.ContainingAssembly, out var dbAssembly ) )
                return false;

            if( !GetByFullyQualifiedName<NamespaceDb>( symbol, out var dbNS, true ) )
                return false;

            // create the link between this namespace entity and the assembly entity to which it belongs
            var assemblyNamespaces = GetDbSet<AssemblyNamespaceDb>();

            var anDb = assemblyNamespaces
                .FirstOrDefault(x => x.AssemblyID == dbAssembly!.DocObjectID && x.NamespaceID == dbNS!.DocObjectID);

            if (anDb == null)
            {
                anDb = new AssemblyNamespaceDb()
                {
                    AssemblyID = dbAssembly!.DocObjectID,
                    Namespace = dbNS!
                };

                assemblyNamespaces.Add(anDb);
            }

            dbNS!.Synchronized = true;
            dbNS.Name = symbol.Name;

            return true;
        }
    }
}
