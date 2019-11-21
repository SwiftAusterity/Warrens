using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class UsageNotes
    {
        public List<string> text { get; set; }

        public VerbalIllustration vis { get; set; }

        public RunIn ri { get; set; }

        public UsageNotes()
        {
            text = new List<string>();
        }
    }
}
