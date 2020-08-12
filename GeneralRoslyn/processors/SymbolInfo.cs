using System;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolInfo
    {
        public static SymbolDisplayFormat FullyQualifiedFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
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

        public static SymbolDisplayFormat NameFormat { get; } = SymbolDisplayFormat.MinimallyQualifiedFormat;

        public class Factory : ISymbolInfo
        {
            public SymbolInfo Create( ISymbol symbol ) => new SymbolInfo( symbol, this );

            public string GetFullyQualifiedName( ISymbol symbol )
            {
                return symbol switch
                {
                    ITypeParameterSymbol tpSymbol => tpSymbol.TypeParameterKind switch
                    {
                        TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                            ? string.Empty
                            : $"{tpSymbol.DeclaringMethod.ToDisplayString(FullyQualifiedFormat)}::{tpSymbol.Name}",
                        TypeParameterKind.Type => tpSymbol.DeclaringType == null
                            ? string.Empty
                            : $"{tpSymbol.DeclaringType.ToDisplayString(FullyQualifiedFormat)}::{tpSymbol.Name}",
                        _ => string.Empty
                    },
                    _ => symbol.ToDisplayString(FullyQualifiedFormat)
                };
            }

            public string GetName( ISymbol symbol )
            {
                return symbol switch
                {
                    ITypeParameterSymbol tpSymbol => tpSymbol.TypeParameterKind switch
                    {
                        TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                            ? string.Empty
                            : $"{tpSymbol.DeclaringMethod.ToDisplayString(NameFormat)}::{tpSymbol.Name}",
                        TypeParameterKind.Type => tpSymbol.DeclaringType == null
                            ? string.Empty
                            : $"{tpSymbol.DeclaringType.ToDisplayString(NameFormat)}::{tpSymbol.Name}",
                        _ => string.Empty
                    },
                    _ => symbol.ToDisplayString(NameFormat)
                };
            }
        }

        private bool _wasOutput;

        private SymbolInfo( ISymbol symbol, Factory siFactory )
        {
            OriginalSymbol = symbol;

            // these assignments get overridden in certain cases
            Symbol = symbol;
            SymbolName = siFactory.GetFullyQualifiedName( symbol );

            switch ( symbol )
            {
                case INamedTypeSymbol ntSymbol:
                    TypeKind = ntSymbol.TypeKind;
                    break;

                case IArrayTypeSymbol arraySymbol:
                    TypeKind = TypeKind.Array;
                    Symbol = arraySymbol.ElementType;
                    SymbolName = siFactory.GetFullyQualifiedName(Symbol);
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