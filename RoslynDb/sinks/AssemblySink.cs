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
    public class AssemblySink : RoslynDbSink<IAssemblySymbol, Assembly>
    {
        public AssemblySink(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger )
            : base( dbContext, symbolInfo, logger )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.InitializeSink( syntaxWalker ) )
                return false;

            MarkUnsynchronized<Assembly>();
            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            foreach( var symbol in Symbols )
            {
                var symbolInfo = SymbolInfo.Create( symbol );

                if (!GetByFullyQualifiedName<Assembly>(symbol, out var dbSymbol))
                    dbSymbol = AddEntity(symbolInfo.SymbolName);

                dbSymbol!.Synchronized = true;
                dbSymbol.Name = symbol.Name;
                dbSymbol.DotNetVersion = symbol.Identity.Version;
            }

            SaveChanges();

            return true;
        }
    }
}
