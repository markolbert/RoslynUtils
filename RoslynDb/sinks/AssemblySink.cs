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
            ISymbolInfo symbolInfo,
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

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, IAssemblySymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if( retVal.AlreadyProcessed )
                return retVal;

            if( !GetByFullyQualifiedName<Assembly>( symbol, out var dbSymbol ) )
                dbSymbol = AddEntity( retVal.SymbolName );

            dbSymbol!.Synchronized = true;
            dbSymbol.Name = symbol.Name;
            dbSymbol.DotNetVersion = symbol.Identity.Version;

            SaveChanges();

            retVal.WasOutput = true;

            return retVal;
        }
    }
}
