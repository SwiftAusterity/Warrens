using NetMud.DataAccess;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMud.Communication.Messaging
{
    /// <summary>
    /// A gramatical element
    /// </summary>
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
        /// Modifiers for this lexica
        /// </summary>
        public Dictionary<ILexica, string> Modifiers { get; }

        public Lexica()
        {
            Modifiers = new Dictionary<ILexica, string>();
        }

        public Lexica(LexicalType type, GrammaticalType role, string phrase)
        {
            Type = type;
            Phrase = phrase;
            Role = role;

            Modifiers = new Dictionary<ILexica, string>();
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <param name="conjunction">the joining text</param>
        /// <returns>Whether or not it succeeded</returns>
        public bool TryModify(ILexica modifier, string conjunction)
        {
            if (Modifiers.ContainsKey(modifier))
            {
                if (String.IsNullOrWhiteSpace(Modifiers[modifier]))
                    Modifiers[modifier] = conjunction;
                else
                    return false;
            }
            else
            {
                Modifiers.Add(modifier, conjunction);
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var adjectives = Modifiers.Where(mod => mod.Key.Role == GrammaticalType.Descriptive);

            switch (Role)
            {
                case GrammaticalType.Descriptive:
                    if (adjectives.Count() > 0)
                    {
                        if (Type == LexicalType.Conjunction || Type == LexicalType.Interjection)
                        {
                            sb.AppendFormat("{0} {1}", Phrase, adjectives.Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma));
                        }
                        else
                        {
                            sb.AppendFormat("{0}, {1}", Phrase, adjectives.Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma));
                        }
                    }
                    else
                    {
                        sb.Append(Phrase);
                    }
                    break;
                case GrammaticalType.IndirectObject:
                    sb.Append(AppendDescriptors(adjectives, Phrase));
                    break;
                case GrammaticalType.DirectObject:
                    var describedNoun = AppendDescriptors(adjectives, Phrase);

                    if (Modifiers.Any(mod => mod.Key.Role == GrammaticalType.IndirectObject))
                    {
                        var iObj = Modifiers.Where(mod => mod.Key.Role == GrammaticalType.IndirectObject)
                                            .Select(mod => String.Format("{0} {1}", mod.Value, mod.Key.ToString())).CommaList(RenderUtility.SplitListType.AllAnd);

                        sb.AppendFormat("{0} {1}", iObj, describedNoun);
                    }
                    else
                        sb.Append(describedNoun);

                    break;
                case GrammaticalType.Verb:
                    var adverbString = adjectives.Where(adj => adj.Key.Type == LexicalType.Adverb)
                                 .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma);

                    var adjectiveString = adjectives.Where(adj => adj.Key.Type == LexicalType.Adjective)
                                     .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.OxfordComma);

                    if(Modifiers.Any(mod => mod.Key.Role == GrammaticalType.DirectObject))
                    {
                        var dObj = Modifiers.Where(mod => mod.Key.Role == GrammaticalType.DirectObject)
                                            .Select(mod =>  mod.Key.ToString()).CommaList(RenderUtility.SplitListType.OxfordComma);

                        sb.AppendFormat("{2} {0} {1} {3}", Phrase, dObj, adverbString, adjectiveString);
                    }
                    else
                        sb.AppendFormat("{1} {0} {2}", Phrase, adverbString, adjectiveString);
                    break;
                case GrammaticalType.Subject:
                    var describedSubject = AppendDescriptors(adjectives, Phrase);

                    if (Modifiers.Any(mod => mod.Key.Role == GrammaticalType.Verb))
                    {
                        var vObj = Modifiers.Where(mod => mod.Key.Role == GrammaticalType.Verb)
                                            .Select(mod => mod.Key.ToString()).CommaList(RenderUtility.SplitListType.AllAnd);

                        sb.AppendFormat("{0} {1}", describedSubject, vObj);
                    }
                    else
                        sb.Append(describedSubject);
                    break;
            }

            return sb.ToString();
        }

        private string AppendDescriptors(IEnumerable<KeyValuePair<ILexica, string>> adjectives, string phrase)
        {
            var described = phrase;

            if (adjectives.Count() > 0)
            {
                var decorativeString = adjectives.Where(adj => adj.Key.Type != LexicalType.Conjunction && adj.Key.Type != LexicalType.Interjection)
                                                 .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma);

                var conjunctive = adjectives.FirstOrDefault(adj => adj.Key.Type == LexicalType.Conjunction || adj.Key.Type == LexicalType.Interjection);
                var conjunctiveString = conjunctive.Key != null ? conjunctive.Key.ToString() : string.Empty;

                described = String.Format("{1} {2} {0}", phrase, conjunctiveString, decorativeString);
            }

            return described;
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
                return CompareTo(other as ILexica);
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
        public int CompareTo(ILexica other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.Phrase.Equals(Phrase) && other.Type == Type)
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
                    return other.Phrase.Equals(Phrase) && other.Type == Type;
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
