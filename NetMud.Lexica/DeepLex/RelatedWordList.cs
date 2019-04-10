using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class RelatedWordList
    {
        public string wd { get; set; }

        public ThesaurusVariant[] wvrs { get; set; }

        public ThesaurusVerbVariant[] wvbvrs { get; set; }

        public ThesaurusStatusLabel[] wsls { get; set; }
    }

}
