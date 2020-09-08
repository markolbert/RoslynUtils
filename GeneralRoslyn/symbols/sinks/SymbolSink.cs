using System;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolSink<TSymbol, TSink> : ISymbolSink<TSymbol, TSink>
        where TSymbol : ISymbol
        where TSink : class
    {
        protected SymbolSink(
            ISymbolNamer symbolNamer,
            IJ4JLogger logger
            )
        {
            SymbolNamer = symbolNamer;

            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }
        protected ISymbolNamer SymbolNamer { get; }

        public bool StopOnFirstError { get; private set; } = false;

        public virtual bool OutputSymbol( ISyntaxWalker syntaxWalker, TSymbol symbol ) => true;

        public virtual bool SupportsSymbol( Type symbolType ) => typeof(TSymbol) == symbolType;

        public virtual bool InitializeSink( ISyntaxWalker syntaxWalker, bool stopOnFirstError = false )
        {
            StopOnFirstError = stopOnFirstError;

            return true;
        }

        public virtual bool FinalizeSink( ISyntaxWalker syntaxWalker ) => true;

        bool ISymbolSink.OutputSymbol( ISyntaxWalker syntaxWalker, ISymbol symbol )
        {
            if( symbol is TSymbol castSymbol )
                return OutputSymbol( syntaxWalker, castSymbol );

            Logger.Error<string, Type>( "{0} is not a {1}", nameof(symbol), typeof(TSymbol) );

            return false;
        }
    }
}
