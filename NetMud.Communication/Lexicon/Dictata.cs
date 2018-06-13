using NetMud.Communication.Messaging;
using NetMud.DataAccess;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;

namespace NetMud.Communication.Lexicon
{
    /// <summary>
    /// Sort of a partial class of Lexica so it can get stored more easily and work for the processor
    /// </summary>
    [Serializable]
    public class Dictata : IComparable<Dictata>, IEquatable<Dictata>
    {
        /// <summary>
        /// The type of word this is to the sentence
        /// </summary>
        public GrammaticalType Role { get; set; }

        /// <summary>
        /// The type of word this is in general
        /// </summary>
        public LexicalType Type { get; set; }

        /// <summary>
        /// The actual word/phrase
        /// </summary>
        public string Phrase { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        HashSet<Dictata> Synonyms { get; set; }

        /// <summary>
        /// Things this is specifically opposite of mostly
        /// </summary>
        HashSet<Dictata> Antonyms { get; set; }

        /// <summary>
        /// Get this in lexica form
        /// </summary>
        /// <returns>A Lexica with the same values</returns>
        public ILexica GetLexica()
        {
            return new Lexica(Type, Role, Phrase);
        }

        #region Equality Functions
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
            try
            {
                return CompareTo(other as Dictata);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return -99;
        }

        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(Dictata other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.Phrase.Equals(Phrase, StringComparison.InvariantCultureIgnoreCase) && other.Type == Type)
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(Dictata other)
        {
            if (other != default(Dictata))
            {
                try
                {
                    return other.Phrase.Equals(Phrase, StringComparison.InvariantCultureIgnoreCase) && other.Type == Type;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
        #endregion
    }
}
