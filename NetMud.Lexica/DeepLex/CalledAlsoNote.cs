using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class CalledAlsoNote
    {
        public string intro { get; set; }

        public List<CalledAlsoNoteTarget> cats { get; set; }

        public CalledAlsoNote()
        {
            cats = new List<CalledAlsoNoteTarget>();
        }
    }
}
