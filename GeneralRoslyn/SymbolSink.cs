using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SymbolSink<TSymbol> : ISymbolSink<TSymbol>
        where TSymbol : class, ISymbol
    {
        protected SymbolSink( IJ4JLogger logger )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public abstract bool OutputSymbol( TSymbol symbol );

        public virtual bool SupportsSymbol( Type symbolType ) => typeof(ISymbol).IsAssignableFrom( symbolType );

        public virtual bool InitializeSink() => true;

        public bool FinalizeSink() => true;

        bool ISymbolSink.OutputSymbol( ISymbol symbol )
        {
            if( symbol is TSymbol castSymbol )
                return OutputSymbol( castSymbol );

            Logger.Error<string, Type>( "{0} is not a {1}", nameof(symbol), typeof(TSymbol) );

            return false;
        }
    }
}
