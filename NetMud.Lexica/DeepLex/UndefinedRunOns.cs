using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class UndefinedRunOns
    {
        public string ure { get; set; }

        //public List<UndefinedRunOnText> utxt { get; set; }

        /// <summary>
        /// Functional Label, word form
        /// </summary>
        public string fl { get; set; }

        /// <summary>
        /// General labels, usage specifics
        /// </summary>
        public List<string> lbs { get; set; }

        /// <summary>
        /// subject/status labels
        /// </summary>
        public List<string> sls { get; set; }

        /// <summary>
        /// Inflections
        /// </summary>
        public List<Inflection> ins { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public List<Variant> vrs { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public List<Pronounciation> prs { get; set; }

        /// <summary>
        /// parenthesized subject/status label (like regional usage)
        /// </summary>
        public string psl { get; set; }

        public UndefinedRunOns()
        {
            prs = new List<Pronounciation>();
            ins = new List<Inflection>();
            sls = new List<string>();
            lbs = new List<string>();
            //utxt = new List<UndefinedRunOnText>();
        }
    }
}
