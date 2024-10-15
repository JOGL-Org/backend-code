using System.Text.Json.Serialization;
using System.Text.Json;
using MongoDB.Bson;

namespace Jogl.Server.Data.Converters
{
    internal class ObjectIdConverter : JsonConverter<ObjectId>
    {
        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return ObjectId.Parse(reader.GetString());
            }
            else
            {
                throw new JsonException("Expected string value.");
            }
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
