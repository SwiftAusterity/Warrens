using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Headword
    {
        /// <summary>
        /// The word
        /// </summary>
        public string hw { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public List<Pronounciation> prs { get; set; }

        /// <summary>
        /// parenthesized subject/status label (like regional usage)
        /// </summary>
        public string psl { get; set; }

        public Headword()
        {
            prs = new List<Pronounciation>();
        }
    }
}
