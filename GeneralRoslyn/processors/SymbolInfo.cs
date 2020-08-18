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

        public static SymbolDisplayFormat GenericTypeFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes )
            .RemoveGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters );

        public static SymbolDisplayFormat NameFormat { get; } = SymbolDisplayFormat.MinimallyQualifiedFormat;

        //private bool _wasOutput;

        internal SymbolInfo( ISymbol symbol, SymbolInfoFactory siFactory )
        {
            Symbol = symbol;
            SymbolName = siFactory.GetFullyQualifiedName( symbol );

            // these assignments may be overwritten below
            ContainingAssembly = symbol.ContainingAssembly;
            ContainingNamespace = symbol.ContainingNamespace;

            switch ( symbol )
            {
                case INamedTypeSymbol ntSymbol:
                    AssociatedSymbol = ntSymbol.BaseType;
                    TypeKind = ntSymbol.TypeKind;
                    break;

                case IArrayTypeSymbol arraySymbol:
                    TypeKind = TypeKind.Array;
                    ContainingAssembly = arraySymbol.ElementType.ContainingAssembly;
                    ContainingNamespace = arraySymbol.ElementType.ContainingNamespace;
                    AssociatedSymbol = arraySymbol.ElementType;
                    break;

                case IDynamicTypeSymbol dynSymbol:
                    AssociatedSymbol = dynSymbol.BaseType;
                    TypeKind = TypeKind.Dynamic;
                    break;

                case IPointerTypeSymbol ptrSymbol:
                    AssociatedSymbol = ptrSymbol.BaseType;
                    TypeKind = TypeKind.Pointer;
                    break;

                case ITypeParameterSymbol typeParamSymbol:
                    TypeKind = TypeKind.TypeParameter;
                    AssociatedSymbol = typeParamSymbol.DeclaringType ?? typeParamSymbol.DeclaringMethod!.ContainingType;
                    //Method = typeParamSymbol.DeclaringMethod;
                    break;
            }
        }

        //public bool AlreadyProcessed { get; set; }

        //public bool WasOutput
        //{
        //    get => _wasOutput || AlreadyProcessed;
        //    set => _wasOutput = value;
        //}

        public ISymbol Symbol { get; }
        public IAssemblySymbol ContainingAssembly { get; }
        public INamespaceSymbol ContainingNamespace { get; }
        //public IMethodSymbol? Method { get; }
        public ISymbol? AssociatedSymbol { get; }
        public string SymbolName { get; }
        public TypeKind TypeKind { get; } = TypeKind.Error;
    }
} 