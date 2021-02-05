using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public abstract class RoslynDbSink<TSymbol> : SymbolSink<TSymbol>
        where TSymbol : class, ISymbol
    {
        protected readonly List<IAction<TSymbol>>? _processors;

        protected RoslynDbSink(
            UniqueSymbols<TSymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger logger,
            IEnumerable<IAction<TSymbol>>? processors = null
        )
            : base( context, logger )
        {
            Symbols = uniqueSymbols;

            _processors = processors?.ToList();

            if( _processors == null )
                Logger.Error( "No {0} processors defined for symbol {1}",
                    typeof(IAction<TSymbol>),
                    typeof(TSymbol) );
        }

        protected UniqueSymbols<TSymbol> Symbols { get; }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.InitializeSink( syntaxWalker ) )
                return false;

            Symbols.Clear();

            return true;
        }

        public override bool FinalizeSink(ISyntaxWalker syntaxWalker)
        {
            if (!base.FinalizeSink(syntaxWalker))
                return false;

            if( _processors == null )
            {
                Logger.Error<Type>("No processors defined for {0}", this.GetType()  );
                return false;
            }

            var allOkay = true;

            foreach( var processor in _processors )
            {
                allOkay &= processor.Process( Symbols );

                if( !allOkay && Context.StopOnFirstError )
                    break;
            }

            return allOkay;
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