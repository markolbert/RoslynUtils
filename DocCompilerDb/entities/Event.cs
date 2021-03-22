using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.DocCompiler
{
    public class Event
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<NamedType> DeclaredIn { get; set; }
        public int EventTypeID { get; set; }
        public NamedTypeReference EventType { get; set; }
    }
}
