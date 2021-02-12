using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.RoslynWalker
{
    public static class SourceText
    {
        public static List<string> GetArgs( string text )
        {
            var retVal = new List<string>();

            var closeParenLoc = text.IndexOf( ")", StringComparison.Ordinal );
            if( closeParenLoc < 0 )
                return retVal;

            var args = text[ ..^closeParenLoc ]
                .Split( "," )
                .Select( x => x.Trim() )
                .ToList();

            foreach( var arg in args )
            {
                var argParts = arg.Split( " " );
                retVal.Add( argParts.Last() );
            }

            return retVal;
        }

        public static List<string> GetTypeArgs( string text )
        {
            var findGreaterThan = text.IndexOf( ">", StringComparison.Ordinal );

            if( findGreaterThan < 0 ) 
                return new List<string>();

            var typeArgs = text[ ..^findGreaterThan ]
                .Split( "," )
                .Select( x => x.Trim() )
                .ToList();

            return typeArgs.Select( x =>
                {
                    var typeParts = x.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                    return typeParts.Length == 1 ? typeParts[ 0 ] : typeParts[ ^1 ];
                } )
                .ToList();
        }
    }
}