using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class UndefinedRunOnText
    {
        public VerbalIllustration vis { get; set; }

        public UsageNotes[] uns { get; set; }
    }
}
