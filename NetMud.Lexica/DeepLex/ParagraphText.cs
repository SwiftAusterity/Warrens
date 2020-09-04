using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class ParagraphText
    {
        /// <summary>
        /// definition content
        /// </summary>
        public List<string> text { get; set; }

        public VerbalIllustration vis { get; set; }

        /// <summary>
        /// usage see in addition reference: contains the text and ID of a "see in addition" reference to another usage section.
        /// </summary>
        public List<string> uaref { get; set; }

        public ParagraphText()
        {
            uaref = new List<string>();
        }
    }
}
