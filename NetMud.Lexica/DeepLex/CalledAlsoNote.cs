using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class CalledAlsoNote
    {
        public string intro { get; set; }

        public CalledAlsoNoteTarget[] cats { get; set; }
    }
}
