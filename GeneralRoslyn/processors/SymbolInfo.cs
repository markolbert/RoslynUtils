using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolInfo
    {
        private bool _wasOutput;

        public SymbolInfo( ISymbol symbol, ISymbolName symbolName )
        {
            OriginalSymbol = symbol;

            // these assignments get overridden in certain cases
            Symbol = symbol;
            SymbolName = symbolName.GetFullyQualifiedName( symbol );

            switch ( symbol )
            {
                case INamedTypeSymbol ntSymbol:
                    TypeKind = ntSymbol.TypeKind;
                    break;

                case IArrayTypeSymbol arraySymbol:
                    TypeKind = TypeKind.Array;
                    Symbol = arraySymbol.ElementType;
                    SymbolName = symbolName.GetFullyQualifiedName(Symbol);
                    break;

                case IDynamicTypeSymbol dynSymbol:
                    TypeKind = TypeKind.Dynamic;
                    break;

                case IPointerTypeSymbol ptrSymbol:
                    TypeKind = TypeKind.Pointer;
                    break;

                case ITypeParameterSymbol typeParamSymbol:
                    TypeKind = TypeKind.TypeParameter;
                    Symbol = typeParamSymbol.DeclaringType ?? typeParamSymbol.DeclaringMethod!.ContainingType;
                    Method = typeParamSymbol.DeclaringMethod;
                    break;
            }
        }

        public bool AlreadyProcessed { get; set; }

        public bool WasOutput
        {
            get => _wasOutput || AlreadyProcessed;
            set => _wasOutput = value;
        }

        public ISymbol OriginalSymbol { get; }
        public IMethodSymbol? Method { get; }
        public ISymbol Symbol { get; }
        public string SymbolName { get; }
        public TypeKind TypeKind { get; } = TypeKind.Error;
    }
}