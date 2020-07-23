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
    public class NamespaceSink : RoslynDbSink<INamespaceSymbol>
    {
        public NamespaceSink(
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
            foreach( var ns in DbContext.Namespaces )
            {
                ns.Synchronized = false;
            }

            DbContext.SaveChanges();

            return true;
        }

        public override bool OutputSymbol( INamespaceSymbol symbol )
        {
            var symbolName = SymbolNamers.GetSymbolName( symbol );
            var assemblyName = SymbolNamers.GetSymbolName( symbol.ContainingAssembly );

            var dbNS = DbContext.Namespaces.FirstOrDefault( a => a.FullyQualifiedName == symbolName );
            var dbAssembly = DbContext.Assemblies.FirstOrDefault( a => a.FullyQualifiedName == assemblyName );

            if( dbAssembly == null )
            {
                Logger.Error<string, string>( "Could not find Assembly entity '{0}' referenced by namespace '{1}'",
                    assemblyName, 
                    symbolName );

                return false;
            }

            bool isNew = dbNS == null;

            dbNS ??= new Namespace() { FullyQualifiedName = symbolName };

            if( isNew )
            {
                DbContext.Namespaces.Add( dbNS );

                // need to add linking entries
                DbContext.AssemblyNamespaces.Add( new AssemblyNamespace()
                {
                    Assembly = dbAssembly,
                    Namespace = dbNS
                } );
            }

            dbNS.Name = SymbolNamers.GetSimpleName( symbol );

            DbContext.SaveChanges();

            return true;
        }
    }
}
