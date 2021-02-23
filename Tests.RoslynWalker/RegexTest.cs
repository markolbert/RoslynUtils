using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.RoslynWalker
{
    public class RegexTest
    {
        private readonly ParserCollection _parsers = ServiceProvider.Instance.GetRequiredService<ParserCollection>();

        [Theory]
        [InlineData("public", "class", "Ralph", "", true, true,true, Accessibility.Public)]
        [InlineData("public", "class", "Ralph", "", true, true,true, Accessibility.Public, "int", "bool", "T<string, T2<int, bool>>")]
        [InlineData("public", "interface", "Ralph", "", true, true, false, Accessibility.Public)]
        [InlineData("public", "event", "Ralph", "", true, false,false, Accessibility.Public)]
        public void ParseClass(
            string accessText, 
            string nature, 
            string name, 
            string ancestry, 
            bool parserExists, 
            bool parseSuccess,
            bool correctType,
            Accessibility accessibility,  
            params string[] typeArgs )
        {
            var sb = AggregateTypeArguments( typeArgs, out var taList );

            sb.Insert( 0, $"{nature} {name}" );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            if( !string.IsNullOrEmpty( ancestry ) )
                sb.Append( $":{ancestry}" );

            var srcLine = new SourceLine( sb.ToString(), LineType.BlockOpener, null );

            var toCheck = ParseEntity<ClassInfo>( srcLine, parserExists, parseSuccess, correctType );
            if( toCheck == null )
                return;

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( taList );
            toCheck.Ancestry.Should().Be( ancestry );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [Theory]
        [InlineData("public", "interface", "Ralph", "", true, true,true, Accessibility.Public)]
        [InlineData("public", "interface", "Ralph", "", true, true,true, Accessibility.Public, "int", "bool", "T<string, T2<int, bool>>")]
        [InlineData("public", "class", "Ralph", "", true, true, false, Accessibility.Public)]
        [InlineData("public", "event", "Ralph", "", true, false,false, Accessibility.Public)]
        public void ParseInterface(
            string accessText, 
            string nature, 
            string name, 
            string ancestry, 
            bool parserExists, 
            bool parseSuccess,
            bool correctType,
            Accessibility accessibility,  
            params string[] typeArgs )
        {
            var sb = AggregateTypeArguments( typeArgs, out var taList );

            sb.Insert( 0, $"{nature} {name}" );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            if( !string.IsNullOrEmpty( ancestry ) )
                sb.Append( $":{ancestry}" );

            var srcLine = new SourceLine( sb.ToString(), LineType.BlockOpener, null );

            var toCheck = ParseEntity<InterfaceInfo>( srcLine, parserExists, parseSuccess, correctType );
            if( toCheck == null )
                return;

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( taList );
            toCheck.Ancestry.Should().Be( ancestry );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [Theory]
        [InlineData("public", "Ralph", true, true,true, Accessibility.Public, new string[0], new string[0])]
        public void ParseDelegate(
            string accessText,
            string name,
            bool parserExists,
            bool parseSuccess,
            bool correctType,
            Accessibility accessibility,
            string[] typeArgs,
            string[] args )
        {
            var sb = AggregateTypeArguments( typeArgs, out var taList );
            var sbArgs = AggregateMethodArguments( args, out var maList );

            sb.Insert( 0, $"delegate {name}" );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            sb.Append( sbArgs );

            var srcLine = new SourceLine( sb.ToString(), LineType.Statement, null );

            var toCheck = ParseEntity<DelegateInfo>( srcLine, parserExists, parseSuccess, correctType );
            if( toCheck == null )
                return;

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( taList );
            toCheck.Arguments.Should().BeEquivalentTo( maList );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        private StringBuilder AggregateTypeArguments( string[] typeArgs, out List<string> result )
        {
            result = typeArgs.Select( x => x.Replace( " ", string.Empty ) )
                .ToList();

            return AggregateArguments( result, '<', '>', false );
        }

        private StringBuilder AggregateMethodArguments( string[] args, out List<string> result )
        {
            result = args.Select( a =>
                {
                    var parts = a.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                    var sb = new StringBuilder();

                    for( var idx = 0; idx < parts.Length; idx++)
                    {
                        sb.Append( idx != parts.Length - 1 ? parts[ idx ].Replace( " ", string.Empty ) : parts[ idx ] );
                    }

                    return sb.ToString();
                } )
                .ToList();

            return AggregateArguments( result, '(', ')', true );
        }

        private StringBuilder AggregateArguments( List<string> args, char openDelim, char closeDelim, bool frameEmpty )
        {
            var sb = args.Aggregate(
                new StringBuilder(),
                ( s, a ) =>
                {
                    if( s.Length > 0 )
                        s.Append( "," );

                    s.Append( a );

                    return s;
                } );

            if( sb.Length <= 0 && !frameEmpty ) 
                return sb;

            sb.Insert( 0, openDelim );
            sb.Append( closeDelim );

            return sb;
        }

        private TInfo? ParseEntity<TInfo>( SourceLine srcLine, bool parserExists, bool parseSuccess,
            bool parseCorrectType )
            where TInfo : BaseInfo
        {
            _parsers.HandlesLine( srcLine ).Should().Be( parserExists );

            if( !parserExists )
                return null;

            var baseInfo = _parsers.Parse( srcLine );

            if( parseSuccess )
                baseInfo.Should().NotBeNull();
            else
            {
                baseInfo.Should().BeNull();
                return null;
            }

            if( parseCorrectType )
            {
                baseInfo!.GetType().Should().Be<TInfo>();
                return (TInfo) baseInfo;
            }

            baseInfo!.GetType().Should().NotBe<TInfo>();
            return null;
        }

        [Theory]
        [InlineData("using", false)]
        [InlineData("using Ralph", true)]
        [InlineData("  using  ", true)]
        [InlineData("  using    Ralph", true)]
        public void UsingDirective( string text, bool success )
        {
            SourceRegex.IsUsingDirective( text ).Should().Be( success );
        }

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

            SourceRegex.ExtractNamedTypeArguments( text, target, out var ntSource )
                .Should().Be( success );

            if( !success )
                return;

            ntSource.Should().NotBeNull();
            ntSource!.Name.Should().Be( name );
            SourceRegex.ParseAccessibility( ntSource.Accessibility, out var temp ).Should().BeTrue();
            temp.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public int SomeMethod", true, "SomeMethod", "int", Accessibility.Public ) ]
        [ InlineData( "int SomeMethod", true, "SomeMethod", "int", Accessibility.Private ) ]
        public void MethodNameTypeAccessibilityExtractor( string text, bool success, string name, string retType,
            Accessibility accessibility )
        {
            SourceRegex.ExtractMethodElements( text, out var methodSource )
                .Should()
                .Be( success );

            if( !success )
                return;

            methodSource.Should().NotBeNull();
            SourceRegex.ParseAccessibility( methodSource!.Accessibility, out var temp ).Should().BeTrue();
            temp.Should().Be( accessibility );
            methodSource.ReturnType.Should().Be( retType.Trim() );
            methodSource.Name.Should().Be( name.Trim() );
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
            SourceRegex.ExtractPropertyElements( text, out var propertySource )
                .Should()
                .Be( success );

            if( !success )
                return;

            propertySource.Should().NotBeNull();
            SourceRegex.ParseAccessibility( propertySource!.Accessibility, out var temp ).Should().BeTrue();
            temp.Should().Be( accessibility );
            propertySource.ReturnType.Should().Be( propType.Trim() );
            propertySource.Name.Should().Be( name.Trim() );
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

            SourceRegex.ExtractDelegateArguments( sb.ToString(), out var delegateSource )
                .Should()
                .Be( success );

            if( !success )
                return;

            delegateSource.Should().NotBeNull();
            SourceRegex.ParseAccessibility( delegateSource!.Accessibility, out var temp ).Should().BeTrue();
            temp.Should().Be( accessibility );
            delegateSource.TypeArguments.Should().BeEquivalentTo( typeArgs );
            delegateSource.Arguments.Should().BeEquivalentTo( arguments );
            delegateSource.Name.Should().Be( name );
        }
    }
}
