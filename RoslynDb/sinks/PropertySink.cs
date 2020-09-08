using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class PropertySink : RoslynDbSink<IPropertySymbol, PropertyDb>
    {
        public PropertySink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger,
            ISymbolProcessors<IPropertySymbol>? processors = null )
            : base( dbContext, symbolNamer, sharpObjMapper, logger, processors)
        {
        }
    }
}
