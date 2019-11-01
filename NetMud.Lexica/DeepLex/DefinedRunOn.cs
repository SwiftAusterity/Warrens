using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class DefinedRunOn
    {
        public string drp { get; set; }

        /// <summary>
        /// The definition section groups together all the sense sequences and verb dividers for a headword or defined run-on phrase.
        /// </summary>
        public Definition[] def { get; set; }

        /// <summary>
        /// General labels, usage specifics
        /// </summary>
        public string[] lbs { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public Pronounciation[] prs { get; set; }

        /// <summary>
        /// subject/status labels
        /// </summary>
        public string[] sls { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public Variant[] vrs { get; set; }

        /// <summary>
        /// parenthesized subject/status label (like regional usage)
        /// </summary>
        public string psl { get; set; }

        /// <summary>
        /// An etymology is an explanation of the historical origin of a word. While the etymology contained in an et most typically relates to the headword, it may also explain the origin of a defined run-on phrase or a particular sense.
        /// </summary>
        public Etymology[] et { get; set; }
    }
}
