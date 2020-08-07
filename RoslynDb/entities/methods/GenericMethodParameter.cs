using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public class GenericMethodParameter : MethodParameter
    {
        public TypeParameterConstraint Constraints { get; set; }

        public List<MethodTypeConstraint> TypeConstraints { get; set; }
    }
}