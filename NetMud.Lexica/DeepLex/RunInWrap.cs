using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class RunInWrap
    {
        /// <summary>
        /// Run-in entry word
        /// </summary>
        public string rie { get; set; }

        /// <summary>
        /// intervening text
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public Variant vrs { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public Pronounciation prs { get; set; }
    }
}
