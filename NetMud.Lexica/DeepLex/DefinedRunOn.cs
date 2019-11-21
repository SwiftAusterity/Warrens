using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class DefinedRunOn
    {
        public string drp { get; set; }

        /// <summary>
        /// The definition section groups together all the sense sequences and verb dividers for a headword or defined run-on phrase.
        /// </summary>
        public List<Definition> def { get; set; }

        /// <summary>
        /// General labels, usage specifics
        /// </summary>
        public List<string> lbs { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public List<Pronounciation> prs { get; set; }

        /// <summary>
        /// subject/status labels
        /// </summary>
        public List<string> sls { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public List<Variant> vrs { get; set; }

        /// <summary>
        /// parenthesized subject/status label (like regional usage)
        /// </summary>
        public string psl { get; set; }

        /// <summary>
        /// An etymology is an explanation of the historical origin of a word. While the etymology contained in an et most typically relates to the headword, it may also explain the origin of a defined run-on phrase or a particular sense.
        /// </summary>
        public List<Etymology> et { get; set; }

        public DefinedRunOn()
        {
            et = new List<Etymology>();
            vrs = new List<Variant>();
            sls = new List<string>();
            prs = new List<Pronounciation>();
            lbs = new List<string>();
            def = new List<Definition>();
        }
    }
}
