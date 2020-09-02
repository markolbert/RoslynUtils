using System;
using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public class DocObjectTypeMapper : IDocObjectTypeMapper
    {
        private readonly Dictionary<Type, DocObjectType> _lookup = new Dictionary<Type, DocObjectType>();

        public DocObjectTypeMapper()
        {
            _lookup.Add(typeof(AssemblyDb), DocObjectType.Assembly  );
            _lookup.Add(typeof(NamespaceDb), DocObjectType.Namespace  );
        }

        public DocObjectType GetDocObjectType<TEntity>()
        {
            return this[ typeof(TEntity) ];
        }

        public DocObjectType this [ Type entityType ] 
        {
            get
            {
                if( _lookup.ContainsKey( entityType ) )
                    return _lookup[ entityType ];

                return DocObjectType.Unknown;
            }
        }
    }
}