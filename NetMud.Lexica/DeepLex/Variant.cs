using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Variant
    {
        /// <summary>
        /// Variant
        /// </summary>
        public string va { get; set; }

        /// <summary>
        /// Variant Label
        /// </summary>
        public string vl { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public Pronounciation[] prs { get; set; }

        /// <summary>
        /// SENSE-SPECIFIC INFLECTION PLURAL LABEL
        /// </summary>
        public string spl { get; set; }
    }
}
