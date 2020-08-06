using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public class GenericMethodArgument : MethodArgument
    {
        public GenericConstraint Constraints { get; set; }

        public List<MethodTypeConstraint> TypeConstraints { get; set; }
    }
}