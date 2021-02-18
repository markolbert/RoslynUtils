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
        [ InlineData( "[attr1][attr2]  [attr3]", "public class Wow<T1, T2<T3>>" ) ]
        [ InlineData( "[attr1][attr2]  [attr3]", "protected internal class Wow<T1, T2<T3>>" ) ]
        [ InlineData( "", "public class Wow<T1, T2<T3>>" ) ]
        [ InlineData( "", "protected internal class Wow<T1, T2<T3>>" ) ]
        [ InlineData( "[attr1][attr2]  [attr3]", "public class Wow" ) ]
        [ InlineData( "[attr1][attr2]  [attr3]", "protected internal class Wow" ) ]
        [ InlineData( "", "public class Wow" ) ]
        [ InlineData( "", "protected internal class Wow" ) ]
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

        [ Theory ]
        [ InlineData( "public class SomeClass", true, "SomeClass", Accessibility.Public ) ]
        [ InlineData( "   class    SomeClass  ", true, "SomeClass", Accessibility.Private ) ]
        [ InlineData( "public interface ISomeInterface", true, "ISomeInterface", Accessibility.Public ) ]
        [ InlineData( "   interface ISomeInterface", true, "ISomeInterface", Accessibility.Private ) ]
        [ InlineData( "public animal SomeClass", false, "SomeClass", Accessibility.Public ) ]
        public void NamedTypeNameAccessibilityExtractor( string text, bool success, string name,
            Accessibility accessibility )
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

        [ Theory ]
        [ InlineData( "public int SomeMethod", true, "SomeMethod", "int", Accessibility.Public ) ]
        [ InlineData( "int SomeMethod", true, "SomeMethod", "int", Accessibility.Private ) ]
        public void MethodNameTypeAccessibilityExtractor( string text, bool success, string name, string retType,
            Accessibility accessibility )
        {
            SourceRegex.ExtractMethodNameTypeAccessibility( text, out var matchName, out var matchReturnType,
                    out var matchAccessibility )
                .Should()
                .Be( success );

            if( !success )
                return;

            accessibility.Should().Be( matchAccessibility );
            matchReturnType.Should().Be( retType.Trim() );
            matchName.Should().Be( name.Trim() );
        }

        [ Theory ]
        [ InlineData( "public int[] Ralph", true ) ]
        [ InlineData( "public int[] Ralph", true, "int idx", "int idx2" ) ]
        public void PropertyIndexerExtractor( string preamble, bool success, params string[] indexers )
        {
            var indexerText = indexers.Aggregate(
                new StringBuilder(),
                ( sb, indexer ) =>
                {
                    if( sb.Length > 0 )
                        sb.Append( "," );

                    sb.Append( indexer );

                    return sb;
                },
                sb =>
                {
                    if( indexers.Length <= 0 )
                        return sb.ToString();

                    sb.Insert( 0, " this[" );
                    sb.Append( "]" );

                    return sb.ToString();
                } );

            var text = $"{preamble}{indexerText}";

            SourceRegex.ExtractPropertyIndexers( text, out var matchPreamble, out var matchIndexers )
                .Should()
                .Be( success );

            if( !success )
                return;

            matchPreamble.Should().Be( preamble );

            for( var idx = 0; idx < indexers.Length; idx++ )
            {
                matchIndexers[ idx ].Should().Be( indexers[ idx ].Trim() );
            }
        }

        [ Theory ]
        [ InlineData( "public int SomeProperty", true, "SomeProperty", "int", Accessibility.Public ) ]
        [ InlineData( "int SomeProperty", true, "SomeProperty", "int", Accessibility.Private ) ]
        public void PropertyNameTypeAccessibilityExtractor( string text, bool success, string name, string propType,
            Accessibility accessibility )
        {
            SourceRegex.ExtractPropertyNameTypeAccessibility( text, out var matchName, out var matchPropType,
                    out var matchAccessibility )
                .Should()
                .Be( success );

            if( !success )
                return;

            accessibility.Should().Be( matchAccessibility );
            matchPropType.Should().Be( propType.Trim() );
            matchName.Should().Be( name.Trim() );
        }

        [ Theory ]
        [ InlineData( Accessibility.Public, "SomeClassName", true, new[] { "T0", "T1", "T2<int, bool, T4<T5, bool>>" } ) ]
        [ InlineData( Accessibility.Private, "SomeClassName", true, new[] { "T0", "T1", "T2<int, bool, T4<T5, bool>>" }, "int arg1",
            "T<int, bool, T2<int, bool>> arg2" ) ]
        public void DelegateTester( Accessibility accessibility, string name, bool success, string[] typeArgs,
            params string[] arguments )
        {
            typeArgs = typeArgs.Select( x => x.Replace( " ", string.Empty ) )
                .ToArray();

            var sb = new StringBuilder();

            foreach( var arg in arguments )
            {
                if( sb.Length > 0 )
                    sb.Append( "," );

                sb.Append( arg );
            }

            var methodArgsClause = sb.Length > 0 ? $"({sb})" : "()";

            sb.Clear();

            foreach( var arg in typeArgs )
            {
                if( sb.Length > 0 )
                    sb.Append( "," );

                sb.Append( arg );
            }

            var typeArgsClause = sb.Length > 0 ? $"<{sb}>" : string.Empty;

            sb.Clear();

            sb.Append( $"delegate {name}{typeArgsClause}{methodArgsClause}" );

            switch( accessibility )
            {
                case Accessibility.Undefined:
                    // convert to private but don't add any text
                    accessibility = Accessibility.Private;
                    break;

                case Accessibility.ProtectedInternal:
                    sb.Insert( 0, "protected internal " );
                    break;

                default:
                    sb.Insert( 0, $"{accessibility.ToString().ToLower()} " );
                    break;
            }

            SourceRegex.ExtractDelegateArguments( sb.ToString(), out var matchName, out var matchAccessibility,
                    out var matchTypeArgs, out var matchArguments )
                .Should()
                .Be( success );

            if( !success )
                return;

            matchAccessibility!.Should().Be( accessibility );
            matchTypeArgs.Should().BeEquivalentTo( typeArgs );
            matchArguments.Should().BeEquivalentTo( arguments );
            matchName.Should().Be( name );
        }
    }
}
