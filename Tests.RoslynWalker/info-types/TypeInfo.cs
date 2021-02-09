using System.Collections.Generic;
#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class TypeInfo
    {
        public string Name { get; set; }
        public List<string> TypeArguments { get; set; }
        public bool IsClass { get; set; }
        public bool IsInterface {get; set; }
        public bool IsDelegate { get; set; }
    }
}