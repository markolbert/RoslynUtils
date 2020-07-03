using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ConfigurationBase
    {
        protected ConfigurationBase( IJ4JLogger logger )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        protected virtual bool ValidateInitializationArguments<TContainer>( 
            string rawName, 
            TContainer container,
            ProjectAssetsContext context,
            [CallerMemberName] string callerName = "" )
        {
            if( !string.IsNullOrEmpty( rawName ) ) 
                return true;

            Logger.Error<string>( "Undefined or empty {rawName}", rawName );

            return false;

        }

        protected virtual bool ValidateInitializationArguments<TContainer>(
            TContainer container,
            ProjectAssetsContext context ) => true;

        protected bool LoadFromContainer<TItem, TContainer>( 
            ExpandoObject? container, 
            Func<TItem> itemCreator,
            ProjectAssetsContext context,
            out List<TItem>? result,
            bool containerCanBeNull = false )
            where TItem : IInitializeFromNamed<TContainer>
        {
            if( container == null )
            {
                result = null;

                if( containerCanBeNull )
                    return true;

                Logger.Error<string>( "Undefined {0}", nameof(container) );

                return false;
            }

            result = new List<TItem>();
            var isOkay = true;

            foreach( var kvp in container )
            {
                if( kvp.Value is TContainer childContainer )
                {
                    var newItem = itemCreator();

                    if( newItem.Initialize( kvp.Key, childContainer, context ) )
                        result.Add( newItem );
                    else
                        isOkay = false;
                }
                else
                {
                    // empty json arrays are always List<object>...which likely won't be the type of
                    // list defined by TContainer. so check for that case
                    if( kvp.Value is List<object> objArray && ( objArray.Count <= 0 ) ) continue;

                    Logger.Error<string, string>( "{0} property is not a {1}", kvp.Key, nameof(ExpandoObject) );

                    isOkay = false;
                }
            }

            // wipe out collection if something went wrong
            if( !isOkay ) result = null;

            return isOkay;
        }

        protected bool LoadNamesFromContainer( 
            ExpandoObject? container, 
            out List<string>? result, 
            bool containerCanBeNull = false )
        {
            if( container == null )
            {
                result = null;

                if( containerCanBeNull )
                    return true;

                Logger.Error<string>( "Undefined {0}", nameof(container) );

                return false;
            }

            var asDict = (IDictionary<string, object>) container;

            result = asDict.Select( kvp => kvp.Key )
                .ToList();

            return true;
        }

        protected bool GetProperty<TProp>( 
            ExpandoObject container, 
            string propName, 
            ProjectAssetsContext context,
            out TProp result,
            bool caseSensitive = false, 
            bool optional = false )
        {
            if( string.IsNullOrEmpty( propName ) )
            {
                Logger.Error<string>( "Undefined/empty {0}", nameof(propName) );
                result = default!;

                return false;
            }

            var asDict = (IDictionary<string, object>) container;

            // ExpandoObject keys are always case sensitive...so if we want a case insensitive match we have to 
            // go a bit convoluted...
            bool hasKey = false;

            if( caseSensitive ) hasKey = asDict.ContainsKey( propName );
            else
            {
                // case insensitive matches
                switch( asDict.Keys.Count( k => k.Equals( propName, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    case 0:
                        // no match; key not found so default value of hasKey is okay
                        break;

                    case 1:
                        // replace the case-insensitive property name with the correctly-cased value
                        propName = asDict.Keys.First( k => k.Equals( propName, StringComparison.OrdinalIgnoreCase ) );
                        hasKey = true;

                        break;

                    default:
                        // multiple case-insensitive matches; case insensitive doesn't work
                        Logger.Error<string, string>(
                            "Multiple matching property names in {0} for property name '{1}'", 
                            nameof(ExpandoObject),
                            propName );

                        break;
                }
            }

            if( !hasKey )
            {
                result = default!;

                var mesg = $"{nameof(container)} doesn't contain a {propName} property";

                // it's okay if optional properties don't exist
                if( optional )
                {
                    Logger.Information( mesg );

                    return true;
                }

                Logger.Error( mesg );

                return false;
            }

            if( asDict[ propName ] is TProp retVal )
            {
                result = retVal;

                return true;
            }

            Logger.Error<string, string>( "The {0} property is not a {1}", propName, typeof(TProp).Name );

            result = default!;

            return false;
        }

        protected bool TraverseContainerTree<TContainer>( TContainer toFind, ExpandoObject curExpando, Stack<string> propertyStack )
        {
            var asDict = (IDictionary<string, object>) curExpando;

            foreach( var kvp in asDict )
            {
                switch( kvp.Value )
                {
                    case TContainer container when Object.Equals( container, toFind ):
                        propertyStack.Push( kvp.Key );
                        return true;

                    case ExpandoObject expando:
                        propertyStack.Push(kvp.Key);

                        if( TraverseContainerTree<TContainer>( toFind, expando, propertyStack ))
                            return true;

                        break;
                }
            }

            propertyStack.Pop();

            return false;
        }
    }
}