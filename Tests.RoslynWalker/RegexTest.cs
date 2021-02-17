using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac.Diagnostics;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Tests.RoslynWalker
{
    public class RegexTest
    {
        [ Theory ]
        [ InlineData( "[attr1][attr2]  [attr3]","public class Wow<T1, T2<T3>>") ]
        [ InlineData( "[attr1][attr2]  [attr3]","protected internal class Wow<T1, T2<T3>>" ) ]
        [ InlineData( "", "public class Wow<T1, T2<T3>>" ) ]
        [ InlineData( "","protected internal class Wow<T1, T2<T3>>" ) ]
        [ InlineData( "[attr1][attr2]  [attr3]","public class Wow" ) ]
        [ InlineData( "[attr1][attr2]  [attr3]","protected internal class Wow" ) ]
        [ InlineData( "","public class Wow" ) ]
        [ InlineData( "","protected internal class Wow" ) ]
        public void ExtractAncestry( string attrText, string declText )
        {
            foreach( var spacer in new[] { " ", "   " } )
            {
                var preamble = $"{spacer}{attrText}{spacer}{declText}{spacer}";
                var text = $"{preamble}:{spacer}AncestryText";

                SourceRegex.ExtractAncestry( text, out var preambleMatch, out var ancestry )
                    .Should().BeTrue();

                preambleMatch.Should().Be( preamble.Trim() );
                ancestry.Should().Be( "AncestryText" );
            }
        }

        [ Theory ]
        [ InlineData( "gubbage gubbage gubbage", true, "attr1", "attr2", "attr3" ) ]
        [ InlineData( "", true, "attr1", "attr2", "attr3" ) ]
        public void ExtractAttributes( 
            string suffix, 
            bool success,
            params string[] attributesText )
        {
            foreach( var spacer in new[] { "", " ", "   " } )
            {
                var sb = new StringBuilder();

                foreach( var text in attributesText )
                {
                    sb.Append( $"{spacer}[{text}]" );
                }

                sb.Append( $"{spacer}{suffix}" );

                SourceRegex.ExtractAttributes( sb.ToString(), out var postAttribute, out var attributes )
                    .Should().Be( success );

                if( !success )
                    continue;

                attributes.Count.Should().Be( attributesText.Length );

                for( var idx = 0; idx < attributesText.Length; idx++ )
                {
                    attributes[ idx ].Should().Be( attributesText[ idx ] );
                }
            }
        }

        [ Theory ]
        [ InlineData( "gubbage gubbage gubbage", true, "int", "string?", "T<int>" ) ]
        [ InlineData( "gubbage gubbage gubbage", true, "int[]", "List<string>", "T1<T2<int>, bool, T3<T<T4>>>" ) ]
        [ InlineData( "", true, "int", "string?", "T<int>" ) ]
        [ InlineData( "", true, "int[]", "List<string>", "T1<T2<int>, bool, T3<T<T4>>>" ) ]
        public void ExtractTypeArguments( 
            string prefix, 
            bool success,
            params string[] typeArgTexts )
        {
            foreach( var spacer in new[] { "", " ", "   " } )
            {
                var sb = new StringBuilder();

                foreach( var text in typeArgTexts )
                {
                    if( sb.Length > 0 )
                        sb.Append( "," );

                    sb.Append( $"{spacer}{text}" );
                }

                sb.Insert( 0, $"{prefix}<" );
                sb.Append( $">{spacer}" );

                SourceRegex.ExtractTypeArguments( sb.ToString(), out var preamble, out var typeArgs )
                    .Should().Be( success );

                if( !success )
                    continue;

                preamble.Should().Be( prefix.Trim() );

                for( var idx = 0; idx < typeArgTexts.Length; idx++ )
                {
                    typeArgs[ idx ].Should().Be( typeArgTexts[ idx ].Replace( " ", string.Empty ).Trim() );
                }
            }
        }

        [ Theory ]
        [ InlineData( "public int SomeMethod", true, "int ralph", "string? jones", "T<int> whoever" ) ]
        [ InlineData( "public int SomeMethod", true ) ]
        [ InlineData( "public int SomeMethod", true, "int ralph", "string? jones",
            "T<int, string> whoever" ) ]
        [ InlineData( "public int SomeMethod", true, "int ralph", "string? jones",
            "T<int, string, T3<int>> whoever" ) ]
        public void ParseMethodArguments(
            string prefix,
            bool success,
            params string[] argTexts )
        {
            foreach( var spacer in new[] { "", " ", "   " } )
            {
                var sb = new StringBuilder();

                foreach( var text in argTexts )
                {
                    if( sb.Length > 0 )
                        sb.Append( "," );

                    sb.Append( $"{spacer}{text}" );
                }

                sb.Insert( 0, $"{prefix}(" );
                sb.Append( $"){spacer}" );

                SourceRegex.ExtractMethodArguments( sb.ToString(), out var preamble, out var arguments )
                    .Should().Be( success );

                if( !success )
                    continue;

                preamble.Should().Be( prefix.Trim() );

                if( string.IsNullOrEmpty( prefix ) )
                    continue;

                for( var idx = 0; idx < argTexts.Length; idx++ )
                {
                    arguments[ idx ].Should().Be( argTexts[ idx ].Trim() );
                }
            }
        }

        [Theory]
        [InlineData("public class SomeClass", true, "SomeClass", Accessibility.Public)]
        [InlineData("   class    SomeClass  ", true, "SomeClass", Accessibility.Private)]
        [InlineData("public interface ISomeInterface", true, "ISomeInterface", Accessibility.Public)]
        [InlineData("   interface ISomeInterface", true, "ISomeInterface", Accessibility.Private)]
        [InlineData("public animal SomeClass", false, "SomeClass", Accessibility.Public)]
        public void NamedTypeNameAccessibilityExtractor(string text, bool success, string name, Accessibility accessibility )
        {
            var target = text.IndexOf( "class", StringComparison.Ordinal ) >= 0
                ? "class"
                : "interface";

            SourceRegex.ExtractNamedTypeNameAccessibility( text, target, out var matchName, out var matchAccessibility )
                .Should().Be( success );

            if( !success )
                return;

            matchName.Should().Be( name );
            matchAccessibility.Should().Be( accessibility );
        }

        [Theory]
        [InlineData("public int SomeMethod", true, "SomeMethod", "int", Accessibility.Public)]
        [InlineData("int SomeMethod", true, "SomeMethod", "int", Accessibility.Private)]
        public void MethodNameTypeAccessibilityExtractor( string text, bool success, string name, string retType,
            Accessibility accessibility )
        {
            SourceRegex.ParseMethodNameTypeAccessibility( text, out var matchName, out var matchReturnType,
                    out var matchAccessibility )
                .Should()
                .Be(success);

            if( !success )
                return;

            accessibility.Should().Be( matchAccessibility );
            matchReturnType.Should().Be( retType.Trim() );
            matchName.Should().Be( name.Trim() );
        }
    }
}
