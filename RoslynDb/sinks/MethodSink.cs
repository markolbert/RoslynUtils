using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class MethodSink : PostProcessDbSink<IMethodSymbol, MethodDb>
    {
        public MethodSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger,
            ISymbolProcessors<IMethodSymbol>? processors = null )
            : base( dbContext, symbolNamer, sharpObjMapper, logger, processors )
        {
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
    }
}
