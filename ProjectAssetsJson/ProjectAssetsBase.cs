using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssetsBase
    {
        public ProjectAssetsBase(
            IJ4JLogger<ProjectAssetsBase> logger
        )
        {
            Logger = logger ?? throw new NullReferenceException( nameof( logger ) );
        }

        protected IJ4JLogger<ProjectAssetsBase> Logger { get; }

        protected virtual bool ValidateLoadArguments<TContainer>( 
            string rawName, 
            TContainer container, 
            [CallerMemberName] string callerName = "" )
        {
            if( container == null )
            {
                Logger.Error( $"Undefined {nameof( container )} in {this.GetType().Name}::{callerName}" );

                return false;
            }

            if( String.IsNullOrEmpty( rawName ) )
            {
                Logger.Error( $"Undefined or empty {nameof( rawName )} in {this.GetType().Name}::{callerName}" );

                return false;
            }

            return true;
        }

        protected virtual bool ValidateLoadArguments<TContainer>( 
            TContainer container, 
            [CallerMemberName] string callerName = "" )
        {
            if( container == null )
            {
                Logger.Error( $"Undefined {nameof( container )} in {this.GetType().Name}::{callerName}" );

                return false;
            }

            return true;
        }

        protected bool LoadFromContainer<TItem, TContainer>( 
            ExpandoObject container, 
            Func<TItem> itemCreator, 
            out List<TItem> result,
            bool containerCanBeNull = false )
            where TItem : ILoadFromNamed<TContainer>
        {
            if( container == null )
            {
                result = null;

                if( containerCanBeNull )
                    return true;

                Logger.Error( $"Undefined {nameof( container )}" );

                return false;
            }

            result = new List<TItem>();
            var isOkay = true;

            foreach( var kvp in container )
            {
                if( kvp.Value is TContainer childContainer )
                {
                    var newItem = itemCreator();

                    if( newItem.Load( kvp.Key, childContainer ) )
                        result.Add( newItem );
                    else
                        isOkay = false;
                }
                else
                {
                    Logger.Error( $"{kvp.Key} property is not a {nameof( ExpandoObject )}" );
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
            bool containerCanBeNull = false )
        {
            if( container == null )
            {
                result = null;

                if( containerCanBeNull )
                    return true;

                Logger.Error($"Undefined {nameof(container)}"  );

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
            if( container == null || String.IsNullOrEmpty( propName ) )
            {
                Logger.Error(
                    $"Undefined {nameof(container)} and/or undefined/empty {nameof(propName)} (called from {GetCallerPath( callerName )})" );
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
                // it's okay if optional properties don't exist
                if( optional )
                {
                    result = default;

                    return true;
                }

                var mesg =
                    $"{nameof(container)} doesn't contain a {propName} property (called from {GetCallerPath( callerName )})";

                if( optional ) Logger.Information( mesg );
                else Logger.Error( mesg );

                result = default;

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

        protected string GetCallerPath( string callerName )
        {
            return $"{this.GetType().Name}::{callerName}";
        }
    }
}