using System.Collections.Generic;
#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class GenericMethodInfo : MethodInfo
    {
        public List<string> TypeArguments { get; set; }
    }
}