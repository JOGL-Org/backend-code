using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

public class BsonDocumentConverter : JsonConverter<BsonDocument>
{
    public override BsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Read the entire JSON subtree into a JsonDocument
        using (JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            // Convert the JsonDocument to a string
            string jsonString = jsonDoc.RootElement.GetRawText();

            // Parse the string into a BsonDocument
            return BsonDocument.Parse(jsonString);
        }
    }

    public override void Write(Utf8JsonWriter writer, BsonDocument value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // Convert BsonDocument to JSON string
        string jsonString = value.ToJson();

        // Parse the JSON string to a JsonDocument
        using (JsonDocument jsonDoc = JsonDocument.Parse(jsonString))
        {
            // Write the JsonDocument to the writer
            jsonDoc.WriteTo(writer);
        }
    }
}