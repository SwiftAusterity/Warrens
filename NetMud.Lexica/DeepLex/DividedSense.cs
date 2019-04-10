using System;

namespace NetMud.Lexica.DeepLex
{
    /// <summary>
    /// A sense may be divided into two parts to show a particular relationship between two closely related meanings. 
    /// The second part of the sense is contained in an sdsense, consisting of an sense divider sd along with a second dt.
    /// </summary>
    [Serializable]
    public class DividedSense
    {
        /// <summary>
        /// Sense divider
        /// </summary>
        public string sd { get; set; }

        /// <summary>
        /// Defining Text
        /// </summary>
        public DefiningText dt { get; set; }

        /// <summary>
        /// General labels, usage specifics
        /// </summary>
        public string[] lbs { get; set; }

        /// <summary>
        /// subject/status labels
        /// </summary>
        public string[] sls { get; set; }

        /// <summary>
        /// Inflections
        /// </summary>
        public Inflection[] ins { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public Variant[] vrs { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public Pronounciation[] prs { get; set; }

        /// <summary>
        ///  SENSE-SPECIFIC GRAMMATICAL LABEL: This label notes whether a particular sense of a verb is transitive (T) or intransitive (I)
        /// </summary>
        public string sgram { get; set; }


        /// <summary>
        /// An etymology is an explanation of the historical origin of a word. While the etymology contained in an et most typically relates to the headword, it may also explain the origin of a defined run-on phrase or a particular sense.
        /// </summary>
        public Etymology[] et { get; set; }
    }
}
