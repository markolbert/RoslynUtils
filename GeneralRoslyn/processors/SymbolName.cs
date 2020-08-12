using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
    public class SymbolName : ISymbolName
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

        public string GetFullyQualifiedName( ISymbol symbol )
        {
            return symbol switch
            {
                ITypeParameterSymbol tpSymbol => tpSymbol.TypeParameterKind switch
                    {
                        TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                            ? string.Empty
                            : $"{tpSymbol.DeclaringMethod.ToDisplayString( FullyQualifiedFormat )}::{tpSymbol.Name}",
                        TypeParameterKind.Type => tpSymbol.DeclaringType == null
                            ? string.Empty
                            : $"{tpSymbol.DeclaringType.ToDisplayString( FullyQualifiedFormat )}::{tpSymbol.Name}",
                        _ => string.Empty
                    },
                _ => symbol.ToDisplayString( FullyQualifiedFormat )
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
}
