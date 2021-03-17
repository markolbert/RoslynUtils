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
using System.Linq;

namespace Tests.RoslynWalker
{
    public static class TokenizerExtensions
    {
        public static bool IsEnumText<TEnum>( string text, out TEnum? result )
            where TEnum : struct, Enum
        {
            result = null;

            var enumText = Enum.GetNames<TEnum>()
                .FirstOrDefault( x => x.ToLower().Equals( text, StringComparison.Ordinal ) );

            if( string.IsNullOrEmpty( enumText ) )
                return false;

            result = Enum.Parse<TEnum>( enumText, false );

            return true;
        }

        public static bool IsContainerStatement( this StatementType type ) =>
            type switch
            {
                StatementType.Class => true,
                StatementType.Interface => true,
                StatementType.Struct => true,
                StatementType.Namespace => true,
                StatementType.Record => true,
                _ => false
            };

        public static TokenType GetContainerTokenType( this StatementType type ) =>
            type switch
            {
                StatementType.Class => TokenType.ClassQualifier,
                StatementType.Interface => TokenType.InternalAccess,
                StatementType.Struct => TokenType.StructQualifier,
                StatementType.Namespace => TokenType.NamespaceQualifier,
                StatementType.Record => TokenType.RecordQualifier,
                _ => TokenType.Undefined
            };

        public static bool HasTokenType( this Token.TokenCollection tokenCollection, TokenType type ) =>
            tokenCollection.Tokens.Any( t => t.Type == type );

        public static AccessQualifier GetAccessibility( this Token.TokenCollection tokenCollection )
        {
            var hasProtected = tokenCollection.HasTokenType( TokenType.ProtectedAccess );
            var hasInternal = tokenCollection.HasTokenType( TokenType.InternalAccess );

            if( hasInternal && hasProtected )
                return AccessQualifier.ProtectedInternal;

            if( hasInternal )
                return AccessQualifier.Internal;

            if( hasProtected )
                return AccessQualifier.Protected;

            return tokenCollection.HasTokenType( TokenType.PublicAccess ) ? AccessQualifier.Public : AccessQualifier.Private;
        }

        public static bool IsKeywordToken( this TokenType type )
        {
            return type switch
            {
                TokenType.ClassQualifier => true,
                TokenType.EventQualifier => true,
                TokenType.DelegateQualifier => true,
                TokenType.InArgumentQualifier => true,
                TokenType.InterfaceQualifier => true,
                TokenType.InternalAccess => true,
                TokenType.NamespaceQualifier => true,
                TokenType.NewQualifier => true,
                TokenType.OutArgumentQualifier => true,
                TokenType.OverrideQualifier => true,
                TokenType.PrivateAccess => true,
                TokenType.ProtectedAccess => true,
                TokenType.PublicAccess => true,
                TokenType.ReadOnlyQualifier => true,
                TokenType.RecordQualifier => true,
                TokenType.RefArgumentQualifier => true,
                TokenType.SealedQualifier => true,
                TokenType.StaticQualifier => true,
                TokenType.StructQualifier => true,
                TokenType.UsingQualifier => true,
                TokenType.VirtualQualifier => true,
                TokenType.WhereClause => true,
                _ => false
            };
        }

        public static bool IsRequiresBracesType( this TokenType type )
        {
            return type switch
            {
                TokenType.ClassQualifier => true,
                TokenType.InterfaceQualifier => true,
                TokenType.NamespaceQualifier => true,
                TokenType.RecordQualifier => true,
                TokenType.StructQualifier => true,
                _ => false
            };
        }

        public static bool IgnoresEOLCharacters( this TokenType type )
            => type.IsRequiresBracesType() || type == TokenType.MultiLineComment;

        public static int NumCharactersInText( string text, char toCheck ) => text.Count( x => x == toCheck );

        public static int NetDelimitersInText( string text, char opener, char closer ) =>
            NumCharactersInText( text, opener ) - NumCharactersInText( text, closer );

    }
}