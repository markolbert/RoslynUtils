using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AssemblyProcessor : AssemblyProcessorBase<IAssemblySymbol>
    {
        public AssemblyProcessor( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            IJ4JLogger logger ) 
            : base( dbContext, symbolNamer, docObjMapper, logger )
        {
        }

        protected override IEnumerable<IAssemblySymbol> ExtractSymbols( object item )
        {
            if (!(item is IAssemblySymbol assemblySymbol ))
            {
                Logger.Error("Supplied item is not an IAssemblySymbol");
                yield break;
            }

            yield return assemblySymbol!;
        }
    }
}
