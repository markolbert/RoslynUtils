using System;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class DefaultSymbolSink : IDefaultSymbolSink
    {
        private readonly IJ4JLogger _logger;

        public DefaultSymbolSink( 
            WalkerContext context,
            IJ4JLogger logger 
            )
        {
            ExecutionContext = context;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public WalkerContext ExecutionContext { get; }

        // even though we support all ISymbols we deny it because we don't want
        // to be selected before a non-default sink can be selected
        public bool SupportsSymbol( Type symbolType ) => false;

        public bool InitializeSink( ISyntaxWalker syntaxWalker ) => true;

        public bool FinalizeSink( ISyntaxWalker syntaxWalker ) => true;

        public bool OutputSymbol( ISyntaxWalker syntaxWalker, ISymbol symbol )
        {
            _logger.Information<string>("Processed a {0}", symbol.ToDisplayString());

            return true;
        }
    }
}