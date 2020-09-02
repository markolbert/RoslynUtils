using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Roslyn.entities
{
    public interface IDocObject
    {
        public int DocObjectID { get; set; }
        public DocObject DocObject { get; set; }
    }

    public interface IFullyQualifiedName
    {
        public string FullyQualifiedName { get; set; }
    }

    public interface ISynchronized
    {
        public bool Synchronized { get; set; }
    }
}
