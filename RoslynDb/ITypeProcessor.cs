using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ITypeProcessor : IRoslynProcessor<TypeProcessorContext>, IEquatable<ITypeProcessor>
    {
        Type SupportedType { get; }
    }
}