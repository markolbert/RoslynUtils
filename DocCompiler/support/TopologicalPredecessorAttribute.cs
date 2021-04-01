using System;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace J4JSoftware.DocCompiler
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class TopologicalPredecessorAttribute : Attribute
    {
        public TopologicalPredecessorAttribute( Type predecessorType )
        {
            PredecessorType = predecessorType;
        }

        public Type PredecessorType { get; }
    }
}
