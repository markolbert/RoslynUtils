using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.DocCompiler
{
    public class TypeInfo
    {
        private readonly List<TypeInfo> _children = new();

        public TypeInfo? Parent { get; private set; }

        public string Name { get; private set; } = string.Empty;
        public TypeCharacteristic Characteristic { get; private set; }
        public ReadOnlyCollection<TypeInfo> Arguments => _children.AsReadOnly();

        public NamedType? DbEntity {get; set; }

        public TypeInfo AddChild( string name, TypeCharacteristic typeChar = TypeCharacteristic.None  )
        {
            var retVal = new TypeInfo
            {
                Name = name,
                Characteristic = typeChar,
                Parent = this
            };

            _children.Add( retVal );

            return retVal;
        }
    }
}
