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
            IJ4JLogger logger )
            : base( dbContext, symbolNamer, logger )
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

            foreach( var symbol in Symbols )
            {
                GetByFullyQualifiedName<AssemblyDb>( symbol, out var dbSymbol, true );

                dbSymbol!.Synchronized = true;
                dbSymbol.Name = symbol.Name;
                dbSymbol.DotNetVersion = symbol.Identity.Version;
            }

            SaveChanges();

            return true;
        }
    }
}
