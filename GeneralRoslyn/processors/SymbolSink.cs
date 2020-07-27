using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolSink<TSymbol, TSink> : ISymbolSink<TSymbol, TSink>
        where TSymbol : ISymbol
        where TSink : class
    {
        protected SymbolSink(
            ISymbolName symbolName,
            IJ4JLogger logger
            )
        {
            SymbolName = symbolName;

            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }
        protected ISymbolName SymbolName { get; }
        protected List<string> ProcessedSymbolNames { get; } = new List<string>();

        public bool OutputSymbol( ISyntaxWalker syntaxWalker, TSymbol symbol ) =>
            OutputSymbolInternal( syntaxWalker, symbol ).WasOutput;

        public virtual bool TryGetSunkValue( TSymbol symbol, out TSink? result )
        {
            result = null;
            return false;
        }

        protected virtual SymbolInfo OutputSymbolInternal( 
            ISyntaxWalker syntaxWalker,
            TSymbol symbol )
        {
            var retVal = new SymbolInfo( symbol, SymbolName );

            retVal.AlreadyProcessed =
                ProcessedSymbolNames.Exists( pn => pn.Equals( retVal.SymbolName, StringComparison.Ordinal ) );

            return retVal;
        }

        public virtual bool SupportsSymbol( Type symbolType ) => typeof(TSymbol) == symbolType;

        public virtual bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            ProcessedSymbolNames.Clear();

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
