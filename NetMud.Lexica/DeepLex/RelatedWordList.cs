using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class RelatedWordList
    {
        public string wd { get; set; }

        public List<ThesaurusVariant> wvrs { get; set; }

        public List<ThesaurusVerbVariant> wvbvrs { get; set; }

        public List<ThesaurusStatusLabel> wsls { get; set; }

        public RelatedWordList()
        {
            wvrs = new List<ThesaurusVariant>();
            wvbvrs = new List<ThesaurusVerbVariant>();
            wsls = new List<ThesaurusStatusLabel>();
        }
    }

}
