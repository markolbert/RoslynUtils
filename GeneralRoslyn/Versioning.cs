using System;
using System.Text;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn.Deprecated
{
    public static class Versioning
    {
        public static bool GetSemanticVersion(string text, out SemanticVersion? result)
        {
            result = null;

            // ignore anything other than '.' or digits
            var sb = new StringBuilder();
            foreach( var curChar in text )
            {
                if( Char.IsDigit( curChar ) || curChar == '.' )
                    sb.Append( curChar );
            }

            if (sb.Length == 0)
                return false;

            text = sb.ToString();

            if (!SemanticVersion.TryParse(text, out var version))
                return false;

            result = version;

            return true;
        }
    }
}