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
using J4JSoftware.Logging;

namespace J4JSoftware.DocCompiler
{
    public abstract class EntityProcessor<TEntity> : IEntityProcessor
    {
        protected EntityProcessor(
            IFullyQualifiedNodeNames fqNamers,
            INodeNames namers,
            INodeIdentifierTokens nodeTokens,
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            FullyQualifiedNames = fqNamers;
            Names = namers;
            NodeIdentifierTokens = nodeTokens;
            DbContext = dbContext;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }
        protected DocDbContext DbContext { get; }
        protected IFullyQualifiedNodeNames FullyQualifiedNames { get; }
        protected INodeNames Names { get; }
        protected INodeIdentifierTokens NodeIdentifierTokens { get; }

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

        public bool Equals( IEntityProcessor? other )
        {
            if( other == null )
                return false;

            return other == this;
        }
    }
}