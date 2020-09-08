using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public static class ProjectAssetsExtensions
    {
        public static bool GetProperty<TProp>(
            this ExpandoObject container,
            string propName,
            out TProp result,
            bool caseSensitive = false,
            bool optional = false)
        {
            result = default!;

            if (string.IsNullOrEmpty(propName))
                return false;

            var asDict = (IDictionary<string, object>)container;

            // ExpandoObject keys are always case sensitive...so if we want a case insensitive match we have to 
            // go a bit convoluted...
            bool hasKey = false;

            if (caseSensitive) hasKey = asDict.ContainsKey(propName);
            else
            {
                // case insensitive matches
                switch (asDict.Keys.Count(k => k.Equals(propName, StringComparison.OrdinalIgnoreCase)))
                {
                    case 0:
                        // no match; key not found so default value of hasKey is okay
                        break;

                    case 1:
                        // replace the case-insensitive property name with the correctly-cased value
                        propName = asDict.Keys.First(k => k.Equals(propName, StringComparison.OrdinalIgnoreCase));
                        hasKey = true;

                        break;

                    default:
                        // multiple case-insensitive matches; case insensitive doesn't work
                        break;
                }
            }

            // it's okay if optional properties don't exist
            if (!hasKey && optional)
                return default!;

            if (asDict[propName] is TProp retVal)
            {
                result = retVal;
                return true;
            }

            return false;
        }

        public static bool ToEnum<TEnum>(this string text, out TEnum result)
        {
            result = default!;

            if (!typeof(TEnum).IsEnum)
                return false;

            if (Enum.TryParse(typeof(TEnum), text, true, out var retVal))
            {
                result = (TEnum)retVal!;
                return true;
            }

            return true;
        }

        public static bool GetEnum<TEnum>(
            this ExpandoObject container,
            string propName,
            out TEnum result,
            bool caseSensitive = false,
            bool optional = false)
        {
            result = default!;

            if( !GetProperty<string>( container, propName, out var text, caseSensitive, optional ) )
                return false;

            if (!text.ToEnum<TEnum>(out var innerResult))
                return false;

            result = innerResult;
            return true;
        }

        public static bool ToSemanticVersion(
            this string text,
            out SemanticVersion result )
        {
            result = default!;

            // ignore anything other than '.' or digits
            var sb = new StringBuilder();

            foreach (var curChar in text)
            {
                if (Char.IsDigit(curChar) || curChar == '.')
                    sb.Append(curChar);
            }

            if (sb.Length == 0)
                return false;

            text = sb.ToString();

            if (!SemanticVersion.TryParse(text, out var version))
                return false;

            result = version;

            return true;
        }

        public static bool GetSemanticVersion(
            this ExpandoObject container,
            string propName,
            out SemanticVersion result,
            bool caseSensitive = false,
            bool optional = false)
        {
            result = default!;

            if( !container.GetProperty<string>( propName, out var text, caseSensitive, optional ) )
                return false;

            if (!text.ToSemanticVersion(out var innerResult))
                return false;

            result = innerResult;

            return true;
        }
    }
}