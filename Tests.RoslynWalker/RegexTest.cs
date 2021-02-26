using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
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
            var sb = AggregateArguments( typeArgs, '<', '>', false );

            sb.Insert( 0, $"{nature} {name}" );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            if( !string.IsNullOrEmpty( ancestry ) )
                sb.Append( $":{ancestry}" );

            var srcLine = new BlockOpeningLine( sb.ToString(), null );

            var infoList = ParseSourceLine<ClassInfo>( srcLine, parserExists, parseSuccess, correctType );
            if( infoList == null )
                return;

            infoList.Count.Should().Be( 1 );
            var toCheck = infoList[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( typeArgs );
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
            var sb = AggregateArguments( typeArgs, '<', '>', false );

            sb.Insert( 0, $"{nature} {name}" );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            if( !string.IsNullOrEmpty( ancestry ) )
                sb.Append( $":{ancestry}" );

            var srcLine = new BlockOpeningLine( sb.ToString(), null );

            var infoList = ParseSourceLine<InterfaceInfo>( srcLine, parserExists, parseSuccess, correctType );
            if( infoList == null )
                return;

            infoList.Count.Should().Be( 1 );
            var toCheck = infoList[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( typeArgs );
            toCheck.Ancestry.Should().Be( ancestry );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "Ralph", true, true, true, Accessibility.Public, new string[0], new string[0] ) ]
        [ InlineData( "public", "Ralph", true, true, true, Accessibility.Public,
            new[] { "int", "bool", "T<string, T2<int, bool>>" }, new string[0] ) ]
        [ InlineData( "", "Ralph", true, true, true, Accessibility.Private,
            new[] { "int", "bool", "T<string, T2<int, bool>>" },
            new[] { "int arg1", "T<int, string, T2<int, bool>> arg2" } ) ]
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
            var sb = AggregateArguments( typeArgs, '<', '>', false );
            var sbArgs = AggregateArguments( args, '(', ')', true );

            sb.Insert( 0, $"delegate {name}" );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            sb.Append( sbArgs );

            var container = new BlockOpeningLine( "public class TestClass", null );
            container.ChildBlock.AddStatement( sb.ToString() );

            var infoList = ParseSourceLine<DelegateInfo>( container.ChildBlock.Lines.First(), 
                parserExists,
                parseSuccess, 
                correctType );

            if( infoList == null )
                return;

            infoList.Count.Should().Be( 1 );
            var toCheck = infoList[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( typeArgs );
            toCheck.Arguments.Should().BeEquivalentTo( args );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "Ralph","int", true, true, true, Accessibility.Public, new string[0], new string[0] ) ]
        [ InlineData( "public", "Ralph","T<int, bool, T2<string>>", true, true, true, Accessibility.Public,
            new[] { "int", "bool", "T<string, T2<int, bool>>" }, new string[0] ) ]
        [ InlineData( "", "Ralph", "void", true, true, true, Accessibility.Private,
            new[] { "int", "bool", "T<string, T2<int, bool>>" },
            new[] { "int arg1", "T<int, string, T2<int, bool>> arg2" } ) ]
        public void ParseMethod(
            string accessText,
            string name,
            string returnType,
            bool parserExists,
            bool parseSuccess,
            bool correctType,
            Accessibility accessibility,
            string[] typeArgs,
            string[] args )
        {
            var sb = AggregateArguments( typeArgs, '<', '>', false );
            var sbArgs = AggregateArguments( args, '(', ')', true );

            sb.Insert( 0, $"{name}" );
            sb.Insert( 0, $"{returnType} " );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            sb.Append( sbArgs );

            var container = new BlockOpeningLine( "public class Ralph", null );
            container.ChildBlock.AddStatement( sb.ToString() );

            var infoList = ParseSourceLine<MethodInfo>( container.ChildBlock.Lines.First(), 
                parserExists, 
                parseSuccess,
                correctType );

            if( infoList == null )
                return;

            infoList.Count.Should().Be( 1 );
            var toCheck = infoList[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( typeArgs );
            toCheck.Arguments.Should().BeEquivalentTo( args );
            toCheck.ReturnType.Should().Be( returnType );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "Ralph","int", true, true, true, Accessibility.Public, new string[0] ) ]
        [ InlineData( "public", "Ralph","T<int, bool, T2<string>>", true, true, true, Accessibility.Public,
            new[] { "int idx1", "bool idx2", "T<string, T2<int, bool>> idx3" } ) ]
        [ InlineData( "", "Ralph", "void", true, true, true, Accessibility.Private,
            new[] { "int idx1", "T<int, string, T2<int, bool>> idx2" } ) ]
        public void ParseProperty(
            string accessText,
            string name,
            string propertyType,
            bool parserExists,
            bool parseSuccess,
            bool correctType,
            Accessibility accessibility,
            string[] indexers )
        {
            var sb = AggregateArguments( indexers, '[', ']', false );;

            sb.Insert( 0, $"{name}" );
            sb.Insert( 0, $"{propertyType} " );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            var containerLine = new BlockOpeningLine( sb.ToString(), null );
            containerLine.ChildBlock.AddBlockOpener( "get" );

            var infoList = ParseSourceLine<PropertyInfo>( containerLine, parserExists, parseSuccess, correctType );
            if( infoList == null )
                return;

            infoList.Count.Should().Be( 1 );
            var toCheck = infoList[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.Arguments.Should().BeEquivalentTo( indexers );
            toCheck.PropertyType.Should().Be( propertyType );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "Ralph","int", LineType.BlockOpener,true, true, true, Accessibility.Public ) ]
        [ InlineData( "public", "Ralph","T<int, bool, T2<string>>",LineType.BlockOpener, true, true, true, Accessibility.Public ) ]
        [ InlineData( "", "Ralph", "",LineType.BlockOpener, true, true, true, Accessibility.Private ) ]
        [ InlineData( "public", "Ralph","int", LineType.Statement,true, true, true, Accessibility.Public ) ]
        [ InlineData( "public", "Ralph","T<int, bool, T2<string>>",LineType.Statement, true, true, true, Accessibility.Public ) ]
        [ InlineData( "", "Ralph", "",LineType.Statement, true, true, true, Accessibility.Private ) ]
        public void ParseEvent(
            string accessText,
            string name,
            string eventArgType,
            LineType lineType,
            bool parserExists,
            bool parseSuccess,
            bool correctType,
            Accessibility accessibility )
        {
            lineType.Should().NotBe( LineType.BlockCloser );

            var sb = new StringBuilder( "event EventHandler" );

            if( !string.IsNullOrEmpty( eventArgType ) )
                sb.Append( $"<{eventArgType}>" );

            sb.Append( $" {name}" );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            var container = new BlockOpeningLine( "public class Ralph", null );

            switch( lineType )
            {
                case LineType.BlockOpener:
                    container.ChildBlock.AddBlockOpener(sb.ToString());
                    break;

                case LineType.Statement:
                    container.ChildBlock.AddStatement(sb.ToString());
                    break;
            }

            var infoList = ParseSourceLine<EventInfo>( container.ChildBlock.Lines.First(), 
                parserExists, 
                parseSuccess,
                correctType );
            if( infoList == null )
                return;

            infoList.Count.Should().Be( 1 );
            var toCheck = infoList[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.EventArgType.Should().Be( eventArgType );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "int", true, true, true, Accessibility.Public, "field1" ) ]
        [ InlineData( "public", "T<int, bool, T2<string>>",true, true, true, Accessibility.Public, "field1", "field2" ) ]
        [ InlineData( "", "",true, true, true, Accessibility.Private, "field1", "field2", "field3" ) ]
        [ InlineData( "public", "int", true, true, true, Accessibility.Public, "field1=0" ) ]
        [ InlineData( "public", "T<int, bool, T2<string>>",true, true, true, Accessibility.Public, "field1", "field2 = null" ) ]
        [ InlineData( "", "int",true, true, true, Accessibility.Private, "field1=1", "field=2", "field3=3" ) ]
        public void ParseField(
            string accessText,
            string fieldType,
            bool parserExists,
            bool parseSuccess,
            bool correctType,
            Accessibility accessibility,
            params string[] nameClauses )
        {
            var sb = new StringBuilder();

            var fields = new List<FieldInfo>();

            foreach( var nameClause in nameClauses )
            {
                var equalsLoc = nameClause.IndexOf( "=", StringComparison.Ordinal );

                var fieldSrc = equalsLoc < 0
                    ? new FieldSource( nameClause.Trim(), accessText, fieldType, string.Empty )
                    : new FieldSource( nameClause[ ..equalsLoc ].Trim(), accessText, fieldType,
                        nameClause[ ( equalsLoc + 1 ).. ].Trim() );

                fields.Add( new FieldInfo( fieldSrc ) );

                if( sb.Length > 0 )
                    sb.Append( ", " );

                sb.Append( nameClause );
            }

            sb.Insert( 0, $"{fieldType} " );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            var classLine = new BlockOpeningLine( "public class TestClass", null );
            classLine.ChildBlock.AddStatement( sb.ToString() );

            var toCheck = ParseSourceLine<FieldInfo>( classLine.ChildBlock.Lines.First(), 
                parserExists, 
                parseSuccess,
                correctType );

            if( toCheck == null )
                return;

            toCheck.Should().BeEquivalentTo( fields );

            foreach( var field in toCheck )
            {
                field.Accessibility.Should().Be( accessibility );
            }
        }

        private StringBuilder AggregateArguments( IEnumerable<string> args, char openDelim, char closeDelim, bool frameEmpty )
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

        private List<TInfo>? ParseSourceLine<TInfo>( SourceLine srcLine, bool parserExists, bool parseSuccess,
            bool parseCorrectType )
            where TInfo : BaseInfo
        {
            _parsers.HandlesLine( srcLine ).Should().Be( parserExists );

            if( !parserExists )
                return null;

            var infoList = _parsers.Parse( srcLine );

            if( parseSuccess )
                infoList.Should().NotBeNull();
            else
            {
                infoList.Should().BeNull();
                return null;
            }

            if( parseCorrectType )
            {
                foreach( var info in infoList! )
                {
                    info.GetType().Should().Be<TInfo>();
                }

                return infoList.Cast<TInfo>().ToList();
            }

            foreach( var info in infoList! )
            {
                info.GetType().Should().NotBe<TInfo>();
            }

            return null;
        }

        //[Theory]
        //[InlineData("using", false)]
        //[InlineData("using Ralph", true)]
        //[InlineData("  using  ", true)]
        //[InlineData("  using    Ralph", true)]
        //public void UsingDirective( string text, bool success )
        //{
        //    SourceRegex.IsUsingDirective( text ).Should().Be( success );
        //}

        //[ Theory ]
        //[ InlineData( "[attr1][attr2]  [attr3]", "public class Wow<T1, T2<T3>>" ) ]
        //[ InlineData( "[attr1][attr2]  [attr3]", "protected internal class Wow<T1, T2<T3>>" ) ]
        //[ InlineData( "", "public class Wow<T1, T2<T3>>" ) ]
        //[ InlineData( "", "protected internal class Wow<T1, T2<T3>>" ) ]
        //[ InlineData( "[attr1][attr2]  [attr3]", "public class Wow" ) ]
        //[ InlineData( "[attr1][attr2]  [attr3]", "protected internal class Wow" ) ]
        //[ InlineData( "", "public class Wow" ) ]
        //[ InlineData( "", "protected internal class Wow" ) ]
        //public void ExtractAncestry( string attrText, string declText )
        //{
        //    foreach( var spacer in new[] { " ", "   " } )
        //    {
        //        var preamble = $"{spacer}{attrText}{spacer}{declText}{spacer}";
        //        var text = $"{preamble}:{spacer}AncestryText";

        //        SourceRegex.ExtractAncestry( text, out var preambleMatch, out var ancestry )
        //            .Should().BeTrue();

        //        preambleMatch.Should().Be( preamble.Trim() );
        //        ancestry.Should().Be( "AncestryText" );
        //    }
        //}

        //[ Theory ]
        //[ InlineData( "gubbage gubbage gubbage", true, "attr1", "attr2", "attr3" ) ]
        //[ InlineData( "", true, "attr1", "attr2", "attr3" ) ]
        //public void ExtractAttributes(
        //    string suffix,
        //    bool success,
        //    params string[] attributesText )
        //{
        //    foreach( var spacer in new[] { "", " ", "   " } )
        //    {
        //        var sb = new StringBuilder();

        //        foreach( var text in attributesText )
        //        {
        //            sb.Append( $"{spacer}[{text}]" );
        //        }

        //        sb.Append( $"{spacer}{suffix}" );

        //        SourceRegex.ExtractAttributes( sb.ToString(), out var postAttribute, out var attributes )
        //            .Should().Be( success );

        //        if( !success )
        //            continue;

        //        attributes.Count.Should().Be( attributesText.Length );

        //        for( var idx = 0; idx < attributesText.Length; idx++ )
        //        {
        //            attributes[ idx ].Should().Be( attributesText[ idx ] );
        //        }
        //    }
        //}

        //[ Theory ]
        //[ InlineData( "gubbage gubbage gubbage", true, "int", "string?", "T<int>" ) ]
        //[ InlineData( "gubbage gubbage gubbage", true, "int[]", "List<string>", "T1<T2<int>, bool, T3<T<T4>>>" ) ]
        //[ InlineData( "", true, "int", "string?", "T<int>" ) ]
        //[ InlineData( "", true, "int[]", "List<string>", "T1<T2<int>, bool, T3<T<T4>>>" ) ]
        //public void ExtractTypeArguments(
        //    string prefix,
        //    bool success,
        //    params string[] typeArgTexts )
        //{
        //    foreach( var spacer in new[] { "", " ", "   " } )
        //    {
        //        var sb = new StringBuilder();

        //        foreach( var text in typeArgTexts )
        //        {
        //            if( sb.Length > 0 )
        //                sb.Append( "," );

        //            sb.Append( $"{spacer}{text}" );
        //        }

        //        sb.Insert( 0, $"{prefix}<" );
        //        sb.Append( $">{spacer}" );

        //        SourceRegex.ExtractTypeArguments( sb.ToString(), out var preamble, out var typeArgs )
        //            .Should().Be( success );

        //        if( !success )
        //            continue;

        //        preamble.Should().Be( prefix.Trim() );

        //        for( var idx = 0; idx < typeArgTexts.Length; idx++ )
        //        {
        //            typeArgs[ idx ].Should().Be( typeArgTexts[ idx ].Replace( " ", string.Empty ).Trim() );
        //        }
        //    }
        //}

        //[ Theory ]
        //[ InlineData( "public int SomeMethod", true, "int ralph", "string? jones", "T<int> whoever" ) ]
        //[ InlineData( "public int SomeMethod", true ) ]
        //[ InlineData( "public int SomeMethod", true, "int ralph", "string? jones",
        //    "T<int, string> whoever" ) ]
        //[ InlineData( "public int SomeMethod", true, "int ralph", "string? jones",
        //    "T<int, string, T3<int>> whoever" ) ]
        //public void ParseMethodArguments(
        //    string prefix,
        //    bool success,
        //    params string[] argTexts )
        //{
        //    foreach( var spacer in new[] { "", " ", "   " } )
        //    {
        //        var sb = new StringBuilder();

        //        foreach( var text in argTexts )
        //        {
        //            if( sb.Length > 0 )
        //                sb.Append( "," );

        //            sb.Append( $"{spacer}{text}" );
        //        }

        //        sb.Insert( 0, $"{prefix}(" );
        //        sb.Append( $"){spacer}" );

        //        SourceRegex.ExtractMethodArguments( sb.ToString(), out var preamble, out var arguments )
        //            .Should().Be( success );

        //        if( !success )
        //            continue;

        //        preamble.Should().Be( prefix.Trim() );

        //        if( string.IsNullOrEmpty( prefix ) )
        //            continue;

        //        for( var idx = 0; idx < argTexts.Length; idx++ )
        //        {
        //            arguments[ idx ].Should().Be( argTexts[ idx ].Trim() );
        //        }
        //    }
        //}

        //[ Theory ]
        //[ InlineData( "public class SomeClass", true, "SomeClass", Accessibility.Public ) ]
        //[ InlineData( "   class    SomeClass  ", true, "SomeClass", Accessibility.Private ) ]
        //[ InlineData( "public interface ISomeInterface", true, "ISomeInterface", Accessibility.Public ) ]
        //[ InlineData( "   interface ISomeInterface", true, "ISomeInterface", Accessibility.Private ) ]
        //[ InlineData( "public animal SomeClass", false, "SomeClass", Accessibility.Public ) ]
        //public void NamedTypeNameAccessibilityExtractor( string text, bool success, string name,
        //    Accessibility accessibility )
        //{
        //    var target = text.IndexOf( "class", StringComparison.Ordinal ) >= 0
        //        ? "class"
        //        : "interface";

        //    SourceRegex.ExtractNamedTypeArguments( text, target, out var ntSource )
        //        .Should().Be( success );

        //    if( !success )
        //        return;

        //    ntSource.Should().NotBeNull();
        //    ntSource!.Name.Should().Be( name );
        //    SourceRegex.ParseAccessibility( ntSource.Accessibility, out var temp ).Should().BeTrue();
        //    temp.Should().Be( accessibility );
        //}

        //[ Theory ]
        //[ InlineData( "public int SomeMethod", true, "SomeMethod", "int", Accessibility.Public ) ]
        //[ InlineData( "int SomeMethod", true, "SomeMethod", "int", Accessibility.Private ) ]
        //public void MethodNameTypeAccessibilityExtractor( string text, bool success, string name, string retType,
        //    Accessibility accessibility )
        //{
        //    SourceRegex.ExtractMethodElements( text, out var methodSource )
        //        .Should()
        //        .Be( success );

        //    if( !success )
        //        return;

        //    methodSource.Should().NotBeNull();
        //    SourceRegex.ParseAccessibility( methodSource!.Accessibility, out var temp ).Should().BeTrue();
        //    temp.Should().Be( accessibility );
        //    methodSource.ReturnType.Should().Be( retType.Trim() );
        //    methodSource.Name.Should().Be( name.Trim() );
        //}

        //[ Theory ]
        //[ InlineData( "public int[] Ralph", true ) ]
        //[ InlineData( "public int[] Ralph", true, "int idx", "int idx2" ) ]
        //public void PropertyIndexerExtractor( string preamble, bool success, params string[] indexers )
        //{
        //    var indexerText = indexers.Aggregate(
        //        new StringBuilder(),
        //        ( sb, indexer ) =>
        //        {
        //            if( sb.Length > 0 )
        //                sb.Append( "," );

        //            sb.Append( indexer );

        //            return sb;
        //        },
        //        sb =>
        //        {
        //            if( indexers.Length <= 0 )
        //                return sb.ToString();

        //            sb.Insert( 0, " this[" );
        //            sb.Append( "]" );

        //            return sb.ToString();
        //        } );

        //    var text = $"{preamble}{indexerText}";

        //    SourceRegex.ExtractPropertyIndexers( text, out var matchPreamble, out var matchIndexers )
        //        .Should()
        //        .Be( success );

        //    if( !success )
        //        return;

        //    matchPreamble.Should().Be( preamble );

        //    for( var idx = 0; idx < indexers.Length; idx++ )
        //    {
        //        matchIndexers[ idx ].Should().Be( indexers[ idx ].Trim() );
        //    }
        //}

        //[ Theory ]
        //[ InlineData( "public int SomeProperty", true, "SomeProperty", "int", Accessibility.Public ) ]
        //[ InlineData( "int SomeProperty", true, "SomeProperty", "int", Accessibility.Private ) ]
        //public void PropertyNameTypeAccessibilityExtractor( string text, bool success, string name, string propType,
        //    Accessibility accessibility )
        //{
        //    SourceRegex.ExtractPropertyElements( text, out var propertySource )
        //        .Should()
        //        .Be( success );

        //    if( !success )
        //        return;

        //    propertySource.Should().NotBeNull();
        //    SourceRegex.ParseAccessibility( propertySource!.Accessibility, out var temp ).Should().BeTrue();
        //    temp.Should().Be( accessibility );
        //    propertySource.ReturnType.Should().Be( propType.Trim() );
        //    propertySource.Name.Should().Be( name.Trim() );
        //}

        //[ Theory ]
        //[ InlineData( Accessibility.Public, "SomeClassName", true, new[] { "T0", "T1", "T2<int, bool, T4<T5, bool>>" } ) ]
        //[ InlineData( Accessibility.Private, "SomeClassName", true, new[] { "T0", "T1", "T2<int, bool, T4<T5, bool>>" }, "int arg1",
        //    "T<int, bool, T2<int, bool>> arg2" ) ]
        //public void DelegateTester( Accessibility accessibility, string name, bool success, string[] typeArgs,
        //    params string[] arguments )
        //{
        //    typeArgs = typeArgs.Select( x => x.Replace( " ", string.Empty ) )
        //        .ToArray();

        //    var sb = new StringBuilder();

        //    foreach( var arg in arguments )
        //    {
        //        if( sb.Length > 0 )
        //            sb.Append( "," );

        //        sb.Append( arg );
        //    }

        //    var methodArgsClause = sb.Length > 0 ? $"({sb})" : "()";

        //    sb.Clear();

        //    foreach( var arg in typeArgs )
        //    {
        //        if( sb.Length > 0 )
        //            sb.Append( "," );

        //        sb.Append( arg );
        //    }

        //    var typeArgsClause = sb.Length > 0 ? $"<{sb}>" : string.Empty;

        //    sb.Clear();

        //    sb.Append( $"delegate {name}{typeArgsClause}{methodArgsClause}" );

        //    switch( accessibility )
        //    {
        //        case Accessibility.Undefined:
        //            // convert to private but don't add any text
        //            accessibility = Accessibility.Private;
        //            break;

        //        case Accessibility.ProtectedInternal:
        //            sb.Insert( 0, "protected internal " );
        //            break;

        //        default:
        //            sb.Insert( 0, $"{accessibility.ToString().ToLower()} " );
        //            break;
        //    }

        //    SourceRegex.ExtractDelegateArguments( sb.ToString(), out var delegateSource )
        //        .Should()
        //        .Be( success );

        //    if( !success )
        //        return;

        //    delegateSource.Should().NotBeNull();
        //    SourceRegex.ParseAccessibility( delegateSource!.Accessibility, out var temp ).Should().BeTrue();
        //    temp.Should().Be( accessibility );
        //    delegateSource.TypeArguments.Should().BeEquivalentTo( typeArgs );
        //    delegateSource.Arguments.Should().BeEquivalentTo( arguments );
        //    delegateSource.Name.Should().Be( name );
        //}
    }
}
