using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISharpObjectTypeMapper
    {
        SharpObjectType this[ ISymbol symbol ] { get; }
        SharpObjectType this[ Type entityType ] { get; }
    }
}