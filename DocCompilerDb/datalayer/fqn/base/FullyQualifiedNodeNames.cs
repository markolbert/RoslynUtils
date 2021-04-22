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
            IJ4JLogger? logger
        )
            : base( idTokens, logger )
        {
        }

        public ResolvedNameState GetName( 
            SyntaxNode node, 
            out string? result, 
            bool includeTypeParams = true )
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
                SyntaxKind.Parameter => GetParameterName( node, out result),
                SyntaxKind.RecordDeclaration => GetNamedTypeName(node, out result),
                SyntaxKind.StructDeclaration => GetNamedTypeName(node, out result),
                SyntaxKind.TupleElement => GetTupleElementName( node, out result),
                SyntaxKind.UsingDirective => GetUsingName( node, out result),

                SyntaxKind.ParameterList => ResolvedNameState.MultiplyAmbiguous,
                SyntaxKind.TypeParameterList => ResolvedNameState.MultiplyAmbiguous,

                _ => unsupported()
            };

            ResolvedNameState unsupported()
            {
                Logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );

                if( ThrowOnUnsupported )
                    throw new SyntaxNodeException( "Unsupported SyntaxNode", null, nodeKind );

                return ResolvedNameState.Failed;
            }
        }

        protected override ResolvedNameState GetMethodName( SyntaxNode node, out string? result )
        {
            result = null;

            if( base.GetMethodName( node, out var name ) == ResolvedNameState.Failed )
                return ResolvedNameState.Failed;

            result = $"{GetMethodPath(node)}.{name}";

            return ResolvedNameState.FullyResolved;
        }

        private string GetMethodPath( SyntaxNode node )
        {
            if( !ValidateNode( node, SyntaxKind.MethodDeclaration ) )
                return string.Empty;

            // find our declaring node's DocumentedType
            var sb = new StringBuilder();

            var curNode = node.Parent;

            if( curNode == null
                || !SyntaxCollections.DocumentedTypeKinds.Any( x => curNode.IsKind( x ) ) )
            {
                Logger?.Error( "MethodDeclaration node is not contained withing a type node" );
                return string.Empty;
            }

            while( curNode != null )
            {
                if( base.GetNameInternal( curNode, out var curName ) == ResolvedNameState.Failed )
                {
                    Logger?.Error( "Could not get name for {0}", curNode.Kind() );
                    return string.Empty;
                }

                sb.Insert( 0, $"{curName}." );

                curNode = curNode.Parent;
            }

            return sb.ToString();
        }

        protected override ResolvedNameState GetNamespaceName( SyntaxNode node, out string? result )
        {
            result = null;

            if( base.GetNamespaceName( node, out var name ) == ResolvedNameState.Failed )
                return ResolvedNameState.Failed;

            result = $"{GetNamespacePath(node)}.{name}";

            return ResolvedNameState.FullyResolved;
        }

        private string GetNamespacePath( SyntaxNode node )
        {
            if( !ValidateNode( node, SyntaxKind.NamespaceDeclaration ) )
                return string.Empty;

            var sb = new StringBuilder();
            var curNode = node.Parent;

            while( curNode?.Kind() == SyntaxKind.NamespaceDeclaration )
            {
                // we call the base version of GetNamespaceName() because 
                // we don't want the fully-qualified name since that's what
                // we're building
                if( base.GetNamespaceName(curNode, out var curName ) == ResolvedNameState.Failed )
                {
                    Logger?.Error("Could not get name for NamespaceDeclaration");
                    return string.Empty;
                }

                sb.Insert( 0, $"{curName}." );

                curNode = curNode.Parent;
            }

            return sb.ToString();
        }

        protected override ResolvedNameState GetParameterName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !ValidateNode( node, SyntaxKind.Parameter ) )
                return ResolvedNameState.Failed;

            return base.GetParameterName( node, out result ) == ResolvedNameState.Failed
                ? ResolvedNameState.Failed
                : ResolvedNameState.Ambiguous;
        }

        //private IEnumerable<NamespaceContext> GetCodeFileNamespaces( SyntaxNode node, IScannedFile scannedFile )
        //{
        //    var cfDb = DbContext.CodeFiles
        //        .Include( x => x.OuterNamespaces )
        //        .FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

        //    if( cfDb == null )
        //        Logger?.Error<string>(
        //            "Could not find code file '{0}' in the database, outer namespaces not added",
        //            scannedFile.SourceFilePath );

        //    return cfDb?.GetNamespaceContext() ?? Enumerable.Empty<NamespaceContext>();
        //}

        //private IEnumerable<NamespaceContext> GetContainingTypeNamespaces( SyntaxNode node )
        //{
        //    // now find the namespace paths in the containing named type
        //    var curNode = node.Parent;

        //    while( curNode != null
        //           && !SyntaxCollections.DocumentedTypeKinds.Any( x => curNode.IsKind( x ) ) )
        //    {
        //        curNode = curNode.Parent;
        //    }

        //    if( curNode == null )
        //    {
        //        Logger?.Error( "Couldn't find a supported named type node containing the Parameter" );
        //        return Enumerable.Empty<NamespaceContext>();
        //    }

        //    if( base.GetNamedTypeName( curNode, out var simpleDtName ) == ResolvedNameState.Failed )
        //        return Enumerable.Empty<NamespaceContext>();

        //    var dtDb = DbContext.DocumentedTypes
        //        .Include( x => x.Namespace )
        //        .Include( x => x.Namespace!.ChildNamespaces )
        //        .FirstOrDefault( x => x.FullyQualifiedName == simpleDtName );

        //    return dtDb?.GetNamespaceContext() ?? Enumerable.Empty<NamespaceContext>();
        //}

        protected override ResolvedNameState GetNamedTypeName( SyntaxNode node, out string? result )
        {
            result = null;

            if( base.GetNamedTypeName( node, out var name ) == ResolvedNameState.Failed )
                return ResolvedNameState.Failed;

            result = $"{GetNamedTypePath(node)}.{name}";

            return ResolvedNameState.FullyResolved;
        }

        private string GetNamedTypePath( SyntaxNode node )
        {
            if( !SyntaxCollections.DocumentedTypeKinds.Any(node.IsKind) )
            {
                Logger?.Error( "SyntaxNode is not supported documented type node" );
                return string.Empty;
            }

            var sb = new StringBuilder();
            var curNode = node.Parent;

            if( curNode == null) 
            {
                Logger?.Error( "Named type node is not contained within a SyntaxNode" );
                return string.Empty;
            }

            while( curNode != null )
            {
                if( base.GetNameInternal( curNode, out var curName ) == ResolvedNameState.Failed )
                {
                    Logger?.Error( "Could not get name for {0}", curNode.Kind() );
                    return string.Empty;
                }

                sb.Insert( 0, $"{curName}." );

                curNode = curNode.Parent;
            }

            return sb.ToString();
        }

        protected override ResolvedNameState GetTupleElementName( SyntaxNode node, out string? result ) =>
            base.GetTupleElementName( node, out result ) == ResolvedNameState.Failed
                ? ResolvedNameState.Failed
                : ResolvedNameState.Ambiguous;

        private bool ValidateNode( SyntaxNode node, SyntaxKind kind )
        {
            if( node.IsKind( kind ) )
                return true;

            Logger?.Error("SyntaxNode is not a {0}", kind);

            return false;
        }
    }
}
