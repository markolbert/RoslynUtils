using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.DocCompiler
{
    public class Field
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int DeclaredInID { get; set; }
        public NamedType DeclaredIn { get; set; }
        public int FieldTypeID { get; set; }
        public NamedTypeReference FieldType { get; set; }
    }
}
