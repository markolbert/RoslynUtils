using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public interface IIdentifier
    {
        string Name { get; }
    }

    public class BasicIdentifier : IIdentifier
    {
        private readonly List<SyntaxToken> _idTokens;

        public BasicIdentifier( params SyntaxToken[] idTokens )
        {
            if( idTokens.Length == 0 )
                throw new ArgumentException( $"Empty {nameof(SyntaxToken)}[] list" );

            _idTokens = new List<SyntaxToken>( idTokens );
        }

        public string Name => string.Join( " ", _idTokens );
    }
}
