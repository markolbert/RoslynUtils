using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class FieldSink : RoslynDbSink<IFieldSymbol, FieldDb>
    {
        public FieldSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger,
            ISymbolProcessors<IFieldSymbol>? processors = null )
            : base( dbContext, symbolNamer, sharpObjMapper, logger, processors)
        {
        }
    }
}
