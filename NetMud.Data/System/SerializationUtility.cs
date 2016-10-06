using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
