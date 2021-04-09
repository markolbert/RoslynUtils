using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.DocCompiler
{
    public class TypeFinder
    {
        private class TypeInfo
        {
            public TypeInfo( string name, int numGenericParameters )
            {
                Name = name;
                NumGenericParameters = numGenericParameters;
            }

            public string Name { get; }
            public int NumGenericParameters { get; }
        }

        private readonly DocDbContext _dbContext;
        private readonly IJ4JLogger? _logger;

        public TypeFinder(
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        //public bool GetNamedType( SyntaxNode typeNode, List<DocumentedTypeUsing> usingContext, out NamedType? result, bool createIfMissing = true )
        //{
        //    result = null;

        //    if( !GetTypeInfo( typeNode, out var typeInfo ) )
        //        return false;

        //    if( FindDocumentedType( typeInfo!, usingContext, out var temp ) )
        //    {
        //        result = temp;
        //        return true;
        //    }

        //    return false;
        //}

        //private bool FindDocumentedType( 
        //    TypeInfo typeInfo, 
        //    List<DocumentedTypeUsing> nsContexts,
        //    out DocumentedType? result )
        //{
        //    result = null;

        //    var docTypes = new List<DocumentedType>();

        //    foreach( var nsContext in nsContexts )
        //    {
        //        docTypes.AddRange( _dbContext.DocumentedTypes
        //            .Include( x => x.TypeParameters )
        //            .Where( x =>
        //                x.FullyQualifiedName == $"{nsContext.UsingText}.{typeInfo!.Name}"
        //                && ( x.TypeParameters == null && typeInfo.NumGenericParameters == 0
        //                     || ( x.TypeParameters != null &&
        //                          x.TypeParameters.Count == typeInfo.NumGenericParameters ) )
        //            ) );
        //    }

        //    // if we find a match return it
        //    result = docTypes.FirstOrDefault( x => x.TypeParameters == null );

        //    if( result != null )
        //        return true;

        //    return false;
        //}

        private ExternalType CreateExternalType()
        {
            return new ExternalType();
        }

        private bool GetTypeInfo( SyntaxNode node, out TypeInfo? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.SimpleBaseType ) )
            {
                _logger?.Error( "SyntaxNode is not a SimpleBaseType" );
                return false;
            }

            if( HasChildNode( node, SyntaxKind.GenericName ))
            {
                if( !GetTypeArguments( node, out result ) )
                    return false;
            }
            else
            {
                var idNode = node.ChildNodes().FirstOrDefault( x => x.IsKind( SyntaxKind.IdentifierName ) );

                if( idNode == null )
                {
                    _logger?.Error("SyntaxNode is not a generic type but does not have an IdentifierName child node");

                    return false;
                }

                result = new TypeInfo( idNode.ToString(), 0 );
            }

            return true;
        }

        private bool HasChildNode( SyntaxNode node, SyntaxKind kind )
            => node.ChildNodes().Any( x => x.IsKind( kind ) );

        private bool GetTypeArguments( SyntaxNode node, out TypeInfo? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.GenericName ) )
                return false;

            var typeName = node.ChildTokens()
                .First( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Text;

            result = new TypeInfo( typeName, 
                node.ChildNodes().Count( x => 
                    x.IsKind( SyntaxKind.PredefinedType ) 
                    || x.IsKind( SyntaxKind.IdentifierName ) ) 
                );

            return true;
        }

    }
}
