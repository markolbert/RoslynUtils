using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolNamer : ISymbolNamer
    {
        private readonly List<Type> _supported = new List<Type>();
        private readonly IJ4JLogger _logger;

        protected SymbolNamer( SymbolDisplayFormat format, IJ4JLogger logger )
        {
            Format = format;
            _logger = logger;
        }

        protected SymbolDisplayFormat Format { get; }

        protected void AddSupportedType<TSymbol>()
            where TSymbol : ISymbol
        {
            if( !_supported.Contains( typeof(TSymbol) ) )
                _supported.Add( typeof(TSymbol) );
        }

        public ReadOnlyCollection<Type> SupportedSymbolTypes => _supported.AsReadOnly();

        public virtual string GetSymbolName<TSymbol>( TSymbol symbol )
            where TSymbol : ISymbol
        {
            if( _supported.Contains( typeof(TSymbol) ) )
                return symbol.ToDisplayString( Format );

            throw new InvalidCastException(
                $"Symbol Type {typeof(TSymbol)} is not supported by {this.GetType()}, using default" );
        }
    }
}