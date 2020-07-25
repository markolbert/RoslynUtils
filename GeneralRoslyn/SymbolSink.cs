using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolSink<TSymbol> : ISymbolSink<TSymbol>
        where TSymbol : ISymbol
    {
        protected enum OutputResult
        {
            Succeeded,
            Failed,
            AlreadyProcessed
        }

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

        public bool OutputSymbol( TSymbol symbol ) =>
            OutputSymbolInternal( symbol ).status switch
            {
                OutputResult.Failed => false,
                _ => true
            };

        protected virtual (OutputResult status, string symbolName) OutputSymbolInternal( TSymbol symbol )
        {
            var symbolName = SymbolName.GetSymbolName( symbol );

            if( ProcessedSymbolNames.Exists( pn => pn.Equals( symbolName, StringComparison.Ordinal ) ) )
                return ( OutputResult.AlreadyProcessed, symbolName );

            return ( OutputResult.Succeeded, symbolName );
        }

        public virtual bool SupportsSymbol( Type symbolType ) => typeof(TSymbol) == symbolType;

        public virtual bool InitializeSink()
        {
            ProcessedSymbolNames.Clear();

            return true;
        }

        public virtual bool FinalizeSink() => true;

        bool ISymbolSink.OutputSymbol( ISymbol symbol )
        {
            if( symbol is TSymbol castSymbol )
                return OutputSymbol( castSymbol );

            Logger.Error<string, Type>( "{0} is not a {1}", nameof(symbol), typeof(TSymbol) );

            return false;
        }
    }
}
