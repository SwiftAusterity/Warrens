using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class UsageSection
    {
        /// <summary>
        /// paragraph label: heading to display at top of section
        /// </summary>
        public string pl { get; set; }

        //public List<ParagraphText> pt { get; set; }

        public UsageSection()
        {
            //pt = new List<ParagraphText>();
        }
    }
}
