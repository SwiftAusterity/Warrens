using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class NearAntonymList
    {
        public string wd { get; set; }

        public ThesaurusStatusLabel wsls { get; set; }

        public List<ThesaurusStatusLabel> wvrs { get; set; }

        public List<ThesaurusVerbVariant> wvbvrs { get; set; }

        public NearAntonymList()
        {
            wvrs = new List<ThesaurusStatusLabel>();
            wvbvrs = new List<ThesaurusVerbVariant>();
        }
    }
}
