using System.Collections.Generic;

namespace Tests.RoslynWalker
{
    public interface ICodeElementTypeArguments
    {
        List<string> TypeArguments { get; }
    }
}