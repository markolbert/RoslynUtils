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
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol )
        {
            if( !GetByFullyQualifiedName<IAssemblySymbol, AssemblyDb>( symbol.ContainingAssembly, out var dbAssembly ) )
                return false;

            if( !GetByFullyQualifiedName<INamespaceSymbol, NamespaceDb>( symbol, out var dbNS, true ) )
                return false;

            // create the link between this namespace entity and the assembly entity to which it belongs
            var assemblyNamespaces = GetDbSet<AssemblyNamespaceDb>();

            var anDb = assemblyNamespaces
                .FirstOrDefault(x => x.AssemblyID == dbAssembly!.SharpObjectID && x.NamespaceID == dbNS!.SharpObjectID);

            if( anDb != null ) 
                return true;
            
            anDb = new AssemblyNamespaceDb()
            {
                AssemblyID = dbAssembly!.SharpObjectID,
                Namespace = dbNS!
            };

            assemblyNamespaces.Add(anDb);

            return true;
        }
    }
}
