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
    /// <summary>
    /// Utility methods for data access/use
    /// </summary>
    public static class DataUtility
    {
        /// <summary>
        /// Fault safe type converted
        /// </summary>
        /// <typeparam name="T">the type to convert to</typeparam>
        /// <param name="thing">the thing being converted</param>
        /// <param name="newThing">the converted thing</param>
        /// <returns>success status</returns>
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
                //dont error on tryconvert, it's called tryconvert for a reason
                newThing = default(T);
            }

            return false;
        }

        /// <summary>
        /// Get's a single column's data from a datarow (fault safe)
        /// </summary>
        /// <typeparam name="T">the data type</typeparam>
        /// <param name="dr">the data row</param>
        /// <param name="columnName">the column to extract</param>
        /// <param name="thing">the output object</param>
        /// <returns>success status</returns>
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
                //dont error on this, it is supposed to be safe
                thing = default(T);
            }
        
            return false;
        }

        /// <summary>
        /// Gets all system types and interfaces implemented by or with for a type, including itself
        /// </summary>
        /// <param name="t">the system type in question</param>
        /// <returns>all types that touch the input type</returns>
        public static IEnumerable<Type> GetAllImplimentingedTypes(Type t)
        {
            var implimentedTypes = t.Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(t) || ty == t);
            return implimentedTypes.Concat(t.GetInterfaces());
        }

        /// <summary>
        /// Partial class for entity backing data xml (live backup storage format)
        /// </summary>
        public partial class EntityFileData
        {
            /// <summary>
            /// The binary data of the file
            /// </summary>
            public byte[] XmlBinary { get; set; }

            /// <summary>
            /// Creates a new file accessor for backing data from the binary data
            /// </summary>
            /// <param name="bytes">the binary data stream of the file</param>
            public EntityFileData(byte[] bytes)
            {
                XmlBinary = bytes;
            }

            /// <summary>
            /// Creates a new file accessor from the xml document
            /// </summary>
            /// <param name="xDoc">the xmldocument format of the file data</param>
            public EntityFileData(XDocument xDoc)
            {
                XDoc = xDoc;
            }

            /// <summary>
            /// String version of the file's data
            /// </summary>
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

            /// <summary>
            /// XML document version of the file's data
            /// </summary>
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
