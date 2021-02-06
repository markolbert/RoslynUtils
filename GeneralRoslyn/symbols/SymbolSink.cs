using System;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolSink<TSymbol> : ISymbolSink<TSymbol>
        where TSymbol : ISymbol
    {
        protected SymbolSink(
            ActionsContext context,
            IJ4JLogger? logger
            )
        {
            Context = context;

            Logger = logger;
            Logger?.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger? Logger { get; }
        protected ISyntaxWalker? SyntaxWalker { get; private set; } = null;
        protected ActionsContext Context { get; }

        public bool Initialized { get; private set; } = false;

        public virtual bool OutputSymbol( ISyntaxWalker syntaxWalker, TSymbol symbol ) => Initialized;

        public virtual bool SupportsSymbol( Type symbolType ) => typeof(TSymbol) == symbolType;

        public virtual bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            SyntaxWalker = syntaxWalker;

            Initialized = true;

            return true;
        }

        public virtual bool FinalizeSink( ISyntaxWalker syntaxWalker ) => Initialized;

        bool ISymbolSink.OutputSymbol( ISyntaxWalker syntaxWalker, ISymbol symbol )
        {
            if( symbol is TSymbol castSymbol )
                return OutputSymbol( syntaxWalker, castSymbol );

            Logger?.Error<string, Type>( "{0} is not a {1}", nameof(symbol), typeof(TSymbol) );

            return false;
        }
    }
}
