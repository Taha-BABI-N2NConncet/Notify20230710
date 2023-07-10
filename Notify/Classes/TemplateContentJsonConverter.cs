using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Notify.Classes
{
    public class TemplateContentJsonConverter : JsonConverter<string>
    {
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return JsonConvert.SerializeObject(JToken.Load(reader));
        }

        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            writer.WriteRawValue(value);
        }
    }
}
