using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolNamer : ISymbolNamer
    {
        public SymbolDisplayFormat FullyQualifiedFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
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

        public SymbolDisplayFormat GenericTypeFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes)
            .RemoveGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters);

        public SymbolDisplayFormat SimpleNameFormat { get; } = SymbolDisplayFormat.MinimallyQualifiedFormat;

        //public SymbolInfo Create(ISymbol symbol) => new SymbolInfo(symbol, this);

        public string GetFullyQualifiedName(ISymbol symbol)
        {
            return symbol switch
            {
                ITypeParameterSymbol tpSymbol => GetParametricTypeName(tpSymbol),
                INamedTypeSymbol ntSymbol => GetNamedTypeName(ntSymbol),
                IArrayTypeSymbol arraySymbol => GetArrayTypeName(arraySymbol),
                _ => symbol.ToDisplayString(FullyQualifiedFormat)
            };
        }

        public string GetName(ISymbol symbol)
        {
            return symbol switch
            {
                ITypeParameterSymbol tpSymbol => tpSymbol.TypeParameterKind switch
                {
                    TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringMethod.ToDisplayString(SimpleNameFormat)}::{tpSymbol.Name}",
                    TypeParameterKind.Type => tpSymbol.DeclaringType == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringType.ToDisplayString(SimpleNameFormat)}::{tpSymbol.Name}",
                    _ => string.Empty
                },
                _ => symbol.ToDisplayString(SimpleNameFormat)
            };
        }

        private string GetNamedTypeName( INamedTypeSymbol symbol )
        {
            if( symbol.IsGenericType)
                return $"{symbol.ToDisplayString(GenericTypeFormat)}<{symbol.TypeParameters.Length}>";

            return symbol.ToDisplayString(FullyQualifiedFormat);
        }

        private string GetArrayTypeName( IArrayTypeSymbol symbol )
        {
            var elementFQN = symbol.ElementType switch
            {
                INamedTypeSymbol ntSymbol => GetNamedTypeName( ntSymbol ),
                ITypeParameterSymbol tpSymbol => GetParametricTypeName( tpSymbol ),
                _ => symbol.ElementType.ToDisplayString( FullyQualifiedFormat )
            };

            return $"{elementFQN}[{symbol.Rank}]";
        }

        private string GetParametricTypeName( ITypeParameterSymbol symbol )
        {
            switch( symbol.TypeParameterKind )
            {
                case TypeParameterKind.Method:
                    return
                        $"{symbol.DeclaringMethod!.ToDisplayString( FullyQualifiedFormat )}::{symbol.Name}";

                case TypeParameterKind.Type:
                    return
                        $"{symbol.DeclaringType!.ToDisplayString( FullyQualifiedFormat )}::{symbol.Name}";

                default:
                    return symbol.ToDisplayString( FullyQualifiedFormat );
            }
        }

        //private string GetTypeName( ITypeSymbol typeSymbol )
        //{
        //    if( typeSymbol is INamedTypeSymbol ntSymbol && ntSymbol.IsGenericType )
        //        return $"{ntSymbol.ToDisplayString( GenericTypeFormat )}<{ntSymbol.TypeParameters.Length}>";

        //    var retVal = typeSymbol.ToDisplayString( FullyQualifiedFormat );

        //    if( typeSymbol is IArrayTypeSymbol arraySymbol )
        //        retVal = retVal.Replace( "[]", $"[{arraySymbol.Rank}]" );

        //    return retVal;
        //}
    }
}