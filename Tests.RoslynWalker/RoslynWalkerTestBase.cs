#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using J4JSoftware.Roslyn;
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

            var parsedTypes = new NamespaceCollection();
            parsedTypes.ParseFile( projFilePath, out _ ).Should().BeTrue();

            CompareRoslynNamedTypesToParsed( walker, parsedTypes );
            CompareParsedToRoslynNamedTypes( walker, parsedTypes );
        }

        private void CompareRoslynNamedTypesToParsed( ISyntaxWalkerNG walker, NamespaceCollection parsedTypes )
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

                        if( x is not ITypeArguments typeArgsInfo )
                            return false;

                        return !ntSymbol!.TypeArguments
                            .Where( ( t, idx ) => !ntSymbol.TypeArguments[ idx ].Name
                                .Equals( typeArgsInfo.TypeArguments[ idx ], StringComparison.Ordinal ) )
                            .Any();
                    } )
                    .FirstOrDefault();

                namedTypeInfo.Should().NotBeNull();

                CompareElements<IMethodSymbol, MethodInfo>( ntSymbol!, namedTypeInfo!.Methods );
                CompareElements<IEventSymbol, EventInfo>( ntSymbol!, namedTypeInfo.Events );
                CompareElements<IPropertySymbol, PropertyInfo>( ntSymbol!, namedTypeInfo.Properties );

                if( namedTypeInfo is ClassInfo classInfo )
                    CompareElements<IFieldSymbol, FieldInfo>( ntSymbol!, classInfo.Fields );
            }
        }

        private void CompareParsedToRoslynNamedTypes( ISyntaxWalkerNG walker, NamespaceCollection parsedTypes )
        {
            var rosylynTypes = walker.NodeCollectors.FirstOrDefault( x => x.SymbolType == typeof(ITypeSymbol) )?
                                   .Cast<ITypeSymbol>()
                                   .ToList()
                               ?? new List<ITypeSymbol>();

            foreach( var parsedType in parsedTypes )
            {
                var roslynType = rosylynTypes
                    .Where( x =>
                    {
                        if( !x.Name.Equals( parsedType.Name, StringComparison.Ordinal ) )
                            return false;

                        if( x is not INamedTypeSymbol ntSymbol ||
                            parsedType is not ITypeArguments typeArgsInfo )
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
            where TInfo : ElementInfo
        {
            var symbols = ntSymbol.GetMembers().Where( x => x is TSymbol ).Cast<TSymbol>().ToList();

            foreach( var symbol in symbols )
            {
                var infoItem =
                    infoCollection.FirstOrDefault( x => x.Name.Equals( symbol.Name, StringComparison.Ordinal ) );

                infoItem.Should().NotBeNull();

                if( !typeof(IMethodSymbol).IsAssignableFrom( typeof(TSymbol) ) )
                    continue;

                if( infoItem is ITypeArguments typeArgsInfo )
                    CompareArguments( ( (IMethodSymbol) symbol ).TypeArguments.ToList(), typeArgsInfo.TypeArguments );
                else
                    throw new ArgumentException(
                        $"{nameof(infoItem)} is not a {nameof(ITypeArguments)} but should be" );

                if( infoItem is IArguments argsInfo )
                    CompareArguments( ( (IMethodSymbol) symbol ).Parameters
                        .Select( p => p.Type )
                        .ToList(),
                        argsInfo.Arguments );
                else
                    throw new ArgumentException(
                        $"{nameof(infoItem)} is not a {nameof(ITypeArguments)} but should be" );
            }

            foreach( var infoItem in infoCollection )
            {
                var symbol = symbols.FirstOrDefault( x => x.Name.Equals( infoItem.Name ) );

                symbol.Should().NotBeNull();

                if( infoItem is ITypeArguments typeArgsInfo )
                {
                    if( symbol is IMethodSymbol methodSymbol )
                        CompareArguments( methodSymbol.TypeArguments.ToList(), typeArgsInfo.TypeArguments );
                    else
                        throw new ArgumentException(
                            $"{nameof(symbol)} should implement {nameof(IMethodSymbol)} but doesn't" );
                }

                if( infoItem is IArguments argsInfo )
                {
                    switch( symbol )
                    {
                        case IMethodSymbol methodSymbol:
                            CompareArguments( methodSymbol.Parameters
                                .Select( p => p.Type )
                                .ToList(),
                                argsInfo.Arguments );

                            break;

                        case IPropertySymbol propSymbol:
                            CompareArguments( propSymbol.Parameters
                                    .Select( p => p.Type )
                                    .ToList(),
                                argsInfo.Arguments );

                            break;

                        default:
                            throw new ArgumentException(
                                $"{nameof(symbol)} should implement {nameof(IMethodSymbol)} or {nameof(IPropertySymbol)} but doesn't" );
                    }
                }
                else
                    throw new ArgumentException(
                        $"{nameof(infoItem)} is not a {nameof(ITypeArguments)} but should be" );
            }
        }

        private void CompareArguments( List<ITypeSymbol> symbols, List<string> typeArgs )
        {
            for( var idx = 0; idx < symbols.Count; idx++ )
                symbols[ idx ].Name.Should().Be( typeArgs[ idx ] );
        }
    }
}