using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public abstract class RoslynDbSink<TSymbol> : SymbolSink<TSymbol>
        where TSymbol : class, ISymbol
    {
        private readonly ISymbolProcessors<TSymbol>? _processors;

        protected RoslynDbSink(
            UniqueSymbols<TSymbol> uniqueSymbols,
            IJ4JLogger logger,
            ISymbolProcessors<TSymbol>? processors = null
        )
            : base( logger )
        {
            Symbols = uniqueSymbols;

            _processors = processors;

            if (_processors == null)
                Logger.Error("No {0} processors defined for symbol {1}",
                    typeof(ISymbolProcessors<TSymbol>),
                    typeof(TSymbol));
        }

        protected UniqueSymbols<TSymbol> Symbols { get; }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker, bool stopOnFirstError = false )
        {
            if( !base.InitializeSink( syntaxWalker, stopOnFirstError ) )
                return false;

            Symbols.Clear();

            return true;
        }

        public override bool FinalizeSink(ISyntaxWalker syntaxWalker)
        {
            if (!base.FinalizeSink(syntaxWalker))
                return false;

            return _processors?.Process(Symbols, StopOnFirstError) ?? true;
        }

        public override bool OutputSymbol(ISyntaxWalker syntaxWalker, TSymbol symbol)
        {
            if (!base.OutputSymbol(syntaxWalker, symbol))
                return false;

            Symbols.Add( symbol );

            return true;
        }
    }
}