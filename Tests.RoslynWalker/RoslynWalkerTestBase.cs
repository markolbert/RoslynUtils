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

            CheckTypeSymbols( projFilePath, walker );

            //var context = ServiceProvider.Instance.GetRequiredService<ActionsContext>();

            //context.StopOnFirstError = true;

            //var walkers = ServiceProvider.Instance.GetRequiredService<SyntaxWalkers>();

            //walkers.Process(result).Should().BeTrue();
        }

        private void CheckTypeSymbols( string projFilePath, ISyntaxWalkerNG walker )
        {
            var typesScanned = walker.NodeCollectors.FirstOrDefault( x => x.SymbolType == typeof(ITypeSymbol) )?
                                     .Cast<ITypeSymbol>()
                                     .ToList()
                                 ?? new List<ITypeSymbol>();

            var typesFound = GetClasses( projFilePath );

            foreach( var typeScanned in typesScanned )
            {
                var ntSymbol = typeScanned as INamedTypeSymbol;
                ntSymbol.Should().NotBeNull();

                var typeFound = typesFound
                    .FirstOrDefault( x => x.Name.Equals( typeScanned.Name, StringComparison.Ordinal )
                                          && x.TypeArguments.Count == ntSymbol!.TypeArguments.Length );

                typeFound.Should().NotBeNull();

                for( var idx = 0; idx < ntSymbol!.TypeParameters.Length; idx++ )
                {
                    ntSymbol.TypeParameters[ idx ].Name.Should().Be( typeFound!.TypeArguments[ idx ] );
                }
            }

            foreach( var typeFound in typesFound )
            {
                var typeScanned = typesScanned
                    .Where( x =>
                        x.Name.Equals( typeFound.Name, StringComparison.Ordinal )
                        && x is INamedTypeSymbol ntSymbol
                        && ntSymbol.TypeParameters.Length == typeFound.TypeArguments.Count )
                    .Cast<INamedTypeSymbol>()
                    .FirstOrDefault();

                typeScanned.Should().NotBeNull();

                for( var idx = 0; idx < typeScanned!.TypeParameters.Length; idx++ )
                {
                    typeScanned.TypeParameters[ idx ].Name.Should().Be( typeFound.TypeArguments[ idx ] );
                }
            }
        }

        private List<TypeInfo> GetClasses( string projPath )
        {
            var retVal = new List<TypeInfo>();

            var projDir = new DirectoryInfo( Path.GetDirectoryName( projPath )! );

            foreach( var csFile in projDir.GetFiles( "*.cs", SearchOption.AllDirectories ) )
            {
                foreach( var classLine in File.ReadAllLines( csFile.FullName ) )
                {
                    var isDelegate = classLine.IndexOf( "public delegate", StringComparison.Ordinal ) >= 0;
                    var isClass =classLine.IndexOf( "public class ", StringComparison.Ordinal ) >= 0;
                    var isInterface = classLine.IndexOf( "public interface", StringComparison.Ordinal ) >= 0;

                    if( !( isDelegate || isClass || isInterface ) )
                        continue;

                    var parts = classLine.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                    var rawName = parts.Length > 3
                        ? isDelegate
                            ? parts[ 3 ][..^1]
                            : string.Join( " ", parts[ 2.. ] )
                        : parts[ 2 ];

                    var findColon = rawName.IndexOf( ":", StringComparison.Ordinal );
                    if( findColon >= 0 )
                        rawName = rawName[ ..( findColon - 1 ) ];

                    var findLessThan = rawName.IndexOf( "<", StringComparison.Ordinal );

                    TypeInfo? typeInfo = null;

                    if( findLessThan >= 0 )
                    {
                        typeInfo = new TypeInfo { Name = rawName[ ..findLessThan ].Trim() };

                        var typeArgs = rawName[ ( findLessThan + 1 )..^1 ]
                            .Split( "," )
                            .Select( x => x.Trim() )
                            .ToList();

                        typeInfo.TypeArguments = typeArgs.Select( x =>
                            {
                                var typeParts = x.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                                return typeParts.Length == 1 ? typeParts[ 0 ] : typeParts[ ^1 ];
                            } )
                            .ToList();

                    }
                    else typeInfo = new TypeInfo { Name = rawName, TypeArguments = new List<string>() };

                    typeInfo.IsClass = isClass;
                    typeInfo.IsDelegate = isDelegate;
                    typeInfo.IsInterface = isInterface;

                    retVal.Add( typeInfo );
                }
            }

            return retVal;
        }
    }
}
