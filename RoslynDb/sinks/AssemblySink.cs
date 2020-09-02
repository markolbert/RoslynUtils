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
    public class AssemblySink : RoslynDbSink<IAssemblySymbol, AssemblyDb>
    {
        public AssemblySink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            IJ4JLogger logger )
            : base( dbContext, symbolNamer, docObjMapper, logger )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.InitializeSink( syntaxWalker ) )
                return false;

            MarkUnsynchronized<AssemblyDb>();
            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            var allOkay = true;

            foreach( var symbol in Symbols )
            {
                if( GetByFullyQualifiedNameNG<AssemblyDb>( symbol, out var dbSymbol, true ) )
                {
                    dbSymbol!.Synchronized = true;
                    dbSymbol.Name = symbol.Name;
                    dbSymbol.DotNetVersion = symbol.Identity.Version;
                }
                else allOkay = false;
            }

            SaveChanges();

            return allOkay;
        }
    }
}
