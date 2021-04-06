using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Roslyn.Tests
{
    using System.CodeDom.Compiler;

    public class ChildClass : BaseClass, IEnumerable<DelegateClass.NestedRecord>
    {
        public IEnumerator<DelegateClass.NestedRecord> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
