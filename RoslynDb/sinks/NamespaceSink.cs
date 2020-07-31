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
    public class NamespaceSink : RoslynDbSink<INamespaceSymbol, Namespace>
    {
        private readonly ISymbolSink<IAssemblySymbol, Assembly> _assemblySink;

        public NamespaceSink(
            RoslynDbContext dbContext,
            ISymbolSink<IAssemblySymbol, Assembly> assemblySink,
            ISymbolName symbolName,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
        {
            _assemblySink = assemblySink;
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
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

        public override bool TryGetSunkValue(INamespaceSymbol symbol, out Namespace? result)
        {
            var symbolName = SymbolName.GetFullyQualifiedName(symbol);

            var retVal = DbContext.Namespaces.FirstOrDefault(a => a.FullyQualifiedName == symbolName);

            if (retVal == null)
            {
                result = null;
                return false;
            }

            result = retVal;

            return true;
        }

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, INamespaceSymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if (retVal.AlreadyProcessed)
                return retVal;

            if( !_assemblySink.TryGetSunkValue( symbol.ContainingAssembly, out var dbAssembly ) )
                return retVal;

            var dbSymbol = DbContext.Namespaces.FirstOrDefault( a => a.FullyQualifiedName == retVal.SymbolName );

            bool isNew = dbSymbol == null;

            dbSymbol ??= new Namespace() { FullyQualifiedName = retVal.SymbolName };

            if( isNew )
            {
                DbContext.Namespaces.Add( dbSymbol );

                // need to add linking entries
                DbContext.AssemblyNamespaces.Add( new AssemblyNamespace()
                {
                    AssemblyID = dbAssembly!.ID,
                    Namespace = dbSymbol
                } );
            }

            dbSymbol.Synchronized = true;
            dbSymbol.Name = symbol.Name;

            DbContext.SaveChanges();

            retVal.WasOutput = true;

            return retVal;
        }
    }
}
