using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class SupplementalInformationNote
    {
        public string[] t { get; set; }

        public VerbalIllustration vis { get; set; }

        public RunIn ri { get; set; }
    }
}
