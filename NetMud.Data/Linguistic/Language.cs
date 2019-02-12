﻿using NetMud.Data.Architectural;
using NetMud.DataAccess;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// Sort of a partial class of Lexica so it can get stored more easily and work for the processor
    /// </summary>
    [Serializable]
    public class Language : ConfigData, ILanguage, IComparable<ILanguage>, IEquatable<ILanguage>, IEqualityComparer<ILanguage>
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.ReviewOnly;

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Language;

        /// <summary>
        /// The code google translate uses to identify this language
        /// </summary>
        [Display(Name = "Language Code", Description = "The language code Google Translate uses to identify this language.")]
        [DataType(DataType.Text)]
        public string GoogleLanguageCode { get; set; }

        /// <summary>
        /// Languages only used for input and output translation
        /// </summary>
        [Display(Name = "UI Only", Description = "Only for use in translating the input/output, not an 'in game' language.")]
        [UIHint("Boolean")]
        public bool UIOnly { get; set; }

        /// <summary>
        /// Does this language have gendered grammar (like most latin based)
        /// </summary>
        [Display(Name = "Gendered", Description = "Does this language have gendered grammar (like most latin based).")]
        [UIHint("Boolean")]
        public bool Gendered { get; set; }

        /// <summary>
        /// Does punctuation come at the beginning of a sentence? (spanish)
        /// </summary>
        [Display(Name = "Precedent Punctuation", Description = "Does punctuation come at the beginning of a sentence? (spanish)")]
        [UIHint("Boolean")]
        public bool PrecedentPunctuation { get; set; }

        /// <summary>
        /// Does punctuation come at the end of a sentence?
        /// </summary>
        [Display(Name = "Antecendent Punctuation", Description = "Does punctuation come at the end of a sentence?")]
        [UIHint("Boolean")]
        public bool AntecendentPunctuation { get; set; }

        /// <summary>
        /// List of grammatical rules to use in sentence construction
        /// </summary>
        [UIHint("GrammarRules")]
        public HashSet<IGrammarRule> Rules { get; set; }

        /// <summary>
        /// Rules for sentence construction
        /// </summary>
        [UIHint("SentenceRules")]
        public HashSet<SentenceGrammarRule> SentenceRules { get; set; }

        public Language()
        {
            Name = string.Empty;
            Rules = new HashSet<IGrammarRule>();
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("UIOnly", UIOnly.ToString());
            returnList.Add("PrecedentPunctuation", PrecedentPunctuation.ToString());
            returnList.Add("AntecendentPunctuation", AntecendentPunctuation.ToString());

            return returnList;
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
                return CompareTo(other as ILanguage);
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
        public int CompareTo(ILanguage other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return 1;
                    }

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
        public bool Equals(ILanguage other)
        {
            if (other != default(ILanguage))
            {
                try
                {
                    return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
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
        public bool Equals(ILanguage x, ILanguage y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(ILanguage obj)
        {
            return obj.GetType().GetHashCode() + obj.Name.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + Name.GetHashCode();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
