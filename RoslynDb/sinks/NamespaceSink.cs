using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public class NamespaceSink : RoslynDbSink<INamespaceSymbol, NamespaceDb>
    {
        public NamespaceSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IJ4JLogger logger )
            : base( dbContext, symbolNamer, logger )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            MarkUnsynchronized<NamespaceDb>();
            SaveChanges();

            return true;
        }

        public override bool FinalizeSink(ISyntaxWalker syntaxWalker)
        {
            if (!base.FinalizeSink(syntaxWalker))
                return false;

            var allOkay = true;

            foreach( var symbol in Symbols )
            {
                if( !GetByFullyQualifiedName<AssemblyDb>( symbol.ContainingAssembly, out var dbAssembly ) )
                {
                    allOkay = false;
                    continue;
                }

                GetByFullyQualifiedName<NamespaceDb>( symbol, out var dbSymbol, true );

                // create the link between this namespace entity and the assembly entity to which it belongs
                var assemblyNamespaces = GetDbSet<AssemblyNamespaceDb>();

                var anDb = assemblyNamespaces
                    .FirstOrDefault( x => x.AssemblyID == dbAssembly!.ID && x.NamespaceID == dbSymbol!.ID );

                if( anDb == null )
                {
                    anDb = new AssemblyNamespaceDb()
                    {
                        AssemblyID = dbAssembly!.ID,
                        Namespace = dbSymbol!
                    };

                    assemblyNamespaces.Add( anDb );
                }

                dbSymbol!.Synchronized = true;
                dbSymbol.Name = symbol.Name;
            }

            SaveChanges();

            return allOkay;
        }
    }
}
