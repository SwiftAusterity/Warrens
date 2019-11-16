using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    /// <summary>
    /// The defining text is the text of the definition or translation for a particular sense. 
    /// </summary>
    [Serializable]
    public class DefiningText
    {
        /// <summary>
        /// definition content
        /// </summary>
        public string text { get; set; }

        public VerbalIllustration vis { get; set; }

        public RunIn ri { get; set; }

        public List<BiographicalNameWrap> bnw { get; set; }

        public CalledAlsoNote ca { get; set; }

        public SupplementalInformationNote snote { get; set; }

        public List<UsageNotes> uns { get; set; }

        public DefiningText()
        {
            uns = new List<UsageNotes>();
            bnw = new List<BiographicalNameWrap>();
        }
    }
}
