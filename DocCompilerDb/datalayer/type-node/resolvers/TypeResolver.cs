#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerDb' is free software: you can redistribute it
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
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.DocCompiler
{
    public abstract class TypeResolver<T> : ITypeResolver<T> 
        where T : NamedType
    {
        protected TypeResolver(
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
        protected List<NamespaceContext>? NamespaceContexts { get; private set; }
        protected bool CreateIfMissing { get; private set; }

        public bool FindType( 
            ITypeNodeAnalyzer analyzer, 
            TypeReferenceInfo typeInfo, 
            NamedType ntDb,
            out T? result )
        {
            if( ntDb is DocumentedType dtDb )
                NamespaceContexts = dtDb.GetNamespaceContext( DbContext, analyzer.CodeFileNamespaceContexts );
            else NamespaceContexts = analyzer.CodeFileNamespaceContexts;

            CreateIfMissing = analyzer.CreateIfMissing;

            return FindTypeOrTuple( typeInfo, ntDb, out result );
        }

        private bool FindTypeOrTuple( TypeReferenceInfo typeInfo, NamedType ntDb, out T? result )
        {
            result = null;

            if( typeInfo.IsTuple )
            {
                if( FindTupleType( typeInfo, ntDb, out var tupleResult ) )
                {
                    result = tupleResult;
                    return true;
                }

                return false;
            }

            return FindTypeInternal( typeInfo, ntDb, out result );
        }

        protected abstract bool FindTypeInternal( TypeReferenceInfo typeInfo, NamedType ntDb, out T? result );

        private bool FindTupleType( TypeReferenceInfo typeInfo, NamedType ntDb, out T? result )
        {
            result = null;

            if( !typeInfo.IsTuple )
                return false;

            var elementTypes = new List<NamedType>();

            // tuples are defined by having the same list of types
            var tuples = DbContext.TupleTypes
                .Include( x => x.TupleElements )
                .Include( x => x.TupleElements!.Select( y => y.ReferencedType ) )
                .Where( x => x.TupleElements != null && x.TupleElements.Count == typeInfo.Arguments.Count )
                .ToList();

            foreach( var tuple in tuples )
            {
                var allMatch = true;

                var elements = tuple.TupleElements!.ToList();

                for( var idx = 0; idx < elements.Count; idx++ )
                {
                    if( !FindTypeOrTuple( typeInfo.Arguments[ idx ], ntDb, out var elementType ) )
                    {
                        Logger?.Error<string>( "Could not resolve tuple element '{0}'",
                            typeInfo.Arguments[ idx ].Name );
                        return false;
                    }

                    if( elements[ idx ].ReferencedType.Name == elementType!.Name ) 
                        continue;

                    allMatch = false;
                    break;
                }

                if( !allMatch ) 
                    continue;

                result = tuple as T;
                break;
            }

            return true;
        }

        bool ITypeResolver.FindType( ITypeNodeAnalyzer analyzer, TypeReferenceInfo typeInfo, NamedType ntDb, out NamedType? result )
        {
            result = null;

            if( !FindType( analyzer, typeInfo, ntDb, out var innerResult ) )
                return false;

            if( innerResult is not NamedType temp )
            {
                Logger?.Error("TypeResolver did not produce a NamedType-derived result");
                return false;
            }

            result = temp;
            
            return true;
        }

        public bool Equals( ITypeResolver? other )
        {
            if( other == null )
                return false;

            return other == this;
        }
    }
}