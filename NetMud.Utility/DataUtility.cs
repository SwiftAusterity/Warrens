using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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
        #region Randomizer
        public static decimal Next(this Random rnd, decimal from, decimal to)
        {
            byte fromScale = new System.Data.SqlTypes.SqlDecimal(from).Scale;
            byte toScale = new System.Data.SqlTypes.SqlDecimal(to).Scale;

            byte scale = (byte)(fromScale + toScale);

            if (scale > 28)
            {
                scale = 28;
            }

            decimal r = new decimal(rnd.Next(), rnd.Next(), rnd.Next(), false, scale);

            if (Math.Sign(from) == Math.Sign(to) || from == 0 || to == 0)
            {
                return decimal.Remainder(r, to - from) + from;
            }

            if ((double)from + rnd.NextDouble() * ((double)to - (double)from) < 0)
            {
                return decimal.Remainder(r, -from) + from;
            }

            return decimal.Remainder(r, to);
        }
        #endregion

        #region String Ext
        public static StringBuilder AppendFormattedLine(this StringBuilder sb, IFormatProvider provider, string format, params object[] args)
        {
            return sb.AppendLine(string.Format(provider, format, args));
        }

        public static StringBuilder AppendFormattedLine(this StringBuilder sb, string format, object arg0, object arg1, object arg2)
        {
            return sb.AppendLine(string.Format(format, arg0, arg1, arg2));
        }

        public static StringBuilder AppendFormattedLine(this StringBuilder sb, string format, params object[] args)
        {
            return sb.AppendLine(string.Format(format, args));
        }

        public static StringBuilder AppendFormattedLine(this StringBuilder sb, IFormatProvider provider, string format, object arg0)
        {
            return sb.AppendLine(string.Format(provider, format, arg0));
        }

        public static StringBuilder AppendFormattedLine(this StringBuilder sb, IFormatProvider provider, string format, object arg0, object arg1)
        {
            return sb.AppendLine(string.Format(provider, format, arg0, arg1));
        }

        public static StringBuilder AppendFormattedLine(this StringBuilder sb, IFormatProvider provider, string format, object arg0, object arg1, object arg2)
        {
            return sb.AppendLine(string.Format(provider, format, arg0, arg1, arg2));
        }

        public static StringBuilder AppendFormattedLine(this StringBuilder sb, string format, object arg0)
        {
            return sb.AppendLine(string.Format(format, arg0));
        }

        public static StringBuilder AppendFormattedLine(this StringBuilder sb, string format, object arg0, object arg1)
        {
            return sb.AppendLine(string.Format(format, arg0, arg1));
        }

        /// <summary>
        /// Is this any number (including negative/decimal/floats)
        /// </summary>
        /// <param name="str">the string in question</param>
        /// <returns>is it a number</returns>
        public static bool IsNumeric(this string str)
        {
            return !string.IsNullOrWhiteSpace(str) && decimal.TryParse(str, out decimal junk);
        }

        /// <summary>
        /// Is this any number of a specific type
        /// </summary>
        /// <typeparam name="T">the number type</typeparam>
        /// <param name="str">the string in question</param>
        /// <returns>is it a number</returns>
        public static bool IsNumeric<T>(this string str)
        {
            var tType = typeof(T);
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (tType == typeof(byte))
                {
                    return byte.TryParse(str, out byte junk);
                }

                if (tType == typeof(short))
                {
                    return short.TryParse(str, out short junk);
                }

                if (tType == typeof(ushort))
                {
                    return ushort.TryParse(str, out ushort junk);
                }

                if (tType == typeof(int))
                {
                    return int.TryParse(str, out int junk);
                }

                if (tType == typeof(uint))
                {
                    return uint.TryParse(str, out uint junk);
                }

                if (tType == typeof(long))
                {
                    return long.TryParse(str, out long junk);
                }

                if (tType == typeof(ulong))
                {
                    return ulong.TryParse(str, out ulong junk);
                }

                if (tType == typeof(double))
                {
                    return double.TryParse(str, out double junk);
                }

                if (tType == typeof(float))
                {
                    return float.TryParse(str, out float junk);
                }

                if (tType == typeof(decimal))
                {
                    return decimal.TryParse(str, out decimal junk);
                }
            }

            return false;
        }
        #endregion

        #region TypeManipulation
        public static int TowardsZero(this int obj, int value = 1)
        {
            if (obj < 0)
            {
                return obj += value;
            }

            if (obj > 0)
            {
                obj -= value;
            }

            return obj;
        }

        public static long TowardsZero(this long obj, long value = 1)
        {
            if (obj < 0)
            {
                return obj += value;
            }

            if (obj > 0)
            {
                obj -= value;
            }

            return obj;
        }

        public static double TowardsZero(this double obj, double value = 1)
        {
            if (obj < 0)
            {
                return obj += value;
            }

            if (obj > 0)
            {
                obj -= value;
            }

            return obj;
        }

        public static float TowardsZero(this float obj, float value = 1)
        {
            if (obj < 0)
            {
                return obj += value;
            }

            if (obj > 0)
            {
                obj -= value;
            }

            return obj;
        }

        public static decimal TowardsZero(this decimal obj, decimal value = 1)
        {
            if (obj < 0)
            {
                return obj += value;
            }

            if (obj > 0)
            {
                obj -= value;
            }

            return obj;
        }

        public static short TowardsZero(this short obj, short value = 1)
        {
            if (obj < 0)
            {
                return obj += value;
            }

            if (obj > 0)
            {
                obj -= value;
            }

            return obj;
        }

        public static bool IsBetweenOrEqual(this int obj, int lower, int higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this int obj, int lower, int higher)
        {
            return obj > lower && obj < higher;
        }

        public static bool IsBetweenOrEqual(this float obj, float lower, float higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this float obj, float lower, float higher)
        {
            return obj > lower && obj < higher;
        }

        public static bool IsBetweenOrEqual(this double obj, double lower, double higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this double obj, double lower, double higher)
        {
            return obj > lower && obj < higher;
        }

        public static bool IsBetweenOrEqual(this decimal obj, decimal lower, decimal higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this decimal obj, decimal lower, decimal higher)
        {
            return obj > lower && obj < higher;
        }

        public static bool IsBetweenOrEqual(this long obj, long lower, long higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this long obj, long lower, long higher)
        {
            return obj > lower && obj < higher;
        }

        public static bool IsBetweenOrEqual(this short obj, short lower, short higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this short obj, short lower, short higher)
        {
            return obj > lower && obj < higher;
        }

        public static bool IsBetweenOrEqual(this uint obj, uint lower, uint higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this uint obj, uint lower, uint higher)
        {
            return obj > lower && obj < higher;
        }

        public static bool IsBetweenOrEqual(this ushort obj, ushort lower, ushort higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this ushort obj, ushort lower, ushort higher)
        {
            return obj > lower && obj < higher;
        }

        public static bool IsBetweenOrEqual(this ulong obj, ulong lower, ulong higher)
        {
            return obj >= lower && obj <= higher;
        }

        public static bool IsBetween(this ulong obj, ulong lower, ulong higher)
        {
            return obj > lower && obj < higher;
        }
        #endregion

        #region "Type Conversion"
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
                {
                    return false;
                }

                if (typeof(T).IsEnum)
                {
                    if (thing is short || thing is int)
                    {
                        newThing = (T)thing;
                    }
                    else
                    {
                        newThing = (T)Enum.Parse(typeof(T), thing.ToString());
                    }
                }
                else
                {
                    newThing = (T)Convert.ChangeType(thing, typeof(T));
                }

                return true;
            }
            catch
            {
                //dont error on tryconvert, it's called tryconvert for a reason
                newThing = default;
            }

            return false;
        }

        /// <summary>
        /// Fault safe type converted
        /// </summary>
        /// <typeparam name="T">the type to convert to</typeparam>
        /// <param name="thing">the thing being converted</param>
        /// <param name="newThing">the converted thing</param>
        /// <returns>success status</returns>
        public static T TryConvert<T>(object thing)
        {
            T newThing = default;

            try
            {
                if (thing != null)
                {
                    newThing = (T)Convert.ChangeType(thing, typeof(T));
                }
            }
            catch
            {
                //dont error on tryconvert, it's called tryconvert for a reason
                newThing = default;
            }

            return newThing;
        }

        /// <summary>
        /// Gets all system types and interfaces implemented by or with for a type, including itself
        /// </summary>
        /// <param name="t">the system type in question</param>
        /// <returns>all types that touch the input type</returns>
        public static IEnumerable<Type> GetAllImplimentingedTypes(Type t)
        {
            IEnumerable<Type> implimentedTypes = t.Assembly.GetTypes().Where(ty => ty == t || ty.GetInterfaces().Contains(t) );
            return implimentedTypes.Concat(t.GetInterfaces());
        }

        /// <summary>
        /// Gets all system types and interfaces implemented by or with for a type, including itself
        /// </summary>
        /// <param name="t">the system type in question</param>
        /// <returns>all types that touch the input type</returns>
        public static bool ImplementsType<T>(this object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Type type = obj.GetType();
            return GetAllImplimentingedTypes(type).Any(ty => ty == typeof(T));
        }

        /// <summary>
        /// Make an instance of a thing
        /// </summary>
        /// <typeparam name="T">The type of the thing</typeparam>
        /// <param name="fillAtWill">if T is a collection add a blank thing in it</param>
        /// <returns>A thing or default(T) if it can't make the thing</returns>
        public static T InsantiateThing<T>(Assembly thingConcreteAssembly, bool fillAtWill = true)
        {
            Type thingType = typeof(T);
            T thing = default;

            if (thingType.IsInterface)
            {
                Type concreteClassType = thingConcreteAssembly.GetTypes().FirstOrDefault(ty => ty.GetInterfaces().Contains(thingType));

                if (concreteClassType != null)
                {
                    thing = (T)Activator.CreateInstance(concreteClassType);
                }
            }
            else if (thingType.IsArray 
                || (!typeof(string).Equals(thingType) && typeof(IEnumerable).IsAssignableFrom(thingType)))
            {
                Type[] underlyingTypes = thingType.GenericTypeArguments;
                thing = (T)Activator.CreateInstance(thingType, underlyingTypes.Select(typ => InstantiateSingleThing(typ, thingConcreteAssembly, fillAtWill)));
            }
            else if(!typeof(string).Equals(thingType))
            {
                thing = Activator.CreateInstance<T>();
            }

            return thing;
        }

        internal static object InstantiateSingleThing(Type thingType, Assembly thingConcreteAssembly, bool fillAtWill)
        {
            object thing = null;
            Assembly thingAssembly = thingType.Assembly;

            if (thingType.IsInterface)
            {
                Type concreteClassType = thingAssembly.GetTypes().FirstOrDefault(ty => ty.GetInterfaces().Contains(thingType));

                if (concreteClassType != null)
                {
                    thing = Activator.CreateInstance(concreteClassType);
                }
            }
            else if (thingType.IsArray
                || (!typeof(string).Equals(thingType) && typeof(IEnumerable).IsAssignableFrom(thingType)))
            {
                Type[] underlyingTypes = thingType.GenericTypeArguments;
                thing = Activator.CreateInstance(thingType, underlyingTypes.Select(typ => InstantiateSingleThing(typ, thingConcreteAssembly, fillAtWill)));
            }
            else
            {
                thing = Activator.CreateInstance(thingType);
            }

            return thing;
        }
        #endregion

        #region "Database"
        /// <summary>
        /// Get's a single column's data from a datarow (fault safe)
        /// </summary>
        /// <typeparam name="T">the data type</typeparam>
        /// <param name="dr">the data row</param>
        /// <param name="columnName">the column to extract</param>
        /// <param name="thing">the output object</param>
        /// <returns>success status</returns>
        public static T GetFromDataRow<T>(DataRow dr, string columnName)
        {
            T thing = default;

            try
            {
                if (dr == null || !dr.Table.Columns.Contains(columnName))
                {
                    return thing;
                }

                TryConvert(dr[columnName], ref thing);
            }
            catch
            {
                //dont error on this, it is supposed to be safe
                thing = default;
            }

            return thing;
        }
        #endregion

        #region "XML"
        /// <summary>
        /// Safely get the value of an element
        /// </summary>
        /// <typeparam name="T">the type of data you're looking for</typeparam>
        /// <param name="element">the parent element</param>
        /// <param name="xName">the XName of the element you want the value of</param>
        /// <returns>the value or default(T)</returns>
        public static T GetSafeElementValue<T>(this XContainer element, string xName)
        {
            T returnValue = default;

            try
            {
                if (element != null && element.Element(xName) != null)
                {
                    if (!TryConvert(element.Element(xName).Value, ref returnValue))
                    {
                        returnValue = default;
                    }
                }
            }
            catch
            {
                //its safe return, dont barf please
            }

            return returnValue;
        }

        /// <summary>
        /// Safely get the value of an element only used for strings
        /// </summary>
        /// <param name="element">the parent element</param>
        /// <param name="xName">the XName of the attribute you want the value of</param>
        /// <returns>the value or string.empty</returns>
        public static string GetSafeElementValue(this XContainer element, string xName)
        {
            string returnValue = string.Empty;

            try
            {
                if (element != null && element.Element(xName) != null)
                {
                    returnValue = element.Element(xName).Value;
                }
            }
            catch
            {
                //its safe return, dont barf please
            }

            return returnValue;
        }

        /// <summary>
        /// Safely get the value of an attribute
        /// </summary>
        /// <typeparam name="T">the type of data you're looking for</typeparam>
        /// <param name="element">the parent element</param>
        /// <param name="xName">the XName of the attribute you want the value of</param>
        /// <returns>the value or default(T)</returns>
        public static T GetSafeAttributeValue<T>(this XElement element, string xName)
        {
            T returnValue = default;

            try
            {
                if (element != null && element.Attribute(xName) != null)
                {
                    if (!TryConvert(element.Attribute(xName).Value, ref returnValue))
                    {
                        returnValue = default;
                    }
                }
            }
            catch
            {
                //its safe return, dont barf please
            }

            return returnValue;
        }

        /// <summary>
        /// Safely get the value of an attribute only used for strings
        /// </summary>
        /// <param name="element">the parent element</param>
        /// <param name="xName">the XName of the attribute you want the value of</param>
        /// <returns>the value or string.empty</returns>
        public static string GetSafeAttributeValue(this XElement element, string xName)
        {
            string returnValue = string.Empty;

            try
            {
                if (element != null && element.Attribute(xName) != null)
                {
                    returnValue = element.Attribute(xName).Value;
                }
            }
            catch
            {
                //its safe return, dont barf please
            }

            return returnValue;
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
                    string xml = Encoding.UTF8.GetString(XmlBinary);
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
                    using (MemoryStream memoryStream = new MemoryStream(XmlBinary))
                    using (XmlReader xmlReader = XmlReader.Create(memoryStream))
                    {
                        XDocument xml = XDocument.Load(xmlReader);
                        return xml;
                    }
                }
                set
                {
                    XmlWriterSettings settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
                    using (MemoryStream memoryStream = new MemoryStream())
                    using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings))
                    {
                        value.WriteTo(xmlWriter);
                        xmlWriter.Flush();
                        XmlBinary = memoryStream.ToArray();
                    }
                }
            }
        }
        #endregion

        #region FileSystem
        public static void CopyTo(this DirectoryInfo sourceDirectory, string destDirName)
        {
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirectory.FullName);
            }

            DirectoryInfo[] dirs = sourceDirectory.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = sourceDirectory.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            //copy subdirectories and their contents to new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                subdir.CopyTo(temppath);
            }
        }
        #endregion
    }
}
