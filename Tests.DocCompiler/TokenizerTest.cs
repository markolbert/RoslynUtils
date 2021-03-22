using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Tests.DocCompiler
{
    public class TokenizerTest
    {
        [ Theory ]
        [ MemberData( nameof(TokenizerMemberData.GetTokenizerData), MemberType = typeof(TokenizerMemberData) ) ]
        public void Test1( string source, bool success, List<TokenData> tokens )
        {
            //var tokenizer = CompositionRoot.Default.Tokenizer;

            //tokenizer.TokenizeText( source, out var tokenCollection )
            //    .Should().Be( success );

            //if( !success )
            //    return;

            //tokenCollection.Should().NotBeNull();

            //tokenCollection!.Count.Should().Be( tokens.Count );

            //for( var idx = 0; idx < tokenCollection.Count; idx++ )
            //{
            //    var parsedToken = tokenCollection.Tokens[ idx ];
            //    var targetToken = tokens[ idx ];

            //    parsedToken.Type.Should().Be( targetToken.Type );
            //    parsedToken.Text.Should().Be( targetToken.Text );
            //}
        }
    }
}
