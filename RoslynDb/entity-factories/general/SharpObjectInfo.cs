using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SharpObjectInfo
    {
        public ISymbol Symbol { get; internal set; }
        public string FullyQualifiedName { get; internal set; }
        public string Name { get; internal set; }
        public SharpObject SharpObject { get; internal set; }
        public SharpObjectType Type { get; internal set; }
        public bool IsNew { get; internal set; }
    }
}