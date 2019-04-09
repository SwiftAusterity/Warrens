using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Pronounciation
    {
        /// <summary>
        /// Linguistics
        /// </summary>
        public string mw { get; set; }

        /// <summary>
        /// Label before pronounciation
        /// </summary>
        public string l { get; set; }

        /// <summary>
        /// Label after pronounciation
        /// </summary>
        public string l2 { get; set; }

        /// <summary>
        /// punctuation to separate pronunciation objects
        /// </summary>
        public string pun { get; set; }

        /// <summary>
        /// Audio file references for the word
        /// </summary>
        public PronounciationSound sound { get; set; }
    }
}
