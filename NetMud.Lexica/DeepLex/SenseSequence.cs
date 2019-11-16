using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class SenseSequence
    {
        public List<SenseBlock> sensb { get; set; }

        public SenseSequence()
        {
            sensb = new List<SenseBlock>();
        }
    }
}
