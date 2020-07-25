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
    public class AssemblySink : RoslynDbSink<IAssemblySymbol>
    {
        public AssemblySink(
            RoslynDbContext dbContext,
            ISymbolName symbolName,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
        {
        }

        public override bool InitializeSink()
        {
            if( !base.InitializeSink() )
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

        protected override (OutputResult status, string symbolName) OutputSymbolInternal( IAssemblySymbol symbol )
        {
            var (status, symbolName) = base.OutputSymbolInternal( symbol );

            if( status != OutputResult.Succeeded )
                return ( status, symbolName );

            var dbSymbol = DbContext.Assemblies.FirstOrDefault( a => a.FullyQualifiedName == symbolName );

            bool isNew = dbSymbol == null;

            dbSymbol ??= new Assembly { FullyQualifiedName = symbolName };

            if( isNew )
                DbContext.Assemblies.Add( dbSymbol );

            dbSymbol.Synchronized = true;
            dbSymbol.Name = symbol.Name;
            dbSymbol.DotNetVersion = symbol.Identity.Version;

            DbContext.SaveChanges();

            return ( status, symbolName );
        }
    }
}
