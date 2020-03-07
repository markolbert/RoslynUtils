using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssetsBase
    {
        public static ExpandoObject RootContainer { get; set; }

        public ProjectAssetsBase(
            IJ4JLogger<ProjectAssetsBase> logger
        )
        {
            Logger = logger ?? throw new NullReferenceException( nameof( logger ) );
        }

        protected IJ4JLogger<ProjectAssetsBase> Logger { get; }

        protected virtual bool ValidateInitializationArguments<TContainer>( 
            string rawName, 
            TContainer container, 
            [CallerMemberName] string callerName = "" )
        {
            if( container == null )
            {
                Logger.Error( $"Undefined {nameof( container )}, {nameof( rawName )} is '{rawName}' (called from {GetCallerPath( callerName )})" );

                return false;
            }

            if( String.IsNullOrEmpty( rawName ) )
            {
                Logger.Error( $"Undefined or empty {nameof( rawName )}  (called from {GetCallerPath( callerName )})" );

                return false;
            }

            return true;
        }

        protected virtual bool ValidateInitializationArguments<TContainer>( 
            TContainer container, 
            [CallerMemberName] string callerName = "" )
        {
            if( container == null )
            {
                Logger.Error( $"Undefined {nameof( container )} (called from {GetCallerPath( callerName )})" );

                return false;
            }

            return true;
        }

        protected bool LoadFromContainer<TItem, TContainer>( 
            ExpandoObject container, 
            Func<TItem> itemCreator, 
            out List<TItem> result,
            bool containerCanBeNull = false,
            [CallerMemberName] string callerName = "")
            where TItem : IInitializeFromNamed<TContainer>
        {
            if( container == null )
            {
                result = null;

                if( containerCanBeNull )
                    return true;

                Logger.Error( $"Undefined {nameof( container )} (called from {GetCallerPath( callerName )})" );

                return false;
            }

            result = new List<TItem>();
            var isOkay = true;

            foreach( var kvp in container )
            {
                if( kvp.Value is TContainer childContainer )
                {
                    var newItem = itemCreator();

                    if( newItem.Initialize( kvp.Key, childContainer ) )
                        result.Add( newItem );
                    else
                        isOkay = false;
                }
                else
                {
                    // empty json arrays are always List<object>...which likely won't be the type of
                    // list defined by TContainer. so check for that case
                    if( kvp.Value is List<object> objArray && ( objArray.Count <= 0 ) ) continue;

                    Logger.Error(
                        $"{kvp.Key} property is not a {nameof(ExpandoObject)} (called from {GetCallerPath( callerName )}){GetPropertyPath( container )}" );

                    isOkay = false;
                }
            }

            // wipe out collection if something went wrong
            if( !isOkay ) result = null;

            return isOkay;
        }

        protected bool LoadNamesFromContainer( 
            ExpandoObject container, 
            out List<string> result, 
            bool containerCanBeNull = false,
            [CallerMemberName] string callerName = "" )
        {
            if( container == null )
            {
                result = null;

                if( containerCanBeNull )
                    return true;

                Logger.Error($"Undefined {nameof(container)} (called from {GetCallerPath( callerName )})"  );

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
            out TProp result,
            bool caseSensitive = false, 
            bool optional = false,
            [CallerMemberName] string callerName = "" )
        {
            if( container == null )
            {
                Logger.Error(
                    $"Undefined {nameof(container)} (called from {GetCallerPath( callerName )})" );
                result = default;

                return false;
            }

            if( String.IsNullOrEmpty( propName ) )
            {
                Logger.Error(
                    $"Undefined/empty {nameof(propName)} (called from {GetCallerPath( callerName )}){GetPropertyPath( container )}" );
                result = default;

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
                        Logger.Error(
                            $"Multiple matching property names in {nameof(ExpandoObject)} for property name '{propName}' (called from {GetCallerPath( callerName )})" );
                        break;
                }
            }

            if( !hasKey )
            {
                result = default;

                var mesg =
                    $"{nameof(container)} doesn't contain a {propName} property (called from {GetCallerPath( callerName )}){GetPropertyPath( container )}";

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

            Logger.Error(
                $"The {propName} property is not a {typeof(TProp).Name} (called from {GetCallerPath( callerName )})" );

            result = default;

            return false;
        }

        protected string GetCallerPath( string callerName, [CallerMemberName] string immediateCaller = "" )
        {
            return $"{this.GetType().Name}::{callerName}::{immediateCaller}";
        }

        protected string GetPropertyPath<TContainer>( TContainer toFind )
        {
            StringBuilder sb = new StringBuilder();
            var propertyStack = new Stack<string>();

            if( TraverseContainerTree<TContainer>( toFind, RootContainer, propertyStack ) )
            {
                while( propertyStack.Count > 0 )
                {
                    if( sb.Length > 0 ) sb.Insert( 0, "->" );
                    sb.Insert( 0, propertyStack.Pop() );
                }
            }

            if( sb.Length > 0 )
            {
                sb.Insert( 0, " [" );
                sb.Append( "]" );
            }

            return sb.ToString();
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