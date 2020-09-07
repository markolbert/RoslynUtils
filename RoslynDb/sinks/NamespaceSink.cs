using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class NamespaceSink : PostProcessDbSink<INamespaceSymbol, NamespaceDb>
    {
        public NamespaceSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger,
            ISymbolProcessors<INamespaceSymbol>? processors = null )
            : base( dbContext, symbolNamer, sharpObjMapper, logger, processors )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            MarkUnsynchronized<NamespaceDb>();
            SaveChanges();

            return true;
        }
    }
}
