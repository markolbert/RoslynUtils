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
using System.Linq;

namespace J4JSoftware.DocCompiler
{
    public class ActiveTokenEvolver : IActiveTokenEvolver
    {
        private readonly List<ITokenEvolver> _evolvers = new();

        private readonly Type[] _convTypes =
        {
            typeof(TokenClosingInfo), 
            typeof(TokenConversionInfo), 
            typeof(TokenModificationInfo)
        };

        public ActiveTokenEvolver()
        {
            _evolvers.Add( new TokenCloser( "\r", includeToken: t => t.Type != TokenType.MultiLineComment ) );
            _evolvers.Add( new TokenCloser( "\n", includeToken: t => t.Type != TokenType.MultiLineComment ) );

            _evolvers.Add( new TokenCloser( "{", TokenType.BlockStart, includeToken:t=>!t.IsCommentType() ) );
            _evolvers.Add( new TokenCloser( "}", TokenType.BlockEnd, includeToken:t=>!t.IsCommentType() ) );

            _evolvers.Add( new TokenLeadingConverter( "/*", TokenType.MultiLineComment ) );
            _evolvers.Add( new TokenCloser( "*/", includeToken: t => t.Type == TokenType.MultiLineComment ) );
            
            //_evolvers.Add( new TokenLeadingConverter( "[", TokenType.BracketStart ) );
            //_evolvers.Add( new TokenCloser( "]", TokenType.BracketEnd,t=>!t.IsCommentType() ) );

            _evolvers.Add( new TokenCloser( "\t", includeToken:t=>!t.IsCommentType() ) );
            _evolvers.Add( new TokenCloser( ":", includeToken:t=>!t.IsCommentType() ) );
            _evolvers.Add( new TokenCloser( " ", includeToken:t=>!t.IsCommentType() ) );
            _evolvers.Add( new TokenCloser( ",", TokenType.Argument, includeToken:t=>!t.IsCommentType() ) );
            _evolvers.Add( new TokenCloser( ")", TokenType.ArgumentListEnd, includeToken:t=>!t.IsCommentType() ) );
            _evolvers.Add( new TokenCloser( ">", TokenType.TypeArgumentEnd, includeToken:t=>!t.IsCommentType() ) );

            _evolvers.Add( new TokenLeadingConverter( "(", TokenType.ArgumentListStart ) );
            _evolvers.Add( new TokenLeadingConverter( "//", TokenType.SingleLineComment, false ) );
            _evolvers.Add( new TokenLeadingConverter( "///", TokenType.XmlComment ) );
            _evolvers.Add( new TokenLeadingConverter( "#", TokenType.Preprocessor ) );
            _evolvers.Add( new TokenLeadingConverter( "<", TokenType.TypeArgumentStart ) );

            _evolvers.Add( new TokenTextMatchesConverter( "public", TokenType.PublicAccess ) );
            _evolvers.Add( new TokenTextMatchesConverter( "private", TokenType.PrivateAccess ) );
            _evolvers.Add( new TokenTextMatchesConverter( "protected", TokenType.ProtectedAccess ) );
            _evolvers.Add( new TokenTextMatchesConverter( "internal", TokenType.InternalAccess ) );

            _evolvers.Add( new TokenTextMatchesConverter( "in", TokenType.InArgumentQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "out", TokenType.OutArgumentQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "ref", TokenType.RefArgumentQualifier ) );

            _evolvers.Add( new TokenTextMatchesConverter( "readonly", TokenType.ReadOnlyQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "sealed", TokenType.SealedQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "static", TokenType.StaticQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "where", TokenType.WhereClause ) );

            _evolvers.Add( new TokenTextMatchesConverter( "get", TokenType.Property ) );
            _evolvers.Add( new TokenTextMatchesConverter( "set", TokenType.Property ) );
            _evolvers.Add( new TokenTextMatchesConverter( "init", TokenType.Property ) );

            _evolvers.Add( new TokenTextMatchesConverter( "class", TokenType.ClassQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "delegate", TokenType.DelegateQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "event", TokenType.EventQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "interface", TokenType.InterfaceQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "namespace", TokenType.NamespaceQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "struct", TokenType.StructQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "using", TokenType.UsingQualifier ) );

            _evolvers.Add( new TokenTextMatchesConverter( "virtual", TokenType.VirtualQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "override", TokenType.OverrideQualifier ) );
            _evolvers.Add( new TokenTextMatchesConverter( "new", TokenType.NewQualifier ) );

            _evolvers.Add( new TokenTrailingModifier( "//", t => t.Type == TokenType.SingleLineComment ) );
            _evolvers.Add( new TokenTrailingModifier( "\r", " " ) );
            _evolvers.Add( new TokenTrailingModifier( "\n", " " ) );
            _evolvers.Add( new TokenTrailingModifier( "\t", " " ) );
            _evolvers.Add( new TokenTrailingModifier( "  ", " " ) );
        }

        public bool EvolveActiveToken( TokenCollection tokenCollection, out TokenEvolutionInfo? result )
        {
            result = null;

            foreach( var resultType in _convTypes )
            {
                if( !RunEvolvers( _evolvers.Where( m => m.ResultType == resultType ),
                    tokenCollection,
                    out var temp ) ) 
                    continue;

                result = temp;
                break;
            }

            return result != null;
        }

        private bool RunEvolvers( IEnumerable<ITokenEvolver> evolvers, TokenCollection tokenCollection, out TokenEvolutionInfo? result )
        {
            result = null;

            foreach( var evolver in evolvers )
            {
                if( !evolver.Matches( tokenCollection, out var innerResult ) ) 
                    continue;

                result = innerResult;
                break;
            }

            return result != null;
        }
    }
}