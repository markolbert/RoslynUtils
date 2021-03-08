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

namespace Tests.RoslynWalker
{
    public class Statement
    {
        public static string[] PropertyMethodNames = new[] { "get", "set", "init" };

        private readonly List<Token> _tokens = new();
        private readonly List<Statement> _children = new();

        private bool _isModifiable = true;

        internal Statement( Statement? parent )
        {
            Parent = parent;
        }

        public bool IsModifiable => _isModifiable;

        public Statement? Parent { get; }
        public ReadOnlyCollection<Statement> Children => _children.AsReadOnly();
        public ReadOnlyCollection<Token> Tokens => _tokens.AsReadOnly();

        public Token? ActiveToken
        {
            get
            {
                if( !_isModifiable )
                    return null;

                var retVal = _tokens.LastOrDefault();

                if( retVal == null )
                    return null;

                return retVal.Children.Any() ? retVal.Children.Last() : retVal;
            }
        }

        public void AddToken( Token toAdd )
        {
            if( !_isModifiable )
                throw new ArgumentException( $"Trying to add {toAdd.Type} token to closed Statement" );

            _tokens.Add( toAdd );
        }

        public Statement AddChild()
        {
            if( !_isModifiable )
                throw new ArgumentException( "Statement is closed and can't be modified" );

            var retVal = new Statement( this );
            _children.Add( retVal );

            return retVal;
        }

        public void Close() =>_isModifiable = false;

        public StatementType Type
        {
            get
            {
                if( Tokens.Count == 0 )
                    return StatementType.Undefined;

                var firstToken = Tokens[ 0 ];

                if( firstToken.Type == TokenType.Preprocessor )
                    return StatementType.Preprocessor;

                foreach( var type in new[]
                {
                    StatementType.Using, StatementType.Namespace, StatementType.Class, StatementType.Interface, 
                    StatementType.Delegate, StatementType.Event, StatementType.Struct
                } )
                {
                    var keyword = Enum.GetName( type )!;

                    if( Tokens.Any( x => x.Type == TokenType.Text
                                       && x.Text.Equals( keyword, StringComparison.Ordinal )
                    ) )
                        return type;
                }

                // at this point if we encounter argument tokens we're a method
                // since the only other entity that has argument tokens is a delegate
                // and we've already checked for that via keyword
                if( Tokens.Any( x => x.Type == TokenType.Argument ) )
                    return StatementType.Method;

                // properties are recognized by examining the child statements of 
                // the property statement. If a statement is a property one of those
                // children have a text token matching an entry in PropertyMethodNames
                if( this.Children.Any( c => c.Tokens
                    .Any( n => n.Type == TokenType.Text
                               && PropertyMethodNames.Any( pmn =>
                                   pmn.Equals( n.Text, StringComparison.Ordinal ) ) ) ) )
                    return StatementType.Property;

                // we must be a field
                return StatementType.Field;
            }
        }
    }
}