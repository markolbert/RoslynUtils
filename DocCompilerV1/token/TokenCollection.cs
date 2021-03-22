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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.DocCompiler
{
    public class TokenCollection : IEnumerable<Token>
    {
        private readonly List<Token> _tokens = new();
        private readonly IActiveTokenEvolver _evolver;

        private readonly IJ4JLogger? _logger;

        public TokenCollection(
            IActiveTokenEvolver evolver,
            IJ4JLogger? logger
        )
        {
            _evolver = evolver;

            IsModifiable = true;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsModifiable { get; private set; }

        public ReadOnlyCollection<Token> Tokens => _tokens.AsReadOnly();
        public int Count => _tokens.Count;

        public Token? GetActiveToken( bool createIfNecessary = false )
        {
            // get the last token we contain or null if we have no tokens
            var lastToken = _tokens.LastOrDefault();

            // if there are no tokens in this tokenCollection, create one
            if( lastToken == null || !lastToken.CanAcceptText )
                return create_new( createIfNecessary );

            // if the lastToken can accept text return it. Otherwise, create
            // a new token and return it.
            return lastToken;

            Token? create_new( bool trueToCreate )
            {
                if( !trueToCreate )
                    return null;

                var newToken = new Token( this );
                _tokens.Add( newToken );

                return newToken;
            }
        }

        public AccessQualifier GetAccessibility()
        {
            var hasProtected = HasTokenType( TokenType.ProtectedAccess );
            var hasInternal = HasTokenType( TokenType.InternalAccess );

            if( hasInternal && hasProtected )
                return AccessQualifier.ProtectedInternal;

            if( hasInternal )
                return AccessQualifier.Internal;

            if( hasProtected )
                return AccessQualifier.Protected;

            return HasTokenType( TokenType.PublicAccess ) ? AccessQualifier.Public : AccessQualifier.Private;
        }

        public bool HasTokenType( TokenType type ) => Tokens.Any( t => t.Type == type );
        public bool InTokenTypes( IEnumerable<TokenType> types ) => types.Any( HasTokenType );

        public int FindTokenTypeIndex( TokenType type ) => _tokens.FindIndex( t => t.Type == type );

        private void AddToken( TokenType type = TokenType.Text, string? initialText = null )
        {
            if( !IsModifiable )
                throw new ArgumentException( $"Trying to add token to closed TokenCollection" );

            _tokens.Add( new Token( this, type, initialText ) );
        }

        public bool AddChar( char toAdd )
        {
            if( !IsModifiable )
            {
                _logger?.Error( "Trying to add character to closed TokenCollection" );
                return false;
            }

            var token = GetActiveToken( true );

            token!.AddChar( toAdd );

            EvolveToken();

            return true;
        }

        private void EvolveToken()
        {
            if( !_evolver.EvolveActiveToken( this, out var evolveInfo ) )
                return;

            switch( evolveInfo )
            {
                case TokenClosingInfo closingInfo:
                    CloseActiveToken( closingInfo );
                    break;

                case TokenConversionInfo convInfo:
                    ConvertActiveToken( convInfo );
                    break;

                case TokenModificationInfo modInfo:
                    ModifyActiveToken( modInfo );
                    break;
            }
        }

        private void CloseActiveToken( TokenClosingInfo closingInfo )
        {
            // we don't reference ActiveToken in this block because as soon as we 
            // close a token ActiveToken will return a new one
            if( closingInfo.CloseToken )
            {
                closingInfo.OriginalToken.CanAcceptText = false;
                closingInfo.OriginalToken.ReplaceText( closingInfo.NewToken.Text );
            }

            closingInfo.OriginalToken.Type = closingInfo.NewToken.Type;
        }

        private void ConvertActiveToken( TokenConversionInfo convInfo )
        {
            if( convInfo.TextChanged )
                convInfo.OriginalToken.ReplaceText( convInfo.NewToken.Text );

            convInfo.OriginalToken.Type = convInfo.NewToken.Type;
        }

        private void ModifyActiveToken( TokenModificationInfo modInfo )
        {
            modInfo.OriginalToken.ReplaceText( modInfo.NewToken.Text );

            if( modInfo.CloseToken )
                modInfo.OriginalToken.CanAcceptText = false;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            foreach( var token in _tokens )
            {
                yield return token;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}