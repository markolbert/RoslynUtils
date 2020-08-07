using System;

namespace J4JSoftware.Roslyn
{
    [Flags]
    public enum TypeParameterConstraint
    {
        Constructor = 1 << 0,
        NotNull = 1 << 1,
        Reference = 1 << 2,
        Unmanaged = 1 << 3,
        Value = 1 << 4,

        None = 0
    }
}
