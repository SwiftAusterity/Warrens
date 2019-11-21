using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class SupplementalInformationNote
    {
        public List<string> t { get; set; }

        public VerbalIllustration vis { get; set; }

        public RunIn ri { get; set; }

        public SupplementalInformationNote()
        {
            t = new List<string>();
        }
    }
}
