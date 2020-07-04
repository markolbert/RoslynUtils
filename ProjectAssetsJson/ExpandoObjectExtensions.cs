using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace J4JSoftware.Roslyn
{
    public static class ExpandoObjectExtensions
    {
        public static bool LoadFromContainer<TItem, TContainer>(
            this ExpandoObject? container,
            Func<TItem> itemCreator,
            ProjectAssetsContext context,
            out List<TItem>? result,
            bool containerCanBeNull = false)
            where TItem : IInitializeFromNamed<TContainer>
        {
            result = null;

            if (container == null)
                return containerCanBeNull;

            var retVal = new List<TItem>();
            var isOkay = true;

            foreach (var kvp in container)
            {
                if (kvp.Value is TContainer childContainer)
                {
                    var newItem = itemCreator();

                    if (newItem.Initialize(kvp.Key, childContainer, context))
                        retVal.Add(newItem);
                    else
                        isOkay = false;
                }
                else
                {
                    // empty json arrays are always List<object>...which likely won't be the type of
                    // list defined by TContainer. so check for that case
                    if (kvp.Value is List<object> objArray && (objArray.Count <= 0)) continue;

                    isOkay = false;
                }
            }

            if( isOkay )
                result = retVal;

            return isOkay;
        }

        public static bool LoadNamesFromContainer(
            this ExpandoObject? container,
            out List<string>? result,
            bool containerCanBeNull = false)
        {
            result = null;

            if (container == null)
                return containerCanBeNull;

            var asDict = (IDictionary<string, object>)container;

            result = asDict.Select(kvp => kvp.Key)
                .ToList();

            return true;
        }

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
            if (!hasKey)
                return optional;

            if (asDict[propName] is TProp retVal)
            {
                result = retVal;

                return true;
            }

            return false;
        }
    }
}