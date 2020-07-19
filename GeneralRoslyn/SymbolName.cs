using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolName : ISymbolName
    {
        public SymbolName()
        {
            AssemblyBasedFormat = SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
                .WithGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters
                    /*| SymbolDisplayGenericsOptions.IncludeVariance*/ )
                .WithMemberOptions( SymbolDisplayMemberOptions.IncludeContainingType
                    | SymbolDisplayMemberOptions.IncludeExplicitInterface
                    | SymbolDisplayMemberOptions.IncludeParameters )
                .WithParameterOptions( SymbolDisplayParameterOptions.IncludeExtensionThis
                    | SymbolDisplayParameterOptions.IncludeName
                    | SymbolDisplayParameterOptions.IncludeParamsRefOut
                    | SymbolDisplayParameterOptions.IncludeDefaultValue
                    | SymbolDisplayParameterOptions.IncludeOptionalBrackets
                    | SymbolDisplayParameterOptions.IncludeType )
                .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes );

            TypeLocalFormat = SymbolDisplayFormat.MinimallyQualifiedFormat;
        }
        
        public SymbolDisplayFormat AssemblyBasedFormat { get; }
        public SymbolDisplayFormat TypeLocalFormat { get; }

        public string ToAssemblyBasedName( ISymbol symbol )
        {
            if( symbol == null )
                return string.Empty;

            // ITypeParameterSymbols need to be handled differently because they're always
            // local to their immediate context
            if( !( symbol is ITypeParameterSymbol tpSymbol ) ) 
                return symbol.ToDisplayString( AssemblyBasedFormat );

            return tpSymbol.TypeParameterKind switch
            {
                TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                    ? string.Empty
                    : $"{tpSymbol.DeclaringMethod.ToDisplayString( AssemblyBasedFormat )}::{ToShortName( tpSymbol )}",
                TypeParameterKind.Type => tpSymbol.DeclaringType == null
                    ? string.Empty
                    : $"{tpSymbol.DeclaringType.ToDisplayString( AssemblyBasedFormat )}::{ToShortName( tpSymbol )}",
                _ => string.Empty
            };
        }

        public string ToTypeLocalName( ISymbol symbol )
        {
            if( symbol == null )
                return string.Empty;

            // ITypeParameterSymbols need to be handled differently because they're always
            // local to their immediate context
            if( !( symbol is ITypeParameterSymbol tpSymbol ) )
                return symbol.ToDisplayString( TypeLocalFormat );

            return tpSymbol.TypeParameterKind switch
            {
                TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                    ? string.Empty
                    : $"{tpSymbol.DeclaringMethod.ToDisplayString( TypeLocalFormat )}::{ToShortName( tpSymbol )}",
                TypeParameterKind.Type => tpSymbol.DeclaringType == null
                    ? string.Empty
                    : $"{tpSymbol.DeclaringType.ToDisplayString( TypeLocalFormat )}::{ToShortName( tpSymbol )}",
                _ => string.Empty
            };
        }

        public string ToShortName( ISymbol symbol )
        {
            if( symbol == null )
                return string.Empty;

            // for some reason IArrayTypeSymbols have a blank Name but a non-blank display string...
            if( symbol is IArrayTypeSymbol arraySymbol )
                return symbol.ToDisplayString();

            return symbol.Name;
        }
    }
}