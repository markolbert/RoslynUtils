using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public static class ExpandoObjectExtensions
    {
        //public static bool LoadFromContainer<TItem, TContainer>(
        //    this ExpandoObject? container,
        //    Func<TItem> itemCreator,
        //    ProjectAssetsContext context,
        //    out List<TItem>? result,
        //    bool containerCanBeNull = false)
        //    where TItem : IInitializeFromNamed<TContainer>
        //{
        //    result = null;

        //    if (container == null)
        //        return containerCanBeNull;

        //    var retVal = new List<TItem>();
        //    var isOkay = true;

        //    foreach (var kvp in container)
        //    {
        //        if (kvp.Value is TContainer childContainer)
        //        {
        //            var newItem = itemCreator();

        //            if (newItem.Initialize(kvp.Key, childContainer, context))
        //                retVal.Add(newItem);
        //            else
        //                isOkay = false;
        //        }
        //        else
        //        {
        //            // empty json arrays are always List<object>...which likely won't be the type of
        //            // list defined by TContainer. so check for that case
        //            if (kvp.Value is List<object> objArray && (objArray.Count <= 0)) continue;

        //            isOkay = false;
        //        }
        //    }

        //    if( isOkay )
        //        result = retVal;

        //    return isOkay;
        //}

        //public static bool LoadNamesFromContainer(
        //    this ExpandoObject? container,
        //    out List<string>? result,
        //    bool containerCanBeNull = false)
        //{
        //    result = null;

        //    if (container == null)
        //        return containerCanBeNull;

        //    var asDict = (IDictionary<string, object>)container;

        //    result = asDict.Select(kvp => kvp.Key)
        //        .ToList();

        //    return true;
        //}

        //public static TProp GetProperty<TProp>(
        //    this ExpandoObject container,
        //    string propName,
        //    bool caseSensitive = false,
        //    bool optional = false)
        //{
        //    if (string.IsNullOrEmpty(propName))
        //        return default!;

        //    var asDict = (IDictionary<string, object>) container;

        //    // ExpandoObject keys are always case sensitive...so if we want a case insensitive match we have to 
        //    // go a bit convoluted...
        //    bool hasKey = false;

        //    if (caseSensitive) hasKey = asDict.ContainsKey(propName);
        //    else
        //    {
        //        // case insensitive matches
        //        switch (asDict.Keys.Count(k => k.Equals(propName, StringComparison.OrdinalIgnoreCase)))
        //        {
        //            case 0:
        //                // no match; key not found so default value of hasKey is okay
        //                break;

        //            case 1:
        //                // replace the case-insensitive property name with the correctly-cased value
        //                propName = asDict.Keys.First(k => k.Equals(propName, StringComparison.OrdinalIgnoreCase));
        //                hasKey = true;

        //                break;

        //            default:
        //                // multiple case-insensitive matches; case insensitive doesn't work
        //                break;
        //        }
        //    }

        //    // it's okay if optional properties don't exist
        //    if (!hasKey && optional)
        //        return default!;

        //    if( asDict[ propName ] is TProp retVal )
        //        return retVal;

        //    throw new ArgumentException( $"Could not find property '{propName}' in ExpandoObject" );
        //}

        //public static TEnum GetEnum<TEnum>(
        //    this ExpandoObject container,
        //    string propName,
        //    bool caseSensitive = false,
        //    bool optional = false )
        //{
        //    if( !typeof(Enum).IsEnum )
        //        throw new ArgumentException( $"{typeof(TEnum)} is not an enum type" );

        //    var text = container.GetProperty<string>( propName, caseSensitive, optional );

        //    if( !Enum.TryParse( typeof(TEnum), text, true, out var retVal ) )
        //        return (TEnum) retVal!;

        //    throw new InvalidEnumArgumentException(
        //        $"Couldn't convert '{text}' to an instance of {typeof(TEnum)}");
        //}

        //public static SemanticVersion GetSemanticVersion(
        //    this ExpandoObject container,
        //    string propName,
        //    bool caseSensitive = false,
        //    bool optional = false )
        //{
        //    var text = container.GetProperty<string>( propName, caseSensitive, optional );

        //    if( Versioning.GetSemanticVersion( text, out var version ) )
        //        return version!;

        //    throw new ArgumentException( $"Couldn't parse '{version}' to a {typeof(SemanticVersion)}" );
        //}
    }
}