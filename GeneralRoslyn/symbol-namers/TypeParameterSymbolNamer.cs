using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeParameterSymbolNamer : SymbolNamer
    {
        public TypeParameterSymbolNamer( IJ4JLogger logger )
            : base( SymbolDisplayFormat.MinimallyQualifiedFormat, logger )
        {
            AddSupportedType<ITypeParameterSymbol>();
        }

        public override string GetSymbolName<TSymbol>( TSymbol symbol )
        {
            if( symbol is ITypeParameterSymbol tpSymbol )
            {
                return tpSymbol.TypeParameterKind switch
                {
                    TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringMethod.ToDisplayString( Format )}::{SymbolNamers.GetSimpleName( tpSymbol )}",
                    TypeParameterKind.Type => tpSymbol.DeclaringType == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringType.ToDisplayString( Format )}::{SymbolNamers.GetSimpleName( tpSymbol )}",
                    _ => string.Empty
                };
            }

            return symbol.ToDisplayString( Format );
        }
    }
}