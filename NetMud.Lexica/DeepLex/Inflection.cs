using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Inflection
    {
        /// <summary>
        /// Inflection
        /// </summary>
        public string If { get; set; }

        /// <summary>
        /// Inflection cutback
        /// </summary>
        public string ifc { get; set; }

        /// <summary>
        /// Inflection label
        /// </summary>
        public string il { get; set; }

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
