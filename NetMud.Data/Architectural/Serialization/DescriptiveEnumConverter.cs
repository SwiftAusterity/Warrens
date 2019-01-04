using System;
using NetMud.Utility;
using Newtonsoft.Json;

namespace NetMud.Data.Architectural.Serialization
{
    public class DescriptiveEnumConverter<TEnum> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("No reading of this serializer.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Enum enumVal = (Enum)value;

            serializer.Serialize(writer, enumVal.GetDescription());
        }
    }
}
