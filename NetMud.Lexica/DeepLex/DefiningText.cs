using System;

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

        public BiographicalNameWrap[] bnw { get; set; }

        public CalledAlsoNote ca { get; set; }

        public SupplementalInformationNote snote { get; set; }
        //uns

        public UsageNotes[] uns { get; set; }
    }
}
