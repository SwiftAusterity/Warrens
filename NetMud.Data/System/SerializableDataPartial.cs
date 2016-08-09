using NetMud.DataStructure.Base.System;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace NetMud.Data.System
{
    public abstract class SerializableDataPartial
    {
        public byte[] Serialize()
        {
            var serializer = JsonSerializer.Create();

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            serializer.Serialize(writer, this);

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        public IData DeSerialize(byte[] bytes)
        {
            var serializer = JsonSerializer.Create();

            var strData = Encoding.ASCII.GetString(bytes);
            var reader = new StringReader(strData);

            return serializer.Deserialize(reader, this.GetType()) as IData;
        }
    }
}
