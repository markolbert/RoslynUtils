using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class PropertySink : PostProcessDbSink<IPropertySymbol, PropertyDb>
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

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            MarkUnsynchronized<PropertyDb>();
            MarkUnsynchronized<PropertyParameterDb>();

            SaveChanges();

            return true;
        }
    }
}
