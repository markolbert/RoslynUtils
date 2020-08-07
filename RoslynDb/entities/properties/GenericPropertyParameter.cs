﻿using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public class GenericPropertyParameter : PropertyParameter
    {
        public TypeParameterConstraint Constraints { get; set; }

        public List<PropertyTypeConstraint> TypeConstraints { get; set; }
    }
}