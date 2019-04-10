using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class ParagraphText
    {
        /// <summary>
        /// definition content
        /// </summary>
        public string text { get; set; }

        public VerbalIllustration vis { get; set; }

        /// <summary>
        /// usage see in addition reference: contains the text and ID of a "see in addition" reference to another usage section.
        /// </summary>
        public string[] uaref { get; set; }
    }
}
