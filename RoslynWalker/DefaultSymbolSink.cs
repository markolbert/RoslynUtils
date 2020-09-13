using System;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class DefaultSymbolSink : IDefaultSymbolSink
    {
        private readonly IJ4JLogger _logger;

        public DefaultSymbolSink( IJ4JLogger logger )
        {
            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public bool StopOnFirstError { get; private set; } = false;

        // even though we support all ISymbols we deny it because we don't want
        // to be selected before a non-default sink can be selected
        public bool SupportsSymbol( Type symbolType ) => false;

        public bool InitializeSink( ISyntaxWalker syntaxWalker, bool stopOnFirstError = false )
        {
            StopOnFirstError = stopOnFirstError;

            return true;
        }

        public bool FinalizeSink( ISyntaxWalker syntaxWalker ) => true;

        public bool OutputSymbol( ISyntaxWalker syntaxWalker, ISymbol symbol )
        {
            _logger.Information<string>("Processed a {0}", symbol.ToDisplayString());

            return true;
        }
    }
}