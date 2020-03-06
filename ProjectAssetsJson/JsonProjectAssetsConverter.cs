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
        private readonly Stack<ExpandoObject> _containers = new Stack<ExpandoObject>();
        private readonly ITypedListCreator _listCreator;
        private readonly IJ4JLogger<JsonProjectAssetsConverter> _logger;

        private string _propName;
        private bool _buildingArray;

        public JsonProjectAssetsConverter(
            ITypedListCreator listCreator,
            IJ4JLogger<JsonProjectAssetsConverter> logger
        )
        {
            _listCreator = listCreator ?? throw new NullReferenceException( nameof(listCreator) );
            _logger = logger ?? throw new NullReferenceException( nameof(logger) );
        }

        public override ExpandoObject Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
        {
            _containers.Push( new ExpandoObject() );

            IterateTokens(ref reader );

            return (ExpandoObject) _containers.Pop();
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
                    var expando = new ExpandoObject();

                    RecordValue( expando );

                    // push new container onto stack after adding property 
                    // because otherwise you'll create a self-reference
                    _containers.Push( expando );

                    IterateTokens( ref reader );

                    break;

                case JsonTokenType.EndObject:
                    // we don't want to pop off what is hopefully the final result
                    // on the last EndObject
                    if( _containers.Count > 1 )
                        _containers.Pop();

                    // erase the existing name
                    _propName = null;

                    return;

                case JsonTokenType.StartArray:
                    // we don't create the value list at this point because
                    // we don't yet know what kind of List<> to create
                    _buildingArray = true;
                    _listCreator.Clear();

                    IterateTokens( ref reader );

                    break;

                case JsonTokenType.EndArray:
                    // have to flag that we're ending list creation before recording
                    // the property because otherwise the list will just get assigned to
                    // to itself
                    _buildingArray = false;

                    RecordValue( _listCreator.GetList() );

                    _listCreator.Clear();

                    return;

                case JsonTokenType.PropertyName:
                    _propName = reader.GetString();

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

                    _logger.Error($"Failed to retrieve numeric token");
                    break;

                case JsonTokenType.String:
                    RecordValue( reader.GetString() );

                    break;
            }
        }

        protected void RecordValue<TProp>( TProp value )
        {
            // where we record the value depends on whether or not
            // we're in the midst of building an array
            if( _buildingArray )
            {
                // just add the value to the list creator
                _listCreator.Add( value );
            }
            else
            {
                if( !_containers.Peek().TryAdd( _propName, value ) )
                    _logger.Error( $"Failed to add property '{_propName}' to container" );

                // discard the property name once we've added the property
                _propName = null;
            }
        }

        public override void Write( Utf8JsonWriter writer, ExpandoObject value, JsonSerializerOptions options )
        {
            _logger.Error($"{nameof(Write)} not implemented");
        }
    }
}