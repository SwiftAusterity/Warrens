using Newtonsoft.Json;

namespace NetMud.Data.System
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
            var serializer = JsonSerializer.Create();

            serializer.TypeNameHandling = TypeNameHandling.Auto;

            return serializer;
        }
    }
}
