using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
    public interface ISharpObjectFactory
    {
        bool Load( ISymbol symbol, out SharpObjectInfo? result, bool createIfMissing = false );
    }
}