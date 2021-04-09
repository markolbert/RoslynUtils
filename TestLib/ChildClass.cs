using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Junk1 = System.CodeDom;

namespace J4JSoftware.Roslyn.Tests
{
    using System.CodeDom.Compiler;
    using Junk2 = System;

    public class ChildClass : BaseClass, IEnumerable<int>
    {
        public IEnumerator<int> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ListChildClass : List<int[]>
    {
    }
}
