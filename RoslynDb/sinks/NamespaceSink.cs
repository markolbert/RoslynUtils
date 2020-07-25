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
            ISymbolName symbolName,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
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

        protected override (OutputResult status, string symbolName) OutputSymbolInternal( INamespaceSymbol symbol )
        {
            var (status, symbolName) = base.OutputSymbolInternal(symbol);

            if (status != OutputResult.Succeeded)
                return (status, symbolName);

            var assemblyName = SymbolName.GetSymbolName( symbol.ContainingAssembly );

            var dbSymbol = DbContext.Namespaces.FirstOrDefault( a => a.FullyQualifiedName == symbolName );
            var dbAssembly = DbContext.Assemblies.FirstOrDefault( a => a.FullyQualifiedName == assemblyName );

            if( dbAssembly == null )
            {
                Logger.Error<string, string>( "Could not find Assembly entity '{0}' referenced by namespace '{1}'",
                    assemblyName, 
                    symbolName );

                return ( OutputResult.Failed, symbolName );
            }

            bool isNew = dbSymbol == null;

            dbSymbol ??= new Namespace() { FullyQualifiedName = symbolName };

            if( isNew )
            {
                DbContext.Namespaces.Add( dbSymbol );

                // need to add linking entries
                DbContext.AssemblyNamespaces.Add( new AssemblyNamespace()
                {
                    Assembly = dbAssembly,
                    Namespace = dbSymbol
                } );
            }

            dbSymbol.Synchronized = true;
            dbSymbol.Name = symbol.Name;

            DbContext.SaveChanges();

            return (status, symbolName);
        }
    }
}
