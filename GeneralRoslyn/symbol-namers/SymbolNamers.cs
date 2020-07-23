using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolNamers
    {
        public static SymbolDisplayFormat DefaultFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
            .WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType
                               | SymbolDisplayMemberOptions.IncludeExplicitInterface
                               | SymbolDisplayMemberOptions.IncludeParameters)
            .WithParameterOptions(SymbolDisplayParameterOptions.IncludeExtensionThis
                                  | SymbolDisplayParameterOptions.IncludeName
                                  | SymbolDisplayParameterOptions.IncludeParamsRefOut
                                  | SymbolDisplayParameterOptions.IncludeDefaultValue
                                  | SymbolDisplayParameterOptions.IncludeOptionalBrackets
                                  | SymbolDisplayParameterOptions.IncludeType)
            .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        public static string GetSimpleName( ISymbol symbol )
        {
            // for some reason IArrayTypeSymbols have a blank Name but a non-blank display string...
            if( symbol is IArrayTypeSymbol arraySymbol )
                return symbol.ToDisplayString();

            return symbol.Name;
        }

        private readonly Dictionary<Type, ISymbolNamer> _symbolNamers = new Dictionary<Type, ISymbolNamer>();
        private readonly IJ4JLogger _logger;

        public SymbolNamers( 
            IEnumerable<ISymbolNamer> symbolNamers,
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType( this.GetType() );

            foreach( var symbolNamer in symbolNamers )
            {
                foreach( var symbolType in symbolNamer.SupportedSymbolTypes )
                {
                    if( _symbolNamers.ContainsKey( symbolType ) )
                    {
                        _logger.Error<Type, Type>( "Duplicate {0} for Symbol Type {1}", 
                            typeof(ISymbolNamer),
                            symbolType );

                        _symbolNamers[ symbolType ] = symbolNamer;
                    }
                    else _symbolNamers.Add( symbolType, symbolNamer );
                }
            }
        }

        public string GetSymbolName<TSymbol>( TSymbol symbol )
            where TSymbol : ISymbol
        {
            var symbolType = typeof(TSymbol);

            if( _symbolNamers.ContainsKey( symbolType ) )
                return _symbolNamers[ symbolType ].GetSymbolName( symbol );

            _logger.Error<Type, Type>("No {0} defined for Symbol Type {1}, using default",
                typeof(ISymbolNamer),
                symbolType);

            return symbol.ToDisplayString( DefaultFormat );
        }
    }
}