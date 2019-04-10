using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class UsageNotes
    {
        public string[] text { get; set; }

        public VerbalIllustration vis { get; set; }

        public RunIn ri { get; set; }
    }
}
