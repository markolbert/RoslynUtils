using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.RoslynWalker
{
    public class RoslynWalkerTestBase
    {
        [ Theory ]
        [ InlineData( "C:\\Programming\\RoslynUtils\\RoslynNetStandardTestLib\\RoslynNetStandardTestLib.csproj" ) ]
        public async void WalkerTest( string projFilePath )
        {
            var ws = ServiceProvider.Instance.GetRequiredService<DocumentationWorkspace>();

            ws.AddProject( projFilePath ).Should().BeTrue();

            var result = await ws.Compile();
            result.Should().NotBeNull();

            result!.Count.Should().BeGreaterThan( 0 );

            var walker = ServiceProvider.Instance.GetRequiredService<ISyntaxWalkerNG>();
            walker.Process( result );

            var parsedTypes = new TypeInfoCollection();
            parsedTypes.ParseFile( projFilePath, out _ ).Should().BeTrue();

            CompareRoslynNamedTypesToParsed( walker, parsedTypes );
            CompareParsedToRoslynNamedTypes( walker, parsedTypes );
        }

        private void CompareRoslynNamedTypesToParsed(ISyntaxWalkerNG walker, TypeInfoCollection parsedTypes )
        {
            var rosylynTypes = walker.NodeCollectors.FirstOrDefault( x => x.SymbolType == typeof(ITypeSymbol) )?
                                   .Cast<ITypeSymbol>()
                                   .ToList()
                               ?? new List<ITypeSymbol>();

            foreach( var roslynType in rosylynTypes )
            {
                var ntSymbol = roslynType as INamedTypeSymbol;
                ntSymbol.Should().NotBeNull();

                var namedTypeInfo = parsedTypes
                    .Where( x =>
                    {
                        if( !x.Name.Equals( roslynType.Name, StringComparison.Ordinal ) )
                            return false;

                        if( x is not ICodeElementTypeArguments typeArgsInfo )
                            return false;

                        return !ntSymbol!.TypeArguments
                            .Where( ( t, idx ) => !ntSymbol.TypeArguments[ idx ].Name
                                .Equals( typeArgsInfo.TypeArguments[ idx ], StringComparison.Ordinal ) )
                            .Any();
                    } )
                    .FirstOrDefault();

                namedTypeInfo.Should().NotBeNull();

                if( namedTypeInfo is not InterfaceInfo interfaceInfo ) 
                    continue;

                CompareElements<IMethodSymbol, MethodInfo>( ntSymbol!, interfaceInfo.Methods );
                CompareElements<IEventSymbol, EventInfo>( ntSymbol!, interfaceInfo.Events );
                CompareElements<IPropertySymbol, PropertyInfo>( ntSymbol!, interfaceInfo.Properties );

                if( interfaceInfo is ClassInfo classInfo )
                    CompareElements<IFieldSymbol, FieldInfo>( ntSymbol!, classInfo.Fields );
            }
        }

        private void CompareParsedToRoslynNamedTypes(ISyntaxWalkerNG walker, TypeInfoCollection parsedTypes )
        {
            var rosylynTypes = walker.NodeCollectors.FirstOrDefault(x => x.SymbolType == typeof(ITypeSymbol))?
                                   .Cast<ITypeSymbol>()
                                   .ToList()
                               ?? new List<ITypeSymbol>();

            foreach ( var parsedType in parsedTypes )
            {
                var roslynType = rosylynTypes
                    .Where( x =>
                    {
                        if( !x.Name.Equals( parsedType.Name, StringComparison.Ordinal ) )
                            return false;

                        if( x is not INamedTypeSymbol ntSymbol || parsedType is not ICodeElementTypeArguments typeArgsInfo ) 
                            return true;

                        return !ntSymbol.TypeArguments
                            .Where( ( t, idx ) =>
                                !t.Name.Equals( typeArgsInfo.TypeArguments[ idx ], StringComparison.Ordinal ) )
                            .Any();

                    } )
                    .Cast<INamedTypeSymbol>()
                    .FirstOrDefault();

                roslynType.Should().NotBeNull();
            }
        }

        private void CompareElements<TSymbol, TInfo>( INamedTypeSymbol ntSymbol, List<TInfo> infoCollection )
            where TSymbol : ISymbol
            where TInfo : ICodeElement
        {
            var symbols = ntSymbol.GetMembers().Where( x => x is TSymbol ).Cast<TSymbol>().ToList();

            foreach( var symbol in symbols )
            {
                var infoItem =
                    infoCollection.FirstOrDefault( x => x.Name.Equals( symbol.Name, StringComparison.Ordinal ) );

                infoItem.Should().NotBeNull();

                if( !typeof(IMethodSymbol).IsAssignableFrom( typeof(TSymbol) ) )
                    continue;

                if( infoItem is ICodeElementTypeArguments typeArgsInfo )
                    CompareTypeArguments( ( (IMethodSymbol) symbol ).TypeArguments.ToList(), typeArgsInfo );
                else
                    throw new ArgumentException(
                        $"{nameof(infoItem)} is not a {nameof(ICodeElementTypeArguments)} but should be" );
            }

            foreach( var infoItem in infoCollection )
            {
                var symbol = symbols.FirstOrDefault( x => x.Name.Equals( infoItem.Name ) );

                symbol.Should().NotBeNull();

                if( infoItem is ICodeElementTypeArguments typeArgsInfo )
                {
                    if( symbol is IMethodSymbol methodSymbol )
                        CompareTypeArguments( methodSymbol.TypeArguments.ToList(), typeArgsInfo );
                    else
                        throw new ArgumentException(
                            $"{nameof(symbol)} should implement {nameof(IMethodSymbol)} but doesn't" );
                }
            }
        }

        private void CompareTypeArguments( List<ITypeSymbol> symbols, ICodeElementTypeArguments typeArgsInfo )
        {
            for( var idx = 0; idx < symbols.Count; idx++ )
            {
                symbols[ idx ].Name.Should().Be( typeArgsInfo.TypeArguments[ idx ] );
            }
        }
    }
}
