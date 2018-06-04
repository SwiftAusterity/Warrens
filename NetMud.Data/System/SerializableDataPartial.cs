using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetMud.Data.System
{
    public abstract class SerializableDataPartial : IFileStored
    {
        /// <summary>
        /// Serialize this live entity to a json string
        /// </summary>
        /// <returns>json string</returns>
        public virtual string Serialize()
        {
            var serializer = SerializationUtility.GetSerializer();

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            serializer.Serialize(writer, this);

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize a json string into this entity
        /// </summary>
        /// <param name="jsonData">string to deserialize</param>
        /// <returns>the entity</returns>
        public virtual IFileStored DeSerialize(string jsonData)
        {
            var serializer = SerializationUtility.GetSerializer();

            var reader = new StringReader(jsonData);

            return serializer.Deserialize(reader, GetType()) as IFileStored;
        }

        /// <summary>
        /// Serialize this live entity to a binary stream
        /// </summary>
        /// <returns>binary stream</returns>
        public virtual byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(Serialize());
        }

        /// <summary>
        /// Deserialize a binary stream into this entity
        /// </summary>
        /// <param name="bytes">binary to deserialize</param>
        /// <returns>the entity</returns>
        public virtual IFileStored FromBytes(byte[] bytes)
        {
            var strData = Encoding.ASCII.GetString(bytes);

            var obj = DeSerialize(strData);

            //Finds containers and inits them to empty after this thing is deserialized
            foreach (var container in obj.GetType().GetProperties())
            {
                if (container.GetValue(obj) == null
                    && !container.PropertyType.GetCustomAttributes<JsonIgnoreAttribute>().Any()
                    && (container.PropertyType.IsArray || (!typeof(string).Equals(container.PropertyType) && typeof(IEnumerable).IsAssignableFrom(container.PropertyType))))
                {
                    try
                    {
                        container.SetValue(obj, Activator.CreateInstance(container.PropertyType, new object[] { }));
                    }
                    catch
                    {
                        //Oh well, can't init this on deserialization that's ok mostly
                    }
                }
            }

            return obj;
        }
    }
}
