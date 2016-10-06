using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.SupportingClasses
{
    /// <summary>
    /// Criteria used to lookup constants values
    /// </summary>
    [TypeConverter(typeof(LookupCriteriaTypeConverter))]
    public interface ILookupCriteria : IComparable<ILookupCriteria>, IEquatable<ILookupCriteria>
    {
        /// <summary>
        /// The type of criteria to look for
        /// </summary>
        Dictionary<CriteriaType, string> Criterion { get; set; }
    }

    /// <summary>
    /// Criteria type for constant values lookup
    /// </summary>
    public enum CriteriaType : short
    {
        Race = 0,
        Language = 1,
        GameLanguage = 2,
        PortType = 3,
        TimeOfDay = 4,
        Season = 5
    }

    public class LookupCriteriaTypeConverter : TypeConverter
    {
        /// <summary>
        /// Overrides the CanConvertFrom method of TypeConverter.
        /// The ITypeDescriptorContext interface provides the context for the conversion. Typically, this interface is used at design time to provide information about the design-time container.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                var serializer = JsonSerializer.Create();
                serializer.TypeNameHandling = TypeNameHandling.All;

                var reader = new StringReader(value as string);

                return serializer.Deserialize(reader, this.GetType()) as ILookupCriteria;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
