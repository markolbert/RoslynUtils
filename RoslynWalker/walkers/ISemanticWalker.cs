using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISemanticWalker : IEquatable<ISemanticWalker>
    {
        Type SymbolType { get; }
    }
}
