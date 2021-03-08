using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.RoslynWalker
{
    public class RegexTest
    {
        private readonly ParserCollection _parsers = ServiceProvider.Instance.GetRequiredService<ParserCollection>();

        [ Theory ]
        [ InlineData( "public", "class", "Ralph", "", true, true, Accessibility.Public ) ]
        [ InlineData( "public", "class", "Ralph", "", true, true, Accessibility.Public, "int", "bool",
            "T<string, T2<int, bool>>" ) ]
        [ InlineData( "public", "interface", "Ralph", "", true, false, Accessibility.Public ) ]
        [ InlineData( "public", "event", "Ralph", "", false, false, Accessibility.Public ) ]
        public void ParseClass(
            string accessText,
            string nature,
            string name,
            string ancestry,
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

            var srcLine = new BlockLine( sb.ToString(), null );

            var elements = ParseSourceLine<ClassInfo>( srcLine, parseSuccess, correctType );
            if( elements == null )
                return;

            elements!.Count.Should().Be( 1 );

            elements[ 0 ].GetType().Should().Be<ClassInfo>();
            var toCheck = (ClassInfo) elements[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( typeArgs );
            toCheck.Ancestry.Should().Be( ancestry );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "interface", "Ralph", "", true, true, Accessibility.Public ) ]
        [ InlineData( "public", "interface", "Ralph", "", true, true, Accessibility.Public, "int", "bool",
            "T<string, T2<int, bool>>" ) ]
        [ InlineData( "public", "class", "Ralph", "", true, false, Accessibility.Public ) ]
        [ InlineData( "public", "event", "Ralph", "", false, false, Accessibility.Public ) ]
        public void ParseInterface(
            string accessText,
            string nature,
            string name,
            string ancestry,
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

            var srcLine = new BlockLine( sb.ToString(), null );

            var elements = ParseSourceLine<InterfaceInfo>( srcLine, parseSuccess, correctType );
            if( elements == null )
                return;

            elements.Count.Should().Be( 1 );

            elements[ 0 ].GetType().Should().Be<InterfaceInfo>();
            var toCheck = (InterfaceInfo) elements[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( typeArgs );
            toCheck.Ancestry.Should().Be( ancestry );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "void", "Ralph", true, true, Accessibility.Public, new string[0], new string[0] ) ]
        [ InlineData( "public", "int", "Ralph", true, true, Accessibility.Public,
            new[] { "int", "bool", "T<string, T2<int, bool>>" }, new string[0] ) ]
        [ InlineData( "","T<string, T2<int, bool>>", "Ralph", true, true, Accessibility.Private,
            new[] { "int", "bool", "T<string, T2<int, bool>>" },
            new[] { "int arg1", "T<int, string, T2<int, bool>> arg2" } ) ]
        public void ParseDelegate(
            string accessText,
            string returnType,
            string name,
            bool parseSuccess,
            bool correctType,
            Accessibility accessibility,
            string[] typeArgs,
            string[] args )
        {
            var sb = AggregateArguments( typeArgs, '<', '>', false );
            var sbArgs = AggregateArguments( args, '(', ')', true );

            sb.Insert( 0, $"{returnType} delegate {name}" );

            if( !string.IsNullOrEmpty( accessText ) )
                sb.Insert( 0, $"{accessText} " );

            sb.Append( sbArgs );

            var container = new BlockLine( "public class TestClass", null );
            container.AddStatement( sb.ToString() );

            var elements = ParseSourceLine<DelegateInfo>( container.Children.First(), 
                parseSuccess, 
                correctType );

            if( elements == null )
                return;

            elements.Count.Should().Be( 1 );

            elements[ 0 ].GetType().Should().Be<DelegateInfo>();
            var toCheck = (DelegateInfo) elements[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( typeArgs );
            toCheck.Arguments.Should().BeEquivalentTo( args );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "Ralph","int", true, true, Accessibility.Public, new string[0], new string[0] ) ]
        [ InlineData( "public", "Ralph","T<int, bool, T2<string>>", true, true, Accessibility.Public,
            new[] { "int", "bool", "T<string, T2<int, bool>>" }, new string[0] ) ]
        [ InlineData( "", "Ralph", "void", true, true, Accessibility.Private,
            new[] { "int", "bool", "T<string, T2<int, bool>>" },
            new[] { "int arg1", "T<int, string, T2<int, bool>> arg2" } ) ]
        public void ParseMethod(
            string accessText,
            string name,
            string returnType,
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

            var container = new BlockLine( "public class Ralph", null );
            container.AddStatement( sb.ToString() );

            var elements = ParseSourceLine<MethodInfo>( container.Children.First(), 
                parseSuccess,
                correctType );

            if( elements == null )
                return;

            elements.Count.Should().Be( 1 );

            elements[ 0 ].GetType().Should().Be<MethodInfo>();
            var toCheck = (MethodInfo) elements[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.TypeArguments.Should().BeEquivalentTo( typeArgs );
            toCheck.Arguments.Should().BeEquivalentTo( args );
            toCheck.ReturnType.Should().Be( returnType );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "Ralph","int", true, true, Accessibility.Public, new string[0] ) ]
        [ InlineData( "public", "Ralph","T<int, bool, T2<string>>", true, true, Accessibility.Public,
            new[] { "int idx1", "bool idx2", "T<string, T2<int, bool>> idx3" } ) ]
        [ InlineData( "", "Ralph", "void", true, true, Accessibility.Private,
            new[] { "int idx1", "T<int, string, T2<int, bool>> idx2" } ) ]
        public void ParseProperty(
            string accessText,
            string name,
            string propertyType,
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

            var containerLine = new BlockLine( sb.ToString(), null );
            containerLine.AddBlockOpener( "get" );

            var elements = ParseSourceLine<PropertyInfo>( containerLine, parseSuccess, correctType );
            if( elements == null )
                return;

            elements.Count.Should().Be( 1 );

            elements[ 0 ].GetType().Should().Be<PropertyInfo>();
            var toCheck = (PropertyInfo) elements[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.Arguments.Should().BeEquivalentTo( indexers );
            toCheck.PropertyType.Should().Be( propertyType );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "Ralph", "int", LineType.BlockOpener, true, true, Accessibility.Public ) ]
        [ InlineData( "public", "Ralph", "T<int, bool, T2<string>>", LineType.BlockOpener, true, true,
            Accessibility.Public ) ]
        [ InlineData( "", "Ralph", "", LineType.BlockOpener, true, true, Accessibility.Private ) ]
        [ InlineData( "public", "Ralph", "int", LineType.Statement, true, true, Accessibility.Public ) ]
        [ InlineData( "public", "Ralph", "T<int, bool, T2<string>>", LineType.Statement, true, true,
            Accessibility.Public ) ]
        [ InlineData( "", "Ralph", "", LineType.Statement, true, true, Accessibility.Private ) ]
        public void ParseEvent(
            string accessText,
            string name,
            string eventArgType,
            LineType lineType,
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

            var container = new BlockLine( "public class Ralph", null );

            switch( lineType )
            {
                case LineType.BlockOpener:
                    container.AddBlockOpener( sb.ToString() );
                    break;

                case LineType.Statement:
                    container.AddStatement( sb.ToString() );
                    break;
            }

            var elements = ParseSourceLine<EventInfo>( container.Children.First(),
                parseSuccess,
                correctType );
            if( elements == null )
                return;

            elements.Count.Should().Be( 1 );

            elements[ 0 ].GetType().Should().Be<EventInfo>();
            var toCheck = (EventInfo) elements[ 0 ];

            toCheck.Name.Should().Be( name );
            toCheck.EventArgType.Should().Be( eventArgType );
            toCheck.Accessibility.Should().Be( accessibility );
        }

        [ Theory ]
        [ InlineData( "public", "int", true, true, Accessibility.Public, "field1" ) ]
        [ InlineData( "public", "T<int, bool, T2<string>>", true, true, Accessibility.Public, "field1", "field2" ) ]
        [ InlineData( "", "", false, true, Accessibility.Private, "field1", "field2", "field3" ) ]
        [ InlineData( "public", "int", true, true, Accessibility.Public, "field1=0" ) ]
        [ InlineData( "public", "T<int, bool, T2<string>>", true, true, Accessibility.Public, "field1",
            "field2 = null" ) ]
        [ InlineData( "", "int", true, true, Accessibility.Private, "field1=1", "field=2", "field3=3" ) ]
        public void ParseField(
            string accessText,
            string fieldType,
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

            var classLine = new BlockLine( "public class TestClass", null );
            classLine.AddStatement( sb.ToString() );

            var elements = _parsers.Parse( classLine.Children.First() );

            if( !parseSuccess )
            {
                elements?.Count.Should().NotBe( nameClauses.Length );
                return;
            }
            
            elements!.Should().NotBeNull();

            elements!.Count.Should().Be( nameClauses.Length );
            elements.Should().BeEquivalentTo( fields );

            if( !correctType ) 
                return;

            foreach (var info in elements!)
            {
                info.GetType().Should().Be<FieldInfo>();
            }

            foreach (var field in elements!.Cast<FieldInfo>())
            {
                field.Accessibility.Should().Be(accessibility);
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

        private List<ElementInfo>? ParseSourceLine<TInfo>( 
            StatementLine srcLine, 
            bool parseSuccess, 
            bool parseCorrectType )
            where TInfo : BaseInfo
        {
            var result = _parsers.Parse( srcLine );

            if( parseSuccess )
                result.Should().NotBeNull();
            else
            {
                result.Should().BeNull();
                return null;
            }

            if( parseCorrectType )
            {
                foreach( var info in result! )
                {
                    info.GetType().Should().Be<TInfo>();
                }

                return result;
            }

            foreach( var info in result! )
            {
                info.GetType().Should().NotBe<TInfo>();
            }

            return null;
        }
    }
}
