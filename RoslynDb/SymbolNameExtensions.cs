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

        private static SymbolDisplayFormat BasicFormat { get; } = new SymbolDisplayFormat(
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

        private static SymbolDisplayFormat UniqueGenericFormat { get; } = new SymbolDisplayFormat(
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

        private static SymbolDisplayFormat FullNameFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
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

        private static SymbolDisplayFormat SimpleNameFormat { get; } = SymbolDisplayFormat.MinimallyQualifiedFormat;

        #endregion

        public static string ToUniqueName( this ISymbol symbol )
        {
            //if( symbol is IMethodSymbol xSymbol
            //    && xSymbol.Name.IndexOf( "UseNamed", StringComparison.Ordinal ) >= 0 )
            //    System.Diagnostics.Debugger.Break();

            var retVal = symbol switch
            {
                ITypeSymbol typeSymbol => GetTypeUniqueName( typeSymbol ),
                IMethodSymbol methodSymbol => GetMethodUniqueName( methodSymbol ),
                IPropertySymbol propSymbol => GetPropertyUniqueName( propSymbol ),
                IFieldSymbol fieldSymbol => GetFieldUniqueName( fieldSymbol ),
                IParameterSymbol paramSymbol => GetParameterUniqueName( paramSymbol ),
                _ => symbol.ToDisplayString( BasicFormat )
            };

            //if( retVal.IndexOf( "(System", StringComparison.Ordinal ) >= 0 )
            //    System.Diagnostics.Debugger.Break();

            return retVal;
        }

        public static string ToFullName( this ISymbol symbol )
        {
            return symbol switch
            {
                ITypeParameterSymbol tpSymbol => GetTypeParameterFullName( tpSymbol ),
                _ => symbol.ToDisplayString( FullNameFormat )
            };
        }

        public static string ToSimpleName( this ISymbol symbol )
        {
            return symbol switch
            {
                ITypeParameterSymbol tpSymbol => GetTypeParameterSimpleName(tpSymbol),
                _ => symbol.ToDisplayString(SimpleNameFormat)
            };
        }

        private static string GetTypeUniqueName( ITypeSymbol symbol )
        {
            return symbol switch
            {
                IArrayTypeSymbol arraySymbol => GetArrayUniqueName( arraySymbol ),
                INamedTypeSymbol ntSymbol => GetNamedTypeUniqueName( ntSymbol ),
                ITypeParameterSymbol tpSymbol => GetTypeParameterUniqueName( tpSymbol ),
                _ => symbol.ToDisplayString( BasicFormat )
            };
        }

        private static string GetArrayUniqueName( IArrayTypeSymbol symbol )
        {
            return symbol.ElementType switch
            {
                INamedTypeSymbol ntSymbol => $"{GetNamedTypeUniqueName( ntSymbol )}[{symbol.Rank}]",
                ITypeParameterSymbol tpSymbol => $"{GetTypeParameterUniqueName( tpSymbol )}[{symbol.Rank}]",
                _ => throw new ArgumentException(
                    $"ElementType of IArraySymbol '{symbol.Name}' is neither an INamedTypeSymbol nor an ITypeParameterSymbol" )
            };
        }

        private static string GetNamedTypeUniqueName( INamedTypeSymbol symbol )
        {
            //if( symbol.IsTupleType )
            //    throw new ArgumentException( $"Trying to get a unique non-tuple name from a tuple type" );

            if( !symbol.IsGenericType )
                return symbol.ToDisplayString( BasicFormat );

            var sb = new StringBuilder();

            if( symbol.IsTupleType )
            {
                // this convoluted approach is necessary for tuple types because the default
                // symbol formatting routines insist on ignoring the ValueTuple name
                sb.Append( symbol.ContainingNamespace.ToDisplayString( BasicFormat ) );

                var containingTypeNames = new List<string>();
                
                var curNTSymbol = symbol;

                do
                {
                    containingTypeNames.Add( curNTSymbol.Name );
                    curNTSymbol = curNTSymbol.ContainingType;
                } while( curNTSymbol != null );

                containingTypeNames.Reverse();

                foreach( var ctName in containingTypeNames )
                {
                    sb.Append( $".{ctName}" );
                }
            }
            else sb.Append( symbol.ToDisplayString( UniqueGenericFormat ) );

            sb.Append( "<" );

            for( var idx = 0; idx < symbol.TypeArguments.Length; idx++ )
            {
                if( idx > 0 )
                    sb.Append( ", " );

                sb.Append( symbol.TypeArguments[ idx ] switch
                {
                    INamedTypeSymbol ntSymbol => GetNamedTypeUniqueName( ntSymbol ),
                    ITypeParameterSymbol tpSymbol => tpSymbol.Name,
                    IArrayTypeSymbol arraySymbol => arraySymbol.ElementType switch
                    {
                        INamedTypeSymbol ntArray => $"{GetNamedTypeUniqueName( ntArray )}[{arraySymbol.Rank}]",
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

        private static string GetTypeParameterUniqueName(ITypeParameterSymbol symbol)
        {
            if( symbol.DeclaringType != null )
                return $"{GetNamedTypeUniqueName(symbol.DeclaringType)}:{symbol.Name}";

            if (symbol.DeclaringMethod != null)
                return $"{symbol.DeclaringMethod.ToDisplayString(BasicFormat)}:{symbol.Name}";

            throw new ArgumentException(
                $"ITypeParameterSymbol '{symbol.Name}' is contained neither by an IMethodSymbol nor an INamedTypeSymbol");
        }

        private static string GetMethodUniqueName( IMethodSymbol symbol )
        {
            var sb = new StringBuilder( GetNamedTypeUniqueName( symbol.ContainingType ) );

            sb.Append( $"{symbol.Name}(" );

            for( var idx = 0; idx < symbol.Parameters.Length; idx++ )
            {
                if( idx > 0 )
                    sb.Append( ", " );

                sb.Append( GetTypeUniqueName( symbol.Parameters[ idx ].Type ) );
            }

            sb.Append( ")" );

            return sb.ToString();
        }

        private static string GetPropertyUniqueName(IPropertySymbol symbol)
        {
            var sb = new StringBuilder($"{GetNamedTypeUniqueName(symbol.ContainingType)}:");

            sb.Append( symbol.Name );

            if( symbol.Parameters.Length > 0 )
                sb.Append("(");

            for (var idx = 0; idx < symbol.Parameters.Length; idx++)
            {
                if (idx > 0)
                    sb.Append(", ");

                sb.Append(GetTypeUniqueName(symbol.Parameters[idx].Type));
            }

            if (symbol.Parameters.Length > 0)
                sb.Append(")");

            return sb.ToString();
        }

        private static string GetParameterUniqueName(IParameterSymbol symbol)
        {
            var sb = new StringBuilder();

            switch( symbol.ContainingSymbol )
            {
                case IMethodSymbol methodSymbol:
                    sb.Append( GetMethodUniqueName( methodSymbol ) );
                    break;

                case IPropertySymbol propSymbol:
                    sb.Append( GetPropertyUniqueName( propSymbol ) );
                    break;

                default:
                    throw new ArgumentException(
                        $"IParameterSymbol is not contained by an IMethodSymbol nor an IPropertySymbol ({symbol.ContainingSymbol.Kind})" );
            }

            sb.Append( $":{GetTypeUniqueName( symbol.Type )}" );

            return sb.ToString();
        }

        private static string GetFieldUniqueName(IFieldSymbol symbol)
        {
            return $"{GetNamedTypeUniqueName(symbol.ContainingType)}:{symbol.Name}";
        }

        private static string GetTypeParameterFullName( ITypeParameterSymbol symbol )
        {
            if( symbol.DeclaringType != null )
                return
                    $"{symbol.DeclaringType.ToDisplayString( FullNameFormat )}:{symbol.ToDisplayString( FullNameFormat )}";

            if (symbol.DeclaringMethod != null)
                return
                    $"{symbol.DeclaringMethod.ToDisplayString(FullNameFormat)}:{symbol.ToDisplayString(FullNameFormat)}";

            return symbol.ToDisplayString( FullNameFormat );
        }

        private static string GetTypeParameterSimpleName(ITypeParameterSymbol symbol)
        {
            if (symbol.DeclaringType != null)
                return
                    $"{symbol.DeclaringType.ToDisplayString(SimpleNameFormat)}:{symbol.Name}";
            if (symbol.DeclaringMethod != null)
                return
                    $"{symbol.DeclaringMethod.ToDisplayString(SimpleNameFormat)}:{symbol.Name}";

            return symbol.ToDisplayString(FullNameFormat);
        }
    }
}
