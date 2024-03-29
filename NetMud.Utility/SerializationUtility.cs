﻿using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace NetMud.Utility
{
    /// <summary>
    /// For grabbing the json serializer
    /// </summary>
    public static class SerializationUtility
    {
        /// <summary>
        /// Gets the serializer
        /// </summary>
        /// <returns>the serializer</returns>
        public static JsonSerializer GetSerializer()
        {
            JsonSerializer serializer = JsonSerializer.Create();

            serializer.TypeNameHandling = TypeNameHandling.Auto;

            return serializer;
        }

        /// <summary>
        /// Serialize this live entity to a json string
        /// </summary>
        /// <returns>json string</returns>
        public static string Serialize(object thingToSerialize)
        {
            JsonSerializer serializer = GetSerializer();

            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);

            serializer.Serialize(writer, thingToSerialize);

            return sb.ToString();
        }
    }
}
