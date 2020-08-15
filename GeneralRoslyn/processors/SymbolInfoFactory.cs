using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolInfoFactory : ISymbolInfoFactory
    {
        public SymbolInfo Create(ISymbol symbol) => new SymbolInfo(symbol, this);

        public string GetFullyQualifiedName(ISymbol symbol)
        {
            return symbol switch
            {
                ITypeParameterSymbol tpSymbol => tpSymbol.TypeParameterKind switch
                {
                    TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringMethod.ToDisplayString(SymbolInfo.FullyQualifiedFormat)}::{tpSymbol.Name}",
                    TypeParameterKind.Type => tpSymbol.DeclaringType == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringType.ToDisplayString(SymbolInfo.FullyQualifiedFormat)}::{tpSymbol.Name}",
                    _ => string.Empty
                },
                ITypeSymbol typeSymbol => GetTypeName(typeSymbol),
                _ => symbol.ToDisplayString(SymbolInfo.FullyQualifiedFormat)
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
                        : $"{tpSymbol.DeclaringMethod.ToDisplayString(SymbolInfo.NameFormat)}::{tpSymbol.Name}",
                    TypeParameterKind.Type => tpSymbol.DeclaringType == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringType.ToDisplayString(SymbolInfo.NameFormat)}::{tpSymbol.Name}",
                    _ => string.Empty
                },
                _ => symbol.ToDisplayString(SymbolInfo.NameFormat)
            };
        }

        private string GetTypeName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol ntSymbol && ntSymbol.IsGenericType)
                return $"{ntSymbol.ToDisplayString(SymbolInfo.GenericTypeFormat)}<{ntSymbol.TypeParameters.Length}>";

            var retVal = typeSymbol.ToDisplayString(SymbolInfo.FullyQualifiedFormat);

            if( typeSymbol is IArrayTypeSymbol arraySymbol )
                retVal = $"{retVal}[{arraySymbol.Rank}]";

            return retVal;
        }
    }
}