using System;

namespace J4JSoftware.Roslyn.Deprecated
{
    public interface IParametricTypeContainer
    {
        Type ContainerType { get; }
        int ContainerID { get; }
        object Container { get; }
    }
}