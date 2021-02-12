using System.Collections.Generic;

namespace Tests.RoslynWalker
{
    public class ClassInfo : InterfaceInfo, ICodeElementTypeArguments
    {
        public new static ClassInfo Create( SourceLine srcLine )
        {
            var (name, typeArgs) = GetNameAndTypeArguments( srcLine.Line );

            var retVal = new ClassInfo( name, srcLine.Accessibility );
            retVal.TypeArguments.AddRange( typeArgs );

            return retVal;
        }
        
        private ClassInfo( string name, Accessibility accessibility )
            :base( name, accessibility )
        {
        }

        public List<FieldInfo> Fields { get; } = new();
    }
}