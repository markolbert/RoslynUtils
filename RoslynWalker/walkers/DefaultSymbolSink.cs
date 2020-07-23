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

        public bool SupportsSymbol( Type symbolType ) => typeof(ISymbol).IsAssignableFrom( symbolType );

        public bool InitializeSink() => true;
        public bool FinalizeSink() => true;

        public bool OutputSymbol( ISymbol symbol )
        {
            _logger.Information<string>("Processed a {0}", symbol.ToDisplayString());

            return true;
        }
    }
}