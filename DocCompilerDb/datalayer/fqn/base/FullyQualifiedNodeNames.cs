using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class FullyQualifiedNodeNames : NodeNamesBase, IFullyQualifiedNodeNames
    {
        public FullyQualifiedNodeNames(
            INodeIdentifierTokens idTokens,
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        :base(idTokens, dbContext, logger)
        {
        }

        public bool GetName( SyntaxNode node, out List<string>? result, bool includeTypeParams = true )
        {
            result = null;

            IncludeTypeParameters = includeTypeParams;

            var nodeKind = node.Kind();

            // tuples are handled differently because they derive from/depend on other types
            return nodeKind switch
            {
                SyntaxKind.ClassDeclaration => GetNamedTypeName(node, out result),
                SyntaxKind.InterfaceDeclaration => GetNamedTypeName(node, out result),
                SyntaxKind.MethodDeclaration => GetMethodName( node, out result ),
                SyntaxKind.NamespaceDeclaration => GetNamespaceName(node, out result),
                //SyntaxKind.ParameterList => GetParameterListName( node, out result),
                //SyntaxKind.Parameter => GetParameterName( node, out result),
                SyntaxKind.RecordDeclaration => GetNamedTypeName(node, out result),
                SyntaxKind.StructDeclaration => GetNamedTypeName(node, out result),
                SyntaxKind.TupleElement => GetTupleElementName( node, out result),
                SyntaxKind.TupleType => GetTupleTypeName( node, out result),
                //SyntaxKind.TypeParameterList => GetTypeParameterListName(node, out result),
                //SyntaxKind.UsingDirective => GetUsingName( node, out result),

                _ => unsupported()
            };

            bool unsupported()
            {
                Logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );

                if( ThrowOnUnsupported )
                    throw new SyntaxNodeException( "Unsupported SyntaxNode", null, nodeKind );

                return false;
            }
        }

        private bool GetMethodName( SyntaxNode node, out List<string>? result )
        {
            result = null;

            if( !base.GetMethodName( node, out var name ) )
                return false;

            result = GetMethodPaths( node ).Select( x => $"{x}.{name}" ).ToList();

            return result.Any();
        }

        private List<string> GetMethodPaths( SyntaxNode node )
        {
            var retVal = new List<string>();

            if( !node.IsKind(SyntaxKind.MethodDeclaration  ))
            {
                Logger?.Error( "SyntaxNode is not a MethodDeclaration" );
                return retVal;
            }

            // find our declaring node's DocumentedType
            var sb = new StringBuilder();

            var curNode = node.Parent;

            if( curNode == null
                || !SyntaxCollections.DocumentedTypeKinds.Any( x => curNode.IsKind( x ) ) )
            {
                Logger?.Error( "MethodDeclaration node is not contained withing a type node" );
                return retVal;
            }

            while( curNode != null )
            {
                if( !base.GetNameInternal( curNode, out var curName ) )
                {
                    Logger?.Error( "Could not get name for {0}", curNode.Kind() );
                    return retVal;
                }

                sb.Insert( 0, $"{curName}." );

                curNode = curNode.Parent;
            }

            retVal.Add(sb.ToString());

            return retVal;
        }

        private bool GetNamespaceName( SyntaxNode node, out List<string>? result )
        {
            result = null;

            if( !base.GetNamespaceName( node, out var name ) )
                return false;

            result = GetNamespacePaths( node ).Select( x => $"{x}.{name}" ).ToList();

            return result.Any();
        }

        private List<string> GetNamespacePaths( SyntaxNode node )
        {
            var retVal = new List<string>();

            if( !node.IsKind(SyntaxKind.NamespaceDeclaration  ))
            {
                Logger?.Error( "SyntaxNode is not a NamespaceDeclaration" );
                return retVal;
            }

            var sb = new StringBuilder();
            var curNode = node.Parent;

            while( curNode?.Kind() == SyntaxKind.NamespaceDeclaration )
            {
                // we call the base version of GetNamespaceName() because 
                // we don't want the fully-qualified name since that's what
                // we're building
                if( !base.GetNamespaceName(curNode, out var curName ) )
                {
                    Logger?.Error("Could not get name for NamespaceDeclaration");
                    return retVal;
                }

                sb.Insert( 0, $"{curName}." );

                curNode = curNode.Parent;
            }

            retVal.Add(sb.ToString());

            return retVal;
        }

        private bool GetNamedTypeName( SyntaxNode node, out List<string>? result )
        {
            result = null;

            if( !base.GetNamedTypeName( node, out var name ) )
                return false;

            result = GetNamedTypePaths( node ).Select( x => $"{x}.{name}" ).ToList();

            return result.Any();
        }

        private List<string> GetNamedTypePaths( SyntaxNode node )
        {
            var retVal = new List<string>();

            if( !SyntaxCollections.DocumentedTypeKinds.Any(node.IsKind) )
            {
                Logger?.Error( "SyntaxNode is not supported documented type node" );
                return retVal;
            }

            var sb = new StringBuilder();
            var curNode = node.Parent;

            if( curNode == null) 
            {
                Logger?.Error( "Named type node is not contained within a SyntaxNode" );
                return retVal;
            }

            while( curNode != null )
            {
                if( !base.GetNameInternal( curNode, out var curName ) )
                {
                    Logger?.Error( "Could not get name for {0}", curNode.Kind() );
                    return retVal;
                }

                sb.Insert( 0, $"{curName}." );

                curNode = curNode.Parent;
            }

            retVal.Add(sb.ToString());

            return retVal;
        }

        private bool GetTupleElementName( SyntaxNode node, out List<string>? result )
        {
            result = null;

            if( !base.GetTupleElementName( node, out var name ) )
                return false;

            result = GetTupleElementPaths( node ).Select( x => $"{x}.{name}" ).ToList();

            return result.Any();
        }

        private List<string> GetTupleElementPaths( SyntaxNode node ) => new List<string>();

        private bool GetTupleTypeName( SyntaxNode node, out List<string>? result )
        {
            result = null;

            if( !base.GetTupleTypeName( node, out var name ) )
                return false;

            result = GetTupleTypePaths( node ).Select( x => $"{x}.{name}" ).ToList();

            return result.Any();
        }

        private List<string> GetTupleTypePaths( SyntaxNode node ) => new List<string>();

    }
}
