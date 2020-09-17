using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.Serialization
{
    /// <summary>
    /// Serializer used to produce JSON strings from a dictionary with a complex key
    /// </summary>
    public class DictionaryConverter<TKey, TValue> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (typeof(IDictionary).IsAssignableFrom(objectType) || TypeImplementsGenericInterface(objectType, typeof(IDictionary<,>)));
        }

        private bool TypeImplementsGenericInterface(Type concreteType, Type interfaceType)
        {
            return concreteType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (existingValue == null)
            {
                reader.Skip();
                return new Dictionary<TKey, TValue>();
            }

            KeyValuePair<TKey, TValue>[] arrayValue = (KeyValuePair<TKey, TValue>[])existingValue;

            if (arrayValue.Count() == 0)
            {
                reader.Skip();
                return new Dictionary<TKey, TValue>();
            }

            return serializer.Deserialize<KeyValuePair<TKey, TValue>[]>(reader).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
            }

            IDictionary<TKey, TValue> dictValue = (IDictionary<TKey, TValue>)value;

            serializer.Serialize(writer, dictValue.ToArray());
        }
    }
}
