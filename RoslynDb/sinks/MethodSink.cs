using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class MethodSink : RoslynDbSink<IMethodSymbol, MethodDb>
    {
        private readonly ISymbolProcessors<IMethodSymbol> _processors;

        public MethodSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            ISymbolProcessors<IMethodSymbol> processors,
            IJ4JLogger logger )
            : base( dbContext, symbolNamer, docObjMapper, logger )
        {
            _processors = processors;
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            MarkUnsynchronized<MethodDb>();
            MarkUnsynchronized<ArgumentDb>();
            MarkUnsynchronized<MethodParametricTypeDb>();

            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            return base.FinalizeSink( syntaxWalker ) && _processors.Process(Symbols);
        }
    }
}
