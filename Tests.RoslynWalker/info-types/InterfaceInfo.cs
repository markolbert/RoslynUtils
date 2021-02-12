using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tests.RoslynWalker
{
    public class InterfaceInfo : NamedTypeInfo, ICodeElementTypeArguments
    {
        public static InterfaceInfo Create( SourceLine srcLine )
        {
            var (name, typeArgs) = GetNameAndTypeArguments( srcLine.Line );

            var retVal = new InterfaceInfo( name, srcLine.Accessibility );
            retVal.TypeArguments.AddRange( typeArgs );

            return retVal;
        }

        public static (string name, List<string> typeArgs) GetNameAndTypeArguments( string line )
        {
            var parts = line.Split( " ", StringSplitOptions.RemoveEmptyEntries );

            var rawName = parts.Length > 3 ? string.Join( " ", parts[ 2.. ] ) : parts[ 2 ];

            var findColon = rawName.IndexOf( ":", StringComparison.Ordinal );
            if( findColon >= 0 )
                rawName = rawName[ ..( findColon - 1 ) ];

            var typeArgs = new List<string>();

            var findLessThan = rawName.IndexOf( "<", StringComparison.Ordinal );

            if( findLessThan < 0 )
                return (rawName, typeArgs);

            return ( rawName[ ..findLessThan ].Trim(),
                SourceText.GetTypeArgs( rawName[ ( findLessThan + 1 ).. ].Trim() ) );
        }

        protected InterfaceInfo( string name, Accessibility accessibility )
            :base( name, accessibility )
        {
        }

        public List<string> TypeArguments { get; } = new();
        public List<MethodInfo> Methods { get; } = new();
        public List<PropertyInfo> Properties { get; } = new();
        public List<EventInfo> Events { get; } = new();
    }
}