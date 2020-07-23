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
            SymbolNamers symbolNamers,
            IJ4JLogger logger )
            : base( dbContext, symbolNamers, logger )
        {
        }

        public override bool InitializeSink()
        {
            // mark all the existing assemblies as unsynchronized since we're starting
            // the synchonrization process
            foreach( var assembly in DbContext.Assemblies )
            {
                assembly.Synchronized = false;
            }

            DbContext.SaveChanges();

            return true;
        }

        public override bool OutputSymbol( IAssemblySymbol symbol )
        {
            var symbolName = SymbolNamers.GetSymbolName( symbol );

            var dbAssembly = DbContext.Assemblies.FirstOrDefault( a => a.FullyQualifiedName == symbolName );

            bool isNew = dbAssembly == null;

            dbAssembly ??= new Assembly { FullyQualifiedName = symbolName };

            if( isNew )
                DbContext.Assemblies.Add( dbAssembly );

            dbAssembly.Name = SymbolNamers.GetSimpleName( symbol );
            dbAssembly.DotNetVersion = symbol.Identity.Version;

            DbContext.SaveChanges();

            return true;
        }
    }
}
