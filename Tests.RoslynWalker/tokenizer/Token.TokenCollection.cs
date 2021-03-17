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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;

namespace Tests.RoslynWalker
{
    public partial class Token
    {
        public class TokenCollection
        {
            private readonly List<Token> _tokens = new();
            private readonly List<TokenCollection> _children = new();

            private readonly IActiveTokenEvolver _evolver;
            private readonly ITokenCollectionFactory _tokenCollectionFactory;

            private readonly IJ4JLogger? _logger;

            public TokenCollection(
                IActiveTokenEvolver evolver,
                TokenCollection? parent,
                ITokenCollectionFactory tokenCollectionFactory,
                IJ4JLogger? logger
            )
            {
                _evolver = evolver;
                Parent = parent;
                _tokenCollectionFactory = tokenCollectionFactory;

                IsModifiable = true;

                _logger = logger;
                _logger?.SetLoggedType( GetType() );
            }

            public bool IsModifiable { get; private set; }

            public TokenCollection? Parent { get; }
            public ReadOnlyCollection<TokenCollection> Children => _children.AsReadOnly();
            public ReadOnlyCollection<Token> Tokens => _tokens.AsReadOnly();

            public Token? GetActiveToken( bool createIfNecessary = false )
            {
                // get the last token we contain or null if we have no tokens
                var lastToken = _tokens.LastOrDefault();

                // if there are no tokens in this tokenCollection, create one
                if( lastToken == null )
                    return create_new( createIfNecessary );

                // if there are tokens in this tokenCollection, see if lastToken has children.
                // if it does, and the child can accept text, return it
                var furthestChild = lastToken.Children.LastOrDefault();

                if( furthestChild?.CanAcceptText ?? false )
                    return furthestChild;

                // there are no children which can accept text

                // if the lastToken can accept text return it. Otherwise, create
                // a new token and return it.
                return lastToken.CanAcceptText ? lastToken : create_new( true );

                Token? create_new( bool trueToCreate )
                {
                    if( !trueToCreate )
                        return null;

                    var newToken = new Token( this, null, TokenType.Text );
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

            public int FindTokenTypeIndex( TokenType type ) => _tokens.FindIndex( t => t.Type == type );

            private void AddToken( TokenType type = TokenType.Text, string? initialText = null )
            {
                if( !IsModifiable )
                    throw new ArgumentException( $"Trying to add token to closed TokenCollection" );

                _tokens.Add( new Token( this, null, type, initialText ) );
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

            public TokenCollection AddChild()
            {
                if( !IsModifiable )
                    throw new ArgumentException( "TokenCollection is closed and can't be modified" );

                var retVal = _tokenCollectionFactory.CreateTokenCollection( this );
                _children.Add( retVal );

                return retVal;
            }

            public StatementType Type
            {
                get
                {
                    // if there aren't any tokens we can't deduce what we might become :)
                    if( _tokens.Count == 0 )
                        return StatementType.Undefined;

                    if( !IsNonCodingStatement( out var nonCodingType ) )
                        return nonCodingType!.Value;

                    if( !IsKeywordStatement( out var keywordType ) )
                        return keywordType!.Value;

                    // check for methods by looking for an argument list...being careful
                    // to not be tricked by tuples. Method argument lists are always the child
                    // of another token
                    if( _tokens.Any( t => t.Type == TokenType.ArgumentList && t.Parent != null ) )
                        return StatementType.Method;

                    if( Tokens.Any( x => x.Type == TokenType.Property ) )
                        return StatementType.Property;

                    // we must be a field
                    return StatementType.Field;
                }
            }

            private bool IsNonCodingStatement( out StatementType? result )
            {
                result = _tokens[ 0 ].Type switch
                {
                    TokenType.MultiLineComment => (StatementType?) StatementType.Comment,
                    TokenType.Preprocessor => (StatementType?) StatementType.Preprocessor,
                    TokenType.SingleLineComment => (StatementType?) StatementType.Comment,
                    _ => null
                };

                return result != null;
            }

            private bool IsKeywordStatement( out StatementType? result )
            {
                result = _tokens
                    .Select( t =>
                    {
                        return t.Type switch
                        {
                            TokenType.ClassQualifier => (StatementType?) StatementType.Class,
                            TokenType.DelegateQualifier => StatementType.Delegate,
                            TokenType.EventQualifier => StatementType.Event,
                            TokenType.InterfaceQualifier => StatementType.Interface,
                            TokenType.NamespaceQualifier => StatementType.Namespace,
                            TokenType.RecordQualifier => StatementType.Record,
                            TokenType.StructQualifier => StatementType.Struct,
                            TokenType.UsingQualifier => StatementType.Using,
                            _ => null
                        };
                    } )
                    .FirstOrDefault( x => x != StatementType.Undefined );

                return result != null;
            }

            private void EvolveToken()
            {
                if( !_evolver.EvolveActiveToken( this, out var evolveInfo ) )
                    return;

                switch( evolveInfo )
                {
                    case TokenClosingInfo closingInfo:
                        CloseActiveToken(closingInfo);
                        break;

                    case TokenConversionInfo convInfo:
                        ConvertActiveToken(convInfo);
                        break;

                    case TokenModificationInfo modInfo:
                        ModifyActiveToken( modInfo );
                        break;

                    case TokenSpawnInfo spawnInfo:
                        SpawnActiveToken( spawnInfo );
                        break;
                }
            }

            private void CloseActiveToken( TokenClosingInfo closingInfo )
            {
                // we don't reference ActiveToken in this block because as soon as we 
                // close a token ActiveToken will return a new one
                switch( closingInfo.ClosingAction )
                {
                    case TokenClosingAction.CloseToken:
                        closingInfo.OriginalToken.CanAcceptText = false;
                        closingInfo.OriginalToken.ReplaceText( closingInfo.NewToken.Text );

                        break;

                    case TokenClosingAction.CloseTokenAndStatement:
                        closingInfo.OriginalToken.CanAcceptText = false;
                        closingInfo.OriginalToken.ReplaceText( closingInfo.NewToken.Text );

                        IsModifiable = false;

                        break;
                }
            }

            private void ConvertActiveToken( TokenConversionInfo convInfo )
            {
                var targetToken = convInfo.RelativePosition switch
                {
                    TokenRelativePosition.Parent => convInfo.OriginalToken.Parent,
                    TokenRelativePosition.Self => convInfo.OriginalToken,
                    _ => null
                };

                if( targetToken == null )
                {
                    _logger?.Error<string>(
                        "Trying to convert a child token, which is not allowed (active token text is '{0}')",
                        convInfo.OriginalToken.Text );
                    return;
                }

                if( convInfo.TextChanged )
                    targetToken.ReplaceText( convInfo.NewToken.Text );

                targetToken.Type = convInfo.NewToken.Type;
            }

            private void ModifyActiveToken( TokenModificationInfo modInfo )
            {
                modInfo.OriginalToken.ReplaceText( modInfo.NewToken.Text );

                switch( modInfo.ClosingAction )
                {
                    case TokenClosingAction.CloseToken:
                        modInfo.OriginalToken.CanAcceptText = false;
                        break;

                    case TokenClosingAction.CloseTokenAndStatement:
                        modInfo.OriginalToken.CanAcceptText = false;
                        IsModifiable = false;
                        break;
                }
            }

            private void SpawnActiveToken( TokenSpawnInfo spawnInfo )
            {
                spawnInfo.OriginalToken!.ReplaceText( spawnInfo.NewToken.Text );

                switch( spawnInfo.SpawnedPosition )
                {
                    case TokenRelativePosition.Child:
                        spawnInfo.OriginalToken.AddChild( spawnInfo.SpawnedToken.Type, spawnInfo.SpawnedToken.Text );
                        break;

                    case TokenRelativePosition.Self:
                        AddToken( spawnInfo.SpawnedToken.Type, spawnInfo.SpawnedToken.Text );
                        break;
                }
            }
        }
    }
}