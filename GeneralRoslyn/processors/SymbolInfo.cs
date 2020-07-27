using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolInfo
    {
        private bool _wasOutput;

        public SymbolInfo( ISymbol symbol, ISymbolName symbolName )
        {
            OriginalSymbol = symbol;

            TypeKind = symbol switch
            {
                INamedTypeSymbol ntSymbol => ntSymbol.TypeKind,
                IArrayTypeSymbol arSymbol => TypeKind.Array,
                IDynamicTypeSymbol dynSymbol => TypeKind.Dynamic,
                IPointerTypeSymbol ptrSymbol => TypeKind.Pointer,
                _ => TypeKind.Error
            };

            Symbol = TypeKind == TypeKind.Array ? ( (IArrayTypeSymbol) symbol ).ElementType : symbol;

            SymbolName = symbolName.GetSymbolName( Symbol );
        }

        public bool AlreadyProcessed { get; set; }

        public bool WasOutput
        {
            get => _wasOutput || AlreadyProcessed;
            set => _wasOutput = value;
        }

        public ISymbol OriginalSymbol { get; }
        public ISymbol Symbol { get; }
        public string SymbolName { get; }
        public TypeKind TypeKind { get; }
    }
}