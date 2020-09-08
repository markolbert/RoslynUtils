using System;

namespace J4JSoftware.Roslyn
{
    [Flags]
    public enum DeclarationModifier
    {
        Abstract = 1 << 0,
        Async = 1 << 1,
        Const = 1 << 2,
        New = 1 << 3,
        Override = 1 << 4,
        Partial = 1 << 5,
        ReadOnly = 1 << 6,
        Ref = 1 << 7,
        Sealed = 1 << 8,
        Static = 1 << 9,
        Unsafe = 1 << 10,
        Virtual = 1 << 11,
        WithEvents = 1 << 12,
        WriteOnly = 1 << 13,

        None = 0
    }
}
