using System;

namespace J4JSoftware.Roslyn
{
    [Flags]
    public enum CompilationReferenceType
    {
        AssemblyName = 1 << 0,
        FileSystem = 1 << 1,

        All = AssemblyName | FileSystem,
        None = 0
    }
}