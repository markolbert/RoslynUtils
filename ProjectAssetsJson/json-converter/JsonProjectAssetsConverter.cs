using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class JsonProjectAssetsConverter : JsonConverter<ExpandoObject>
    {
        private readonly Stack<ExpandoObject> _expandos = new Stack<ExpandoObject>();
        private readonly IJ4JLogger _logger;
        private readonly Stack<string> _propertyNames = new Stack<string>();
        private readonly ITypedListCreator _listBuilder;

        private bool _buildingArray;

        public JsonProjectAssetsConverter(
            ITypedListCreator listBuilder,
            IJ4JLogger logger
        )
        {
            _listBuilder = listBuilder;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public override ExpandoObject Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
        {
            _expandos.Push( new ExpandoObject() );

            IterateTokens(ref reader );

            return (ExpandoObject) _expandos.Pop();
        }

        protected void IterateTokens( ref Utf8JsonReader reader )
        {
            while( reader.Read() )
            {
                ProcessToken( ref reader );
            }
        }

        protected void ProcessToken( ref Utf8JsonReader reader )
        {
            switch( reader.TokenType )
            {
                case JsonTokenType.StartObject:
                    // containers can be either a named property or
                    // an unnamed item that's part of a list...so we decide what to 
                    // do after the object is completed/closed

                    // push new container onto stack 
                    _expandos.Push( new ExpandoObject() );

                    IterateTokens( ref reader );

                    break;

                case JsonTokenType.EndObject:
                    // we don't want to pop off what is hopefully the final result
                    // on the last EndObject (the very last EndObject can't be for an
                    // expando that's in a list because lists can't define/close a
                    // json file
                    if( _expandos.Count > 1 )
                        RecordValue( _expandos.Pop() );

                    return;

                case JsonTokenType.StartArray:
                    // we don't create the value list at this point because
                    // we don't yet know what kind of List<> to create
                    _buildingArray = true;
                    _listBuilder.Clear();

                    IterateTokens( ref reader );

                    break;

                case JsonTokenType.EndArray:
                    // have to flag that we're ending list creation before recording
                    // the property because otherwise the list will just get assigned to
                    // to itself
                    _buildingArray = false;

                    RecordValue( _listBuilder.GetList() );

                    _listBuilder.Clear();

                    return;

                case JsonTokenType.PropertyName:
                    _propertyNames.Push( reader.GetString() );

                    break;

                case JsonTokenType.False:
                    RecordValue(false);
                    break;

                case JsonTokenType.True:
                    RecordValue( true );
                    break;

                case JsonTokenType.Number:
                    // try all the usual suspects...
                    if( reader.TryGetInt32( out var intValue ) )
                    {
                        RecordValue( intValue );
                        return;
                    }

                    if( reader.TryGetDouble( out var dblValue ) )
                    {
                        RecordValue( dblValue );
                        return;
                    }

                    if( reader.TryGetDecimal( out var decValue ) )
                    {
                        RecordValue( decValue );
                        return;
                    }

                    _logger.Error("Failed to retrieve numeric token");
                    break;

                case JsonTokenType.String:
                    RecordValue( reader.GetString() );

                    break;
            }
        }

        protected void RecordValue<TProp>( TProp value )
        {
            if( String.IsNullOrEmpty( _propertyNames.Peek() ))
                System.Diagnostics.Debugger.Break();

            // where we record the value depends on whether or not
            // we're in the midst of building an array
            
            // if we're building an array just add the value to the list builder
            if( _buildingArray ) _listBuilder.Add( value );
            else
            {
                var propName = _propertyNames.Pop();

                if( !_expandos.Peek().TryAdd( propName, value ) )
                    _logger.Error<string>( "Failed to add property '{0}' to container", propName );
            }
        }

        public override void Write( Utf8JsonWriter writer, ExpandoObject value, JsonSerializerOptions options )
        {
            _logger.Error<string>( $"{0} not implemented", nameof(Write) );
        }
    }
}