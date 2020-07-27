using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAssemblyProcessor : BaseProcessor<Assembly, List<ITypeSymbol>>, ITypeProcessor
    {
        private readonly ISymbolSink<IAssemblySymbol, Assembly> _assemblySink;

        public TypeAssemblyProcessor(
            RoslynDbContext dbContext,
            ISymbolSink<IAssemblySymbol, Assembly> assemblySink,
            IJ4JLogger logger
        )
            : base( dbContext, logger )
        {
            _assemblySink = assemblySink;
        }

        public override bool Process( ISyntaxWalker syntaxWalker, List<ITypeSymbol> inputData )
        {
            var allOkay = true;

            foreach( var assemblySymbol in inputData.Select( ts => ts.ContainingAssembly ) )
            {
                if( assemblySymbol == null )
                    continue;

                allOkay &= _assemblySink.OutputSymbol( syntaxWalker, assemblySymbol );
            }

            return allOkay;
        }

        public bool Equals( ITypeProcessor? other )
        {
            if (other == null)
                return false;

            return other.SupportedType == SupportedType;
        }
    }
}
