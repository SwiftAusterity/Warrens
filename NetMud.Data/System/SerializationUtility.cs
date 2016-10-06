using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.System
{
    /// <summary>
    /// For grabbing the json serializer
    /// </summary>
    public static class SerializationUtility
    {
        #region "JSON"
        /// <summary>
        /// Gets the serializer
        /// </summary>
        /// <returns>the serializer</returns>
        public static JsonSerializer GetSerializer()
        {
            var serializer = JsonSerializer.Create();

            serializer.Converters.Add(new LookupCriteriaConverter());

            return serializer;
        }
        #endregion
    }

    /// <summary>
    /// Used for serializing lookup criteria
    /// </summary>
    internal class LookupCriteriaConverter : CustomCreationConverter<LookupCriteria>
    {
        public override LookupCriteria Create(Type objectType)
        {
            return new LookupCriteria();
        }
         
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }  
    }
}
