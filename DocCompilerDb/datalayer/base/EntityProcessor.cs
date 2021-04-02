#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompiler' is free software: you can redistribute it
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public abstract class EntityProcessor<TEntity> : IEntityProcessor
    {
        protected EntityProcessor(
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            DbContext = dbContext;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }
        protected DocDbContext DbContext { get; }

        public virtual bool UpdateDb( IDocScanner source )
        {
            foreach( var node in GetNodesToProcess( source ) )
            {
                if( !ProcessEntity(node))
                    return false;
            }

            return true;
        }

        protected abstract IEnumerable<TEntity> GetNodesToProcess( IDocScanner source );
        protected abstract bool ProcessEntity( TEntity srcEntity );

        protected bool GetName( SyntaxNode node, out string? result )
        {
            result = null;

            // grab all the IdentifierToken child tokens as they contain the dotted
            // elements of the namespace's name
            var qualifiedName = node.ChildNodes()
                .FirstOrDefault(x => x.Kind() == SyntaxKind.QualifiedName);

            var identifierNodes = (qualifiedName ?? node).DescendantNodes()
                    .Where( x => x.Kind() == SyntaxKind.IdentifierName );

            result = string.Join(".", identifierNodes);

            return true;
        }

        public abstract bool GetFullyQualifiedName( TEntity srcEntity, out string? result );

        public bool Equals( IEntityProcessor? other )
        {
            if( other == null )
                return false;

            return other == this;
        }
    }
}