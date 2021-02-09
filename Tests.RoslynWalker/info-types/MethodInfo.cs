using System.Collections.Generic;
#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class MethodInfo
    {
        public string Name { get; set; }
        public List<string> Arguments { get; set; }
    }
}