namespace J4JSoftware.DocCompiler
{
    public class NamespaceContext
    {
        public NamespaceContext( Namespace nsDb )
        {
            Namespace = nsDb;
        }

        public Namespace Namespace { get; }

        public string Label =>Namespace.AliasedNamespace == null ? Namespace.FullyQualifiedName : Namespace.Name;

        public string NamespaceName => Namespace.AliasedNamespace == null
            ? Namespace.FullyQualifiedName
            : Namespace.AliasedNamespace.FullyQualifiedName;
    }
}
