using J4JSoftware.Logging;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public abstract class PostProcessDbSink<TSymbol, TSink> : RoslynDbSink<TSymbol, TSink>
        where TSymbol : class, ISymbol
        where TSink : class, ISharpObject, new()
    {
        private readonly ISymbolProcessors<TSymbol>? _processors;

        protected PostProcessDbSink( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger,
            ISymbolProcessors<TSymbol>? processors = null
            ) 
            : base( dbContext, symbolNamer, sharpObjMapper, logger )
        {
            _processors = processors;

            if( _processors == null )
                Logger.Error( "No {0} processors defined for symbol {1}", 
                    typeof(ISymbolProcessors<TSymbol>),
                    typeof(TSymbol) );
        }

        public override bool FinalizeSink(ISyntaxWalker syntaxWalker)
        {
            if (!base.FinalizeSink(syntaxWalker))
                return false;

            return _processors?.Process(Symbols) ?? true;
        }
    }
}