using NetMud.Data.Serialization;
using NetMud.DataAccess;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NetMud.Data.System
{
    [Serializable]
    public class Lexica : ILexica
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
        /// Modifiers for this lexica. (Modifier, Conjunction)
        /// </summary>
        public HashSet<ILexica> Modifiers { get; set; }

        public Lexica()
        {
            Modifiers = new HashSet<ILexica>();
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <param name="conjunction">the joining text</param>
        /// <returns>Whether or not it succeeded</returns>
        public bool TryModify(ILexica modifier)
        {
            if (Modifiers.Contains(modifier))
                return false;

            Modifiers.Add(modifier);

            return true;
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
        public int CompareTo(ILexica other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.Phrase.Equals(Phrase))
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
        public bool Equals(ILexica other)
        {
            if (other != default(ILexica))
            {
                try
                {
                    return other.GetType() == GetType() && other.Phrase.Equals(Phrase);
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
