using CommonMark;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NetMud.DataStructure.Architectural
{
    [Serializable]
    public class MarkdownString : IComparable<MarkdownString>, IEquatable<MarkdownString>, IEqualityComparer<MarkdownString>, IComparable<string>, IEquatable<string>, IEqualityComparer<string>
    {
        public string Value { get; set; }

        public MarkdownString(string value)
        {
            Value = value;
        }

        public static implicit operator string(MarkdownString d)
        {
            return CommonMarkConverter.Convert(d.Value);
        }

        public static implicit operator MarkdownString(string d)
        {
            return new MarkdownString(d);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        #region Extraneous String Manipulation Functions
        public int Length { get; }

        public static int Compare(MarkdownString strA, MarkdownString strB, bool ignoreCase)
        {
            return string.Compare(strA.Value, strB.Value, ignoreCase);
        }

        [SecuritySafeCritical]
        public static int Compare(MarkdownString strA, int indexA, MarkdownString strB, int indexB, int length, StringComparison comparisonType)
        {
            return string.Compare(strA.Value, indexA, strB.Value, indexB, length, comparisonType);
        }

        public static int Compare(MarkdownString strA, int indexA, MarkdownString strB, int indexB, int length, CultureInfo culture, CompareOptions options)
        {
            return string.Compare(strA.Value, indexA, strB.Value, indexB, length, culture, options);
        }

        public static int Compare(MarkdownString strA, int indexA, MarkdownString strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
        {
            return string.Compare(strA.Value, indexA, strB.Value, indexB, length, ignoreCase, culture);
        }

        public static int Compare(MarkdownString strA, int indexA, MarkdownString strB, int indexB, int length, bool ignoreCase)
        {
            return string.Compare(strA.Value, indexA, strB.Value, indexB, length, ignoreCase);
        }

        public static int Compare(MarkdownString strA, int indexA, MarkdownString strB, int indexB, int length)
        {
            return string.Compare(strA.Value, indexA, strB.Value, indexB, length);
        }

        public static int Compare(MarkdownString strA, MarkdownString strB, bool ignoreCase, CultureInfo culture)
        {
            return string.Compare(strA.Value, strB.Value, ignoreCase, culture);
        }

        public static int Compare(MarkdownString strA, MarkdownString strB, CultureInfo culture, CompareOptions options)
        {
            return string.Compare(strA.Value, strB.Value, culture, options);
        }

        [SecuritySafeCritical]
        public static int Compare(MarkdownString strA, MarkdownString strB, StringComparison comparisonType)
        {
            return string.Compare(strA.Value, strB.Value, comparisonType);
        }

        public static int Compare(MarkdownString strA, MarkdownString strB)
        {
            return string.Compare(strA.Value, strB.Value);
        }

        [SecuritySafeCritical]
        public static int CompareOrdinal(MarkdownString strA, int indexA, MarkdownString strB, int indexB, int length)
        {
            return string.CompareOrdinal(strA.Value, indexA, strB.Value, indexB, length);
        }

        public static int CompareOrdinal(MarkdownString strA, MarkdownString strB)
        {
            return string.CompareOrdinal(strA.Value, strB.Value);
        }

        [SecuritySafeCritical]
        public static MarkdownString Concat(MarkdownString str0, MarkdownString str1)
        {
            return string.Concat(str0.Value, str1.Value);
        }

        [SecuritySafeCritical]
        public static MarkdownString Concat(MarkdownString str0, MarkdownString str1, MarkdownString str2)
        {
            return string.Concat(str0.Value, str1.Value, str2.Value);
        }

        public static MarkdownString Concat(object arg0)
        {
            return string.Concat(arg0);
        }

        [SecuritySafeCritical]
        public static MarkdownString Concat(MarkdownString str0, MarkdownString str1, MarkdownString str2, MarkdownString str3)
        {
            return string.Concat(str0.Value, str1.Value, str2.Value, str3.Value);
        }

        public static MarkdownString Concat(object arg0, object arg1)
        {
            return string.Concat(arg0, arg1);
        }

        public static MarkdownString Concat(params MarkdownString[] values)
        {
            return string.Concat(values.Select(val => val.Value));
        }

        public static MarkdownString Concat(object arg0, object arg1, object arg2, object arg3)
        {
            return string.Concat(arg0, arg1, arg2, arg3);
        }

        public static MarkdownString Concat(params object[] args)
        {
            return string.Concat(args);
        }

        [ComVisible(false)]
        public static MarkdownString Concat<T>(IEnumerable<T> values)
        {
            return string.Concat(values);
        }

        [ComVisible(false)]
        public static MarkdownString Concat(IEnumerable<MarkdownString> values)
        {
            return string.Concat(values.Select(val => val.Value));
        }

        public static MarkdownString Concat(object arg0, object arg1, object arg2)
        {
            return string.Concat(arg0, arg1, arg2);
        }

        [SecuritySafeCritical]
        public static MarkdownString Copy(MarkdownString str)
        {
            return string.Copy(str.Value);
        }

        [SecuritySafeCritical]
        public static bool Equals(MarkdownString a, MarkdownString b, StringComparison comparisonType)
        {
            return string.Equals(a.Value, b.Value, comparisonType);
        }

        public static MarkdownString Format(MarkdownString format, object arg0)
        {
            return string.Format(format.Value, arg0);
        }

        public static MarkdownString Format(MarkdownString format, object arg0, object arg1, object arg2)
        {
            return string.Format(format.Value, arg0, arg1, arg2);
        }

        public static MarkdownString Format(MarkdownString format, params object[] args)
        {
            return string.Format(format.Value, args);
        }

        public static MarkdownString Format(MarkdownString format, object arg0, object arg1)
        {
            return string.Format(format.Value, arg0, arg1);
        }

        public static MarkdownString Format(IFormatProvider provider, MarkdownString format, object arg0, object arg1, object arg2)
        {
            return string.Format(provider, format.Value, arg0, arg1, arg2);
        }

        public static MarkdownString Format(IFormatProvider provider, MarkdownString format, params object[] args)
        {
            return string.Format(provider, format.Value, args);
        }

        public static MarkdownString Format(IFormatProvider provider, MarkdownString format, object arg0, object arg1)
        {
            return string.Format(provider, format.Value, arg0, arg1);
        }

        public static MarkdownString Format(IFormatProvider provider, MarkdownString format, object arg0)
        {
            return string.Format(provider, format.Value, arg0);
        }

        [SecuritySafeCritical]
        public static MarkdownString Intern(MarkdownString str)
        {
            return string.Intern(str.Value);
        }

        [SecuritySafeCritical]
        public static MarkdownString IsInterned(MarkdownString str)
        {
            return string.IsInterned(str.Value);
        }

        public static bool IsNullOrEmpty(MarkdownString value)
        {
            return string.IsNullOrEmpty(value.Value);
        }

        public static bool IsNullOrWhiteSpace(MarkdownString value)
        {
            return string.IsNullOrWhiteSpace(value.Value);
        }

        [ComVisible(false)]
        public static MarkdownString Join<T>(MarkdownString separator, IEnumerable<T> values)
        {
            return string.Join(separator.Value, values);
        }

        [ComVisible(false)]
        public static MarkdownString Join(MarkdownString separator, IEnumerable<MarkdownString> values)
        {
            return string.Join(separator.Value, values);
        }

        [SecuritySafeCritical]
        public static MarkdownString Join(MarkdownString separator, MarkdownString[] value, int startIndex, int count)
        {
            return string.Join(separator.Value, value.Select(val => val.Value), startIndex, count);
        }

        public static MarkdownString Join(MarkdownString separator, params MarkdownString[] value)
        {
            return string.Join(separator.Value, value.Select(val => val.Value));
        }

        [ComVisible(false)]
        public static MarkdownString Join(MarkdownString separator, params object[] values)
        {
            return string.Join(separator.Value, values);
        }

        public object Clone()
        {
            return new MarkdownString(Value);
        }

        public bool Contains(MarkdownString value)
        {
            return Value.Contains(value.Value);
        }

        [SecuritySafeCritical]
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            Value.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public bool EndsWith(MarkdownString value)
        {
            return Value.EndsWith(value.Value);
        }

        [ComVisible(false)]
        [SecuritySafeCritical]
        public bool EndsWith(MarkdownString value, StringComparison comparisonType)
        {
            return Value.EndsWith(value.Value, comparisonType);
        }

        public bool EndsWith(MarkdownString value, bool ignoreCase, CultureInfo culture)
        {
            return Value.EndsWith(value.Value, ignoreCase, culture);
        }

        [SecuritySafeCritical]
        public bool Equals(MarkdownString value, StringComparison comparisonType)
        {
            return Value.Equals(value.Value, comparisonType);
        }

        public CharEnumerator GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [SecuritySafeCritical]
        public TypeCode GetTypeCode()
        {
            return Value.GetTypeCode();
        }

        [SecuritySafeCritical]
        public int IndexOf(char value, int startIndex, int count)
        {
            return Value.IndexOf(value, startIndex, count);
        }

        public int IndexOf(char value, int startIndex)
        {
            return Value.IndexOf(value, startIndex);
        }


        public int IndexOf(MarkdownString value)
        {
            return Value.IndexOf(value.Value);
        }

        public int IndexOf(MarkdownString value, int startIndex)
        {
            return Value.IndexOf(value.Value, startIndex);
        }

        public int IndexOf(MarkdownString value, int startIndex, int count)
        {
            return Value.IndexOf(value.Value, startIndex, count);
        }

        public int IndexOf(MarkdownString value, StringComparison comparisonType)
        {
            return Value.IndexOf(value.Value, comparisonType);
        }

        public int IndexOf(MarkdownString value, int startIndex, StringComparison comparisonType)
        {
            return Value.IndexOf(value.Value, startIndex, comparisonType);
        }

        public int IndexOf(char value)
        {
            return Value.IndexOf(value);
        }

        [SecuritySafeCritical]
        public int IndexOf(MarkdownString value, int startIndex, int count, StringComparison comparisonType)
        {
            return Value.IndexOf(value.Value, startIndex, count, comparisonType);
        }

        [SecuritySafeCritical]
        public int IndexOfAny(char[] anyOf, int startIndex, int count)
        {
            return Value.IndexOfAny(anyOf, startIndex, count);
        }

        public int IndexOfAny(char[] anyOf, int startIndex)
        {
            return Value.IndexOfAny(anyOf, startIndex);
        }

        public int IndexOfAny(char[] anyOf)
        {
            return Value.IndexOfAny(anyOf);
        }

        [SecuritySafeCritical]
        public MarkdownString Insert(int startIndex, MarkdownString value)
        {
            return Value.Insert(startIndex, value.Value);
        }

        public bool IsNormalized()
        {
            return Value.IsNormalized();
        }

        [SecuritySafeCritical]
        public bool IsNormalized(NormalizationForm normalizationForm)
        {
            return Value.IsNormalized(normalizationForm);
        }

        [SecuritySafeCritical]
        public int LastIndexOf(MarkdownString value, int startIndex, int count, StringComparison comparisonType)
        {
            return Value.LastIndexOf(value.Value, startIndex, count, comparisonType);
        }

        [SecuritySafeCritical]
        public int LastIndexOf(char value, int startIndex, int count)
        {
            return Value.LastIndexOf(value, startIndex, count);
        }

        public int LastIndexOf(char value, int startIndex)
        {
            return Value.LastIndexOf(value, startIndex);
        }

        public int LastIndexOf(char value)
        {
            return Value.LastIndexOf(value);
        }

        public int LastIndexOf(MarkdownString value, int startIndex)
        {
            return Value.LastIndexOf(value.Value, startIndex);
        }

        public int LastIndexOf(MarkdownString value, int startIndex, StringComparison comparisonType)
        {
            return Value.LastIndexOf(value.Value, startIndex, comparisonType);
        }

        public int LastIndexOf(MarkdownString value, int startIndex, int count)
        {
            return Value.LastIndexOf(value.Value, startIndex, count);
        }

        public int LastIndexOf(MarkdownString value, StringComparison comparisonType)
        {
            return Value.LastIndexOf(value.Value, comparisonType);
        }

        public int LastIndexOf(MarkdownString value)
        {
            return Value.LastIndexOf(value.Value);
        }

        public int LastIndexOfAny(char[] anyOf)
        {
            return Value.LastIndexOfAny(anyOf);
        }

        public int LastIndexOfAny(char[] anyOf, int startIndex)
        {
            return Value.LastIndexOfAny(anyOf, startIndex);
        }

        [SecuritySafeCritical]
        public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
        {
            return Value.LastIndexOfAny(anyOf, startIndex, count);
        }

        public MarkdownString Normalize()
        {
            return Value.Normalize();
        }

        [SecuritySafeCritical]
        public MarkdownString Normalize(NormalizationForm normalizationForm)
        {
            return Value.Normalize(normalizationForm);
        }

        public MarkdownString PadLeft(int totalWidth, char paddingChar)
        {
            return Value.PadLeft(totalWidth, paddingChar);
        }

        public MarkdownString PadLeft(int totalWidth)
        {
            return Value.PadLeft(totalWidth);
        }

        public MarkdownString PadRight(int totalWidth, char paddingChar)
        {
            return Value.PadRight(totalWidth, paddingChar);
        }

        public MarkdownString PadRight(int totalWidth)
        {
            return Value.PadRight(totalWidth);
        }

        public MarkdownString Remove(int startIndex)
        {
            return Value.Remove(startIndex);
        }

        [SecuritySafeCritical]
        public MarkdownString Remove(int startIndex, int count)
        {
            return Value.Remove(startIndex, count);
        }

        public MarkdownString Replace(MarkdownString oldValue, MarkdownString newValue)
        {
            return Value.Replace(oldValue.Value, newValue.Value);
        }

        public MarkdownString Replace(char oldChar, char newChar)
        {
            return Value.Remove(oldChar, newChar);
        }

        public MarkdownString[] Split(params char[] separator)
        {
            return Value.Split(separator).Select(str => new MarkdownString(str)).ToArray();
        }

        public MarkdownString[] Split(char[] separator, int count)
        {
            return Value.Split(separator, count).Select(str => new MarkdownString(str)).ToArray();
        }

        [ComVisible(false)]
        public MarkdownString[] Split(char[] separator, StringSplitOptions options)
        {
            return Value.Split(separator, options).Select(str => new MarkdownString(str)).ToArray();
        }

        [ComVisible(false)]
        public MarkdownString[] Split(char[] separator, int count, StringSplitOptions options)
        {
            return Value.Split(separator).Select(str => new MarkdownString(str)).ToArray();
        }

        [ComVisible(false)]
        public MarkdownString[] Split(MarkdownString[] separator, StringSplitOptions options)
        {
            return Value.Split(separator.Select(mds => mds.Value).ToArray(), options).Select(str => new MarkdownString(str)).ToArray();
        }

        [ComVisible(false)]
        public MarkdownString[] Split(MarkdownString[] separator, int count, StringSplitOptions options)
        {
            return Value.Split(separator.Select(mds => mds.Value).ToArray(), count, options).Select(str => new MarkdownString(str)).ToArray();
        }

        [ComVisible(false)]
        [SecuritySafeCritical]
        public bool StartsWith(MarkdownString value, StringComparison comparisonType)
        {
            return Value.StartsWith(value.Value, comparisonType);
        }

        public bool StartsWith(MarkdownString value, bool ignoreCase, CultureInfo culture)
        {
            return Value.StartsWith(value.Value, ignoreCase, culture);
        }

        public bool StartsWith(MarkdownString value)
        {
            return Value.StartsWith(value.Value);
        }

        [SecuritySafeCritical]
        public MarkdownString Substring(int startIndex, int length)
        {
            return Value.Substring(startIndex, length);
        }

        public MarkdownString Substring(int startIndex)
        {
            return Value.Substring(startIndex);
        }

        [SecuritySafeCritical]
        public char[] ToCharArray(int startIndex, int length)
        {
            return Value.ToCharArray(startIndex, length);
        }

        [SecuritySafeCritical]
        public char[] ToCharArray()
        {
            return Value.ToCharArray();
        }

        public MarkdownString ToLower(CultureInfo culture)
        {
            return Value.ToLower(culture);
        }

        public MarkdownString ToLower()
        {
            return Value.ToLower();
        }

        public MarkdownString ToLowerInvariant()
        {
            return Value.ToLowerInvariant();
        }

        public override string ToString()
        {
            return Value;
        }

        public MarkdownString ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
        }

        public MarkdownString ToUpper()
        {
            return Value.ToUpper();
        }

        public MarkdownString ToUpper(CultureInfo culture)
        {
            return Value.ToUpper(culture);
        }

        public MarkdownString ToUpperInvariant()
        {
            return Value.ToUpperInvariant();
        }

        public MarkdownString Trim(params char[] trimChars)
        {
            return Value.Trim(trimChars);
        }

        public MarkdownString Trim()
        {
            return Value.Trim();
        }

        public MarkdownString TrimEnd(params char[] trimChars)
        {
            return Value.TrimEnd(trimChars);
        }

        public MarkdownString TrimStart(params char[] trimChars)
        {
            return Value.TrimStart(trimChars);
        }
        #endregion

        #region ValueType Operands
        public static bool operator ==(MarkdownString obj1, MarkdownString obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator ==(MarkdownString obj1, string obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator ==(string obj1, MarkdownString obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(string obj1, MarkdownString obj2)
        {
            return !obj1.Equals(obj2);
        }

        public static bool operator !=(MarkdownString obj1, MarkdownString obj2)
        {
            return !obj1.Equals(obj2);
        }

        public static bool operator !=(MarkdownString obj1, string obj2)
        {
            return !obj1.Equals(obj2);
        }

        public static MarkdownString operator +(MarkdownString obj1, MarkdownString obj2)
        {
            return new MarkdownString(obj1.Value + obj2.Value);
        }

        public static MarkdownString operator +(MarkdownString obj1, string obj2)
        {
            return new MarkdownString(obj1.Value + obj2);
        }

        public static MarkdownString operator +(string obj1, MarkdownString obj2)
        {
            return new MarkdownString(obj1 + obj2.Value);
        }
        #endregion

        #region Self Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(MarkdownString other)
        {
            if (string.IsNullOrEmpty(other))
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.Value.Equals(Value))
                    {
                        return 1;
                    }

                    return 0;
                }
                catch
                {
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(MarkdownString other)
        {
            if (other != default(MarkdownString))
            {
                try
                {
                    return other.GetType() == GetType() && other.Value.Equals(Value);
                }
                catch
                {

                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(MarkdownString x, MarkdownString y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(MarkdownString obj)
        {
            return obj.Value.GetHashCode();
        }
        #endregion

        #region Object Type Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.Equals(Value))
                    {
                        return 1;
                    }

                    return 0;
                }
                catch
                {

                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public override bool Equals(object other)
        {
            if (other != null)
            {
                try
                {
                    return other.Equals(Value);
                }
                catch
                {

                }
            }

            return false;
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        #endregion

        #region Value Type Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(string other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.Equals(Value))
                    {
                        return 1;
                    }

                    return 0;
                }
                catch
                {

                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(string other)
        {
            if (other != default)
            {
                try
                {
                    return other.Equals(Value);
                }
                catch
                {
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(string x, string y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
        #endregion
    }
}
