using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class CalledAlsoNoteTarget
    {
        public string cat { get; set; }
        public string catref { get; set; }
        public string pn { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public List<Pronounciation> prs { get; set; }

        /// <summary>
        /// parenthesized subject/status label (like regional usage)
        /// </summary>
        public string psl { get; set; }

        public CalledAlsoNoteTarget()
        {
            prs = new List<Pronounciation>();
        }
    }
}
