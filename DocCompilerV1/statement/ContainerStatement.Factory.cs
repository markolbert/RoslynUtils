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

using J4JSoftware.Logging;

namespace J4JSoftware.DocCompiler
{
    public class StatementFactory
    {
        private static readonly TokenType[] ContainerTokenTypes =
        {
            TokenType.ClassQualifier,
            TokenType.InterfaceQualifier,
            TokenType.NamespaceQualifier,
            TokenType.RecordQualifier,
            TokenType.StructQualifier
        };

        private readonly IJ4JLogger? _logger;

        public StatementFactory( IJ4JLogger? logger )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool CreateStatement(
            TokenCollection tokenCollection,
            ContainerStatement? parent,
            out Statement? result )
        {
            result = null;

            if( CreateContainerStatement( tokenCollection, parent, out var containerStatement ) )
            {
                result = containerStatement;
                return true;
            }

            return true;
        }

        public bool CreateContainerStatement( 
            TokenCollection tokenCollection,
            ContainerStatement? parent,
            out ContainerStatement? result )
        {
            result = null;
            var keywordIdx = -1;
            var typeToCreate = TokenType.Undefined;

            foreach( var containerTokenType in ContainerTokenTypes )
            {
                keywordIdx = tokenCollection.FindTokenTypeIndex( containerTokenType );

                if( keywordIdx < 0 )
                    continue;

                typeToCreate = containerTokenType;

                break;
            }

            if( keywordIdx < 0 )
            {
                _logger?.Error("Provided TokenCollection does not contain a container-type token");
                return false;
            }

            if( keywordIdx + 1 >= tokenCollection.Tokens.Count )
            {
                _logger?.Error("Provided TokenCollection does not contain a potential name token");
                return false;
            }

            if( tokenCollection.Tokens[ keywordIdx + 1 ].Type != TokenType.Text )
            {
                _logger?.Error("Provided TokenCollection does not contain a name token");
                return false;
            }

            switch( typeToCreate )
            {
                case TokenType.NamespaceQualifier:
                    result = new NamespaceStatement( parent )
                    {
                        Name = tokenCollection.Tokens[ keywordIdx + 1 ].Text
                    };

                    break;

                case TokenType.ClassQualifier when parent != null:
                    result = new ClassStatement( parent )
                    {
                        Accessibility = tokenCollection.GetAccessibility(),
                        IsSealed = tokenCollection.HasTokenType( TokenType.SealedQualifier ),
                        IsStatic = tokenCollection.HasTokenType( TokenType.StaticQualifier ),
                        Name = tokenCollection.Tokens[ keywordIdx + 1 ].Text
                    };

                    break;

                case TokenType.InterfaceQualifier when parent != null:
                    result = new InterfaceStatement( parent )
                    {
                        Accessibility = tokenCollection.GetAccessibility(),
                        Name = tokenCollection.Tokens[ keywordIdx + 1 ].Text
                    };

                    break;

                case TokenType.StructQualifier when parent != null:
                    result = new StructStatement( parent )
                    {
                        Accessibility = tokenCollection.GetAccessibility(),
                        IsSealed = tokenCollection.HasTokenType( TokenType.SealedQualifier ),
                        IsStatic = tokenCollection.HasTokenType( TokenType.StaticQualifier ),
                        Name = tokenCollection.Tokens[ keywordIdx + 1 ].Text
                    };

                    break;

                case TokenType.RecordQualifier when parent != null:
                    result = new RecordStatement( parent )
                    {
                        Accessibility = tokenCollection.GetAccessibility(),
                        IsSealed = tokenCollection.HasTokenType( TokenType.SealedQualifier ),
                        IsStatic = tokenCollection.HasTokenType( TokenType.StaticQualifier ),
                        Name = tokenCollection.Tokens[ keywordIdx + 1 ].Text
                    };

                    break;

            }

            if( result == null )
                _logger?.Error(
                    parent == null
                        ? "Container-type token '{0}' must be contained in a non-null container type but the supplied parent was undefined"
                        : "Unsupported container-type token '{0}'",
                    typeToCreate );
            else
            {
                AddAttributes( tokenCollection, result );
            }

            return result != null;
        }

        private void AddAttributes( TokenCollection tokenCollection, ContainerStatement statement )
        {
            foreach( var token in tokenCollection.Tokens )
            {
                if( token.Type != TokenType.AttributeStart )
                    break;

                var attrStatement = new AttributeStatement( statement ) { Name = token.Text };
                statement.Attributes.Add( attrStatement );
            }
        }
    }
}