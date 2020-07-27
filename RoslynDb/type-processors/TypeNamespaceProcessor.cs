using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeAssemblyProcessor))]
    public class TypeNamespaceProcessor : BaseProcessor<INamespaceSymbol, List<ITypeSymbol>>, ITypeProcessor
    {
        private readonly ISymbolSink<INamespaceSymbol, Namespace> _nsSink;

        public TypeNamespaceProcessor(
            RoslynDbContext dbContext,
            ISymbolSink<INamespaceSymbol, Namespace> nsSink,
            IJ4JLogger logger
        )
            : base( dbContext, logger )
        {
            _nsSink = nsSink;
        }

        public override bool Process( ISyntaxWalker syntaxWalker, List<ITypeSymbol> inputData )
        {
            var allOkay = true;

            foreach( var nsSymbol in inputData.Select( ts => ts.ContainingNamespace ) )
            {
                allOkay &= _nsSink.OutputSymbol( syntaxWalker, nsSymbol );
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
