using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class PropertySink : RoslynDbSink<IPropertySymbol, PropertyDb>
    {
        private readonly ISymbolSetProcessor<IPropertySymbol> _processors;

        public PropertySink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISymbolSetProcessor<IPropertySymbol> processors,
            IJ4JLogger logger )
            : base( dbContext, symbolNamer, logger )
        {
            _processors = processors;
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

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            return base.FinalizeSink(syntaxWalker) && _processors.Process(Symbols);
        }
    }
}
