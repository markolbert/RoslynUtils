using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public static class SymbolNameExtensions
    {
        #region Format specifications

        public static SymbolDisplayFormat BasicFormat { get; } = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.IncludeTypeParameters,
            SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeExplicitInterface |
            SymbolDisplayMemberOptions.IncludeParameters,
            SymbolDisplayDelegateStyle.NameAndParameters,
            SymbolDisplayExtensionMethodStyle.StaticMethod,
            SymbolDisplayParameterOptions.IncludeType,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.IncludeType,
            SymbolDisplayKindOptions.None );

        public static SymbolDisplayFormat UniqueGenericFormat { get; } = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.None,
            SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeExplicitInterface |
            SymbolDisplayMemberOptions.IncludeParameters,
            SymbolDisplayDelegateStyle.NameAndParameters,
            SymbolDisplayExtensionMethodStyle.StaticMethod,
            SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeType,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.IncludeType,
            SymbolDisplayKindOptions.None);

        public static SymbolDisplayFormat UniqueNameFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .WithGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters )
            .WithMemberOptions( SymbolDisplayMemberOptions.IncludeContainingType
                                | SymbolDisplayMemberOptions.IncludeExplicitInterface )
            .WithParameterOptions( SymbolDisplayParameterOptions.None )
            .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes );

        public static SymbolDisplayFormat FullNameFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .WithGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters )
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

        public static SymbolDisplayFormat GenericTypeFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes )
            .RemoveGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters );

        public static SymbolDisplayFormat SimpleNameFormat { get; } = SymbolDisplayFormat.MinimallyQualifiedFormat;

        #endregion

        public static string ToUniqueName( this IAssemblySymbol symbol ) =>
            symbol.ToDisplayString( BasicFormat );

        public static string ToUniqueName( this INamespaceSymbol symbol ) =>
            symbol.ToDisplayString( BasicFormat );

        public static string ToUniqueName( this IFieldSymbol symbol ) =>
            symbol.ToDisplayString( BasicFormat );

        public static string ToUniqueName(this IEventSymbol symbol) =>
            symbol.ToDisplayString(BasicFormat);

        public static string ToUniqueName( this INamedTypeSymbol symbol )
        {
            if( !symbol.IsGenericType )
                return symbol.ToDisplayString( BasicFormat );

            var sb = new StringBuilder( symbol.ToDisplayString( UniqueGenericFormat ) );

            sb.Append( "<" );

            for( var idx = 0; idx < symbol.TypeArguments.Length; idx++ )
            {
                if( idx > 0 )
                    sb.Append( ", " );

                sb.Append( symbol.TypeArguments[ idx ] switch
                {
                    INamedTypeSymbol ntSymbol => ntSymbol.ToUniqueName(),
                    ITypeParameterSymbol tpSymbol => tpSymbol.Name,
                    IArrayTypeSymbol arraySymbol => arraySymbol.ElementType switch
                    {
                        INamedTypeSymbol ntArray => $"{ntArray.ToUniqueName()}[{arraySymbol.Rank}]",
                        ITypeParameterSymbol tpArray => $"{tpArray.Name}[{arraySymbol.Rank}]",
                        _ => throw new ArgumentException(
                            $"ElementType of IArraySymbol '{arraySymbol.Name}' is neither an INamedTypeSymbol nor an ITypeParameterSymbol" )
                    },
                    _ => throw new ArgumentException(
                        $"Unhandled symbol type {symbol.TypeArguments[ idx ].ToFullName()}" )
                } );
            }

            sb.Append( ">" );

            return sb.ToString();
        }

        public static string ToUniqueName( this ITypeParameterSymbol symbol )
        {
            if( symbol.DeclaringType != null )
                return $"{symbol.DeclaringType.ToDisplayString( BasicFormat )}:{symbol.Name}";

            if( symbol.DeclaringMethod != null )
                return $"{symbol.DeclaringMethod.ToDisplayString( BasicFormat )}:{symbol.Name}";

            throw new ArgumentException(
                $"ITypeParameterSymbol '{symbol.Name}' is contained neither by an IMethodSymbol nor an INamedTypeSymbol" );
        }

        public static string ToUniqueName( this IArrayTypeSymbol symbol )
        {
            return symbol.ElementType switch
            {
                INamedTypeSymbol ntSymbol => $"{ntSymbol.ToUniqueName()}[{symbol.Rank}]",
                ITypeParameterSymbol tpSymbol => $"{tpSymbol.ToUniqueName()}[{symbol.Rank}]",
                _ => throw new ArgumentException(
                    $"ElementType of IArraySymbol '{symbol.Name}' is neither an INamedTypeSymbol nor an ITypeParameterSymbol" )
            };
        }

        public static string ToUniqueName( this IMethodSymbol symbol ) =>
            symbol.ToDisplayString( BasicFormat );

        public static string ToUniqueName( this IPropertySymbol symbol ) =>
            symbol.ToDisplayString( BasicFormat );

        public static string ToUniqueName(this IParameterSymbol symbol) =>
            $"{symbol.ToDisplayString(BasicFormat)}:{symbol.Name}";

        public static string ToFullName( this ISymbol symbol )
        {
            if( !( symbol is ITypeParameterSymbol tpSymbol ) )
                return symbol.ToDisplayString( FullNameFormat );

            ISymbol? declaringSymbol = null;

            if( tpSymbol.DeclaringType != null )
                declaringSymbol = tpSymbol.DeclaringType;

            if( tpSymbol.DeclaringMethod != null )
                declaringSymbol = tpSymbol.DeclaringMethod;

            return declaringSymbol != null
                ? $"{declaringSymbol.ToDisplayString( FullNameFormat )}:{tpSymbol.ToDisplayString( FullNameFormat )}"
                : tpSymbol.ToDisplayString( FullNameFormat );
        }

        public static string ToSimpleName( this ISymbol symbol )
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

        public static string GetUniqueName( this ISymbol symbol )
        {
            var retVal = symbol switch
            {
                IAssemblySymbol aSymbol => aSymbol.ToUniqueName(),
                INamespaceSymbol nsSymbol => nsSymbol.ToUniqueName(),
                IEventSymbol eventSymbol => eventSymbol.ToUniqueName(),
                IFieldSymbol fieldSymbol => fieldSymbol.ToUniqueName(),
                INamedTypeSymbol ntSymbol => ntSymbol.ToUniqueName(),
                ITypeParameterSymbol tpSymbol => tpSymbol.ToUniqueName(),
                IArrayTypeSymbol arraySymbol => arraySymbol.ToUniqueName(),
                IMethodSymbol methodSymbol => methodSymbol.ToUniqueName(),
                IPropertySymbol propSymbol => propSymbol.ToUniqueName(),
                IParameterSymbol paramSymbol => paramSymbol.ToUniqueName(),
                _ => throw new ArgumentException(
                    $"Unsupported ISymbol type '{symbol.GetType()}'" )
            };

            return retVal;
        }
    }
}
