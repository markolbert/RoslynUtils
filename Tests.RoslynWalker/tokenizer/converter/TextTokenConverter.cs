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
using System.Text;

namespace Tests.RoslynWalker
{
    public class TextTokenConverter
    {
        protected TextTokenConverter()
        {
        }

        protected string? OpeningMatch( string text, params string[] openers )
        {
            foreach( var opener in openers )
            {
                var location = text.IndexOf( opener, StringComparison.OrdinalIgnoreCase );

                if( location == 0 )
                    return opener;
            }

            return null;
        }

        protected TokenConversionInfo ConvertActiveToken( 
            Token.Statement statement, 
            TokenType convertedType, 
            params string[] openers )
        {
            var token = statement.GetActiveToken( true )!;

            if( token.Type != TokenType.Text || !token.CanAcceptText )
                return new TokenConversionInfo(token);

            var openerLength = OpeningMatch( token.Text, openers );

            return openerLength == null 
                ? new TokenConversionInfo(token) 
                : new TokenConversionInfo(token, convertedType, string.Empty );
        }

        protected TokenConversionInfo ConvertActiveTokenToQualifier<TQualifier>( 
            Token.Statement statement, 
            TokenType convertedType,
            TokenRelativePosition relativePosition = TokenRelativePosition.Self )
            where TQualifier: struct, Enum
        {
            var token = statement.GetActiveToken( true )!;

            if( token.Type != TokenType.Text || !token.CanAcceptText )
                return new TokenConversionInfo( token );

            return !TokenizerExtensions.IsEnumText<TQualifier>( token.Text, out var _ )
                ? new TokenConversionInfo( token )
                : new TokenConversionInfo( token, convertedType, relativePosition: relativePosition );
        }

        protected TokenConversionInfo ConvertKeywordToken( Token.Statement statement )
        {
            var token = statement.GetActiveToken( true )!;

            if( !token.CanAcceptText )
                return new TokenConversionInfo(token);

            var qualifier = ParseQualifierText<NamedClauseQualifier>( token.Text );

            if( qualifier == null )
                return new TokenConversionInfo(token);

            var newType = qualifier switch
            {
                NamedClauseQualifier.Class => TokenType.ClassQualifier,
                NamedClauseQualifier.Delegate => TokenType.DelegateQualifier,
                NamedClauseQualifier.Event => TokenType.EventQualifier,
                NamedClauseQualifier.Interface => TokenType.InterfaceQualifier,
                NamedClauseQualifier.Namespace=> TokenType.NamespaceQualifier,
                NamedClauseQualifier.Struct => TokenType.StructQualifier,
                NamedClauseQualifier.Using => TokenType.UsingQualifier,
                _ => TokenType.Undefined
            };

            return newType == TokenType.Undefined 
                ? new TokenConversionInfo(token) 
                : new TokenConversionInfo( token, newType );
        }

        protected TQualifier? ParseQualifierText<TQualifier>( string text )
            where TQualifier: struct, Enum
        {
            TokenizerExtensions.IsEnumText<TQualifier>( text, out var retVal );

            return retVal;
        }
    }
}