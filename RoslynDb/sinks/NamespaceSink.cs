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
    public class NamespaceSink : RoslynDbSink<INamespaceSymbol, Namespace>
    {
        public NamespaceSink(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger )
            : base( dbContext, symbolInfo, logger )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            MarkUnsynchronized<Namespace>();
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
                var symbolInfo = SymbolInfo.Create( symbol );

                if( !GetByFullyQualifiedName<Assembly>( symbolInfo.ContainingAssembly, out var dbAssembly ) )
                {
                    allOkay = false;
                    continue;
                }

                if( !GetByFullyQualifiedName<Namespace>( symbol, out var dbSymbol ) )
                    dbSymbol = AddEntity( symbolInfo.SymbolName );

                // create the link between this namespace entity and the assembly entity to which it belongs
                var assemblyNamespaces = GetDbSet<AssemblyNamespace>();

                var anDb = assemblyNamespaces
                    .FirstOrDefault( x => x.AssemblyID == dbAssembly!.ID && x.NamespaceID == dbSymbol!.ID );

                if( anDb == null )
                {
                    anDb = new AssemblyNamespace()
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
