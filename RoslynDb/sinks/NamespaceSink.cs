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
        private readonly ISymbolProcessors<INamespaceSymbol> _processors;

        public NamespaceSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            ISymbolProcessors<INamespaceSymbol> processors,
            IJ4JLogger logger )
            : base( dbContext, symbolNamer, docObjMapper, logger )
        {
            _processors = processors;
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
            return base.FinalizeSink(syntaxWalker) && _processors.Process(Symbols);
            //if (!base.FinalizeSink(syntaxWalker))
            //    return false;

            //var allOkay = true;

            //foreach( var symbol in Symbols )
            //{
            //    if( !GetByFullyQualifiedNameNG<AssemblyDb>( symbol.ContainingAssembly, out var dbAssembly ) )
            //    {
            //        allOkay = false;
            //        continue;
            //    }

            //    if( !GetByFullyQualifiedNameNG<NamespaceDb>( symbol, out var dbSymbol, true ) )
            //    {
            //        allOkay = false;
            //        continue;
            //    }

            //    // create the link between this namespace entity and the assembly entity to which it belongs
            //    var assemblyNamespaces = GetDbSet<AssemblyNamespaceDb>();

            //    var anDb = assemblyNamespaces
            //        .FirstOrDefault( x => x.AssemblyID == dbAssembly!.DocObjectID && x.NamespaceID == dbSymbol!.DocObjectID );

            //    if( anDb == null )
            //    {
            //        anDb = new AssemblyNamespaceDb()
            //        {
            //            AssemblyID = dbAssembly!.DocObjectID,
            //            Namespace = dbSymbol!
            //        };

            //        assemblyNamespaces.Add( anDb );
            //    }

            //    dbSymbol!.Synchronized = true;
            //    dbSymbol.Name = symbol.Name;
            //}

            //SaveChanges();

            //return allOkay;
        }
    }
}
