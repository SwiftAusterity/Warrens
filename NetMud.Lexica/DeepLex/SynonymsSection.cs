using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class SynonymsSection
    {
        /// <summary>
        /// paragraph label: heading to display at top of section
        /// </summary>
        public string pl { get; set; }

        public ParagraphText[] pt { get; set; }
    }
}
