using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace NetMud.Utility
{
    public static class DataUtility
    {
        public static bool TryConvert<T>(object thing, ref T newThing)
        {
            try
            {
                if (thing == null)
                    return false;

                newThing = (T)thing;

                return true;
            }
            catch
            {
                //Def some logging here
            }

            return false;
        }

        public static bool GetFromDataRow<T>(DataRow dr, string columnName, ref T thing)
        {
            try
            {
                if (dr == null || !dr.Table.Columns.Contains(columnName))
                    return false;

                return TryConvert<T>(dr[columnName], ref thing);
            }
            catch
            {
                //Def some logging here
            }

            return false;
        }

        public static IEnumerable<Type> GetAllImplimentingedTypes(Type t)
        {
            var implimentedTypes = t.Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(t) || ty == t);
            return implimentedTypes.Concat(t.GetInterfaces());
        }

        public partial class EntityFileData
        {
            public byte[] XmlBinary { get; set; }

            public EntityFileData(byte[] bytes)
            {
                XmlBinary = bytes;
            }

            public EntityFileData(XDocument xDoc)
            {
                XDoc = xDoc;
            }

            public string XmlString
            {
                get
                {
                    var xml = Encoding.UTF8.GetString(XmlBinary);
                    xml = Regex.Replace(xml, @"[^\u0000-\u007F]", string.Empty); // Removes non ascii characters
                    return xml;
                }
                set
                {
                    value = Regex.Replace(value, @"[^\u0000-\u007F]", string.Empty); // Removes non ascii characters
                    XmlBinary = Encoding.UTF8.GetBytes(value);
                }
            }

            public XDocument XDoc
            {
                get
                {
                    using (var memoryStream = new MemoryStream(XmlBinary))
                    using (var xmlReader = XmlReader.Create(memoryStream))
                    {
                        var xml = XDocument.Load(xmlReader);
                        return xml;
                    }
                }
                set
                {
                    var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
                    using (var memoryStream = new MemoryStream())
                    using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                    {
                        value.WriteTo(xmlWriter);
                        xmlWriter.Flush();
                        XmlBinary = memoryStream.ToArray();
                    }
                }
            }
        }

    }
}
