using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class UndefinedRunOns
    {
        public string ure { get; set; }

        public UndefinedRunOnText[] utxt { get; set; }

        /// <summary>
        /// Functional Label, word form
        /// </summary>
        public string fl { get; set; }

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
        /// parenthesized subject/status label (like regional usage)
        /// </summary>
        public string psl { get; set; }
    }
}
