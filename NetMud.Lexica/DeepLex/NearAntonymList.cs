using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class NearAntonymList
    {
        public string wd { get; set; }

        public ThesaurusStatusLabel wsls { get; set; }

        public ThesaurusStatusLabel[] wvrs { get; set; }

        public ThesaurusVerbVariant[] wvbvrs { get; set; }
    }
}
