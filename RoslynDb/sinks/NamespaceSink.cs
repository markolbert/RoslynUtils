﻿using System;
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
            MarkUnsynchronized<Namespace>();
            SaveChanges();

            return true;
        }

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, INamespaceSymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if (retVal.AlreadyProcessed)
                return retVal;

            if( !_assemblySink.TryGetSunkValue( symbol.ContainingAssembly, out var dbAssembly ) )
                return retVal;

            if( !GetByFullyQualifiedName( retVal.SymbolName, out var dbSymbol ) )
                dbSymbol = AddEntity( retVal.SymbolName );

            // create the link between this namespace entity and the assembly entity to which it belongs
            var anDbSet = GetDbSet<AssemblyNamespace>();

            var anDb = anDbSet.FirstOrDefault( x => x.AssemblyID == dbAssembly!.ID && x.NamespaceID == dbSymbol!.ID );

            if( anDb == null )
            {
                anDb = new AssemblyNamespace()
                {
                    AssemblyID = dbAssembly!.ID,
                    Namespace = dbSymbol!
                };

                anDbSet.Add( anDb );
            }

            dbSymbol!.Synchronized = true;
            dbSymbol.Name = symbol.Name;

            SaveChanges();

            retVal.WasOutput = true;

            return retVal;
        }
    }
}
