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
            ISymbolName symbolName,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.InitializeSink( syntaxWalker ) )
                return false;

            // mark all the existing assemblies as unsynchronized since we're starting
            // the synchonrization process
            foreach( var assembly in DbContext.Assemblies )
            {
                assembly.Synchronized = false;
            }

            DbContext.SaveChanges();

            return true;
        }

        public override bool TryGetSunkValue( IAssemblySymbol symbol, out Assembly? result )
        {
            var symbolName = SymbolName.GetSymbolName( symbol );
            
            var retVal = DbContext.Assemblies.FirstOrDefault( a => a.FullyQualifiedName == symbolName );

            if( retVal == null )
            {
                result = null;
                return false;
            }

            result = retVal;

            return true;
        }

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, IAssemblySymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if( retVal.AlreadyProcessed )
                return retVal;

            var dbSymbol = DbContext.Assemblies.FirstOrDefault( a => a.FullyQualifiedName == retVal.SymbolName );

            bool isNew = dbSymbol == null;

            dbSymbol ??= new Assembly { FullyQualifiedName = retVal.SymbolName };

            if( isNew )
                DbContext.Assemblies.Add( dbSymbol );

            dbSymbol.Synchronized = true;
            dbSymbol.Name = symbol.Name;
            dbSymbol.DotNetVersion = symbol.Identity.Version;

            DbContext.SaveChanges();

            retVal.WasOutput = true;

            return retVal;
        }
    }
}
