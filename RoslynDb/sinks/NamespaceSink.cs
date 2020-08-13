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
        private readonly List<INamespaceSymbol> _symbols = new List<INamespaceSymbol>();

        public NamespaceSink(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger )
            : base( dbContext, symbolInfo, logger )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            _symbols.Clear();

            MarkUnsynchronized<Namespace>();
            SaveChanges();

            return true;
        }

        public override bool FinalizeSink(ISyntaxWalker syntaxWalker)
        {
            if (!base.FinalizeSink(syntaxWalker))
                return false;

            var allOkay = true;

            foreach( var symbol in _symbols.Distinct( Comparer ) )
            {
                var symbolInfo = SymbolInfo.Create( symbol );

                if( !GetByFullyQualifiedName<Assembly>( symbol.ContainingAssembly, out var dbAssembly ) )
                {
                    allOkay = false;
                    continue;
                }

                if( !GetByFullyQualifiedName<Namespace>( symbol, out var dbSymbol ) )
                    dbSymbol = AddEntity( symbolInfo.SymbolName );

                // create the link between this namespace entity and the assembly entity to which it belongs
                var assemblyNamespaces = GetDbSet<AssemblyNamespace>();

                var anDb = assemblyNamespaces
                    .FirstOrDefault( x => x.AssemblyID == dbAssembly!.ID && x.NamespaceID == dbSymbol!.ID );

                if( anDb == null )
                {
                    anDb = new AssemblyNamespace()
                    {
                        AssemblyID = dbAssembly!.ID,
                        Namespace = dbSymbol!
                    };

                    assemblyNamespaces.Add( anDb );
                }

                dbSymbol!.Synchronized = true;
                dbSymbol.Name = symbol.Name;
            }

            SaveChanges();

            return allOkay;
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, INamespaceSymbol symbol )
        {
            if (!base.OutputSymbol(syntaxWalker, symbol))
                return false;

            _symbols.Add(symbol);

            return true;

            //var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            //if (retVal.AlreadyProcessed)
            //    return retVal;

            //if( !GetByFullyQualifiedName<Assembly>( symbol.ContainingAssembly, out var dbAssembly ) )
            //    return retVal;

            //if( !GetByFullyQualifiedName<Namespace>( symbol, out var dbSymbol ) )
            //    dbSymbol = AddEntity( retVal.SymbolName );

            //// create the link between this namespace entity and the assembly entity to which it belongs
            //var anDbSet = GetDbSet<AssemblyNamespace>();

            //var anDb = anDbSet.FirstOrDefault( x => x.AssemblyID == dbAssembly!.ID && x.NamespaceID == dbSymbol!.ID );

            //if( anDb == null )
            //{
            //    anDb = new AssemblyNamespace()
            //    {
            //        AssemblyID = dbAssembly!.ID,
            //        Namespace = dbSymbol!
            //    };

            //    anDbSet.Add( anDb );
            //}

            //dbSymbol!.Synchronized = true;
            //dbSymbol.Name = symbol.Name;

            //SaveChanges();

            //retVal.WasOutput = true;

            //return retVal;
        }
    }
}
