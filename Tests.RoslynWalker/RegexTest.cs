using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
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
        public void AncestrySucceeds( string attrText, string declText )
        {
            foreach( var spacer in new[] { " ", "   " } )
            {
                var preamble = $"{spacer}{attrText}{spacer}{declText}{spacer}";
                var text = $"{preamble}:{spacer}AncestryText";

                var results = new string[] { preamble.TrimStart(), "AncestryText" };

                Validate( @"\s*(.+):\s*(.*)", text, true, results );
            }
        }

        [ Theory ]
        [ InlineData( @"\[", @"\]", "gubbage gubbage gubbage", true, true, "attr1", "attr2", "attr3" ) ]
        [ InlineData( @"\[", @"\]", "", true, true, "attr1", "attr2", "attr3" ) ]
        [ InlineData( @"\[", @"\]", "gubbage x[3]", true, false, "attr1", "attr2", "attr3" ) ]
        [ InlineData( @"\[", @"x", "gubbage gubbage gubbage", false, false, "attr1", "attr2", "attr3" ) ]
        public void ExtractAttributes( 
            string openDelim, 
            string closeDelim, 
            string suffix, 
            bool success,
            bool remainderNonEmpty, 
            params string[] attributesText )
        {
            var attributes = new Regex( @$"\s*({openDelim}.*{closeDelim})\s*(.*)" );
            var firstAttr = new Regex( @$"\s*{openDelim}(.+?){closeDelim}\s*(.*)" );

            foreach( var spacer in new[] { " ", "   " } )
            {
                var sb = new StringBuilder();

                foreach( var text in attributesText )
                {
                    sb.Append( $"{spacer}[{text}]" );
                }

                sb.Append( $"{spacer}{suffix}" );

                ValidateArrayResult( attributes, firstAttr, sb.ToString(), success,
                    false, suffix, remainderNonEmpty, x => x.Trim(), attributesText );
            }
        }

        [ Theory ]
        [ InlineData( @"<", @">", "gubbage gubbage gubbage", true, "int ralph", "string? jones", "T<int> whoever" ) ]
        [ InlineData( @"<", @">", "gubbage gubbage gubbage", true, "int[] ralph", "List<string> jones", "T1<T2<int>> whoever" ) ]
        [ InlineData( @"<", @">", "", true, "int ralph", "string? jones", "T<int> whoever" ) ]
        [ InlineData( @"<", @">", "", true, "int[] ralph", "List<string> jones", "T1<T2<int>> whoever" ) ]
        public void ExtractTypeArguments( 
            string openDelim, 
            string closeDelim, 
            string prefix, 
            bool success,
            params string[] typeArgTexts )
        {
            var typeArgs = new Regex( @$"\s*([^<]*)<(.*)>" );
            var firstArg = new Regex( @"\s*(.+?,|.+)(.*)" );

            foreach( var spacer in new[] { " ", "   " } )
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

                ValidateArrayResult( typeArgs, firstArg, sb.ToString(), success, true,
                    prefix, !string.IsNullOrEmpty( prefix ), x => x.Replace( ",", "" ).Trim(),
                    typeArgTexts );
            }
        }

        private void Validate( string pattern, string text, bool success, string[] results )
        {
            var regEx = new Regex( pattern );

            var match = regEx.Match( text );
            match.Success.Should().Be( success );

            if( !success )
                return;

            match.Groups.Count.Should().Be( results.Length + 1 );

            for( var idx = 0; idx < results.Length; idx++)
            {
                var result =  match.Groups[ idx + 1 ].Value;
                if( idx == ( results.Length - 1 ) )
                    result = result.Trim();

                result.Should().Be( results[ idx ] );
            }
        }

        private void ValidateArrayResult( 
            Regex groupExtractor, 
            Regex itemExtractor, 
            string text, 
            bool success,
            bool hasPrefix,
            string prefixSuffix, 
            bool remainderNonEmpty,
            Func<string, string> processItem,
            string[] results )
        {
            var groupMatch = groupExtractor.Match( text.Trim() );
            groupMatch.Success.Should().Be( success );

            if( !success )
                return;

            groupMatch.Groups.Count.Should().Be( 3 );

            string? remainder;

            if( hasPrefix )
            {
                groupMatch.Groups[ 1 ].Value.Trim().Should()
                    .Be( remainderNonEmpty ? prefixSuffix.Trim() : string.Empty );

                remainder = groupMatch.Groups[ 2 ].Value.Trim();
            }
            else
            {
                groupMatch.Groups[ 2 ].Value.Trim().Should()
                    .Be( remainderNonEmpty ? prefixSuffix.Trim() : string.Empty );

                remainder = groupMatch.Groups[ 1 ].Value.Trim();
            }

            if( !remainderNonEmpty )
                return;

            var matches = new List<string>();

            do
            {
                var itemMatch = itemExtractor.Match( remainder );
                itemMatch.Success.Should().BeTrue();

                matches.Add( processItem( itemMatch.Groups[ 1 ].Value ) );
                remainder = itemMatch.Groups.Count > 2 ? itemMatch.Groups[ 2 ].Value.Trim() : null;

            } while( !string.IsNullOrEmpty( remainder ) );

            matches.Count.Should().Be( results.Length );

            for( var idx = 0; idx < matches.Count; idx++ )
            {
                matches[ idx ].Should().Be( results[ idx ] );
            }
        }
    }
}
