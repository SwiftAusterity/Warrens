using System;

namespace NetMud.Lexica.DeepLex
{
    /// <summary>
    /// The definition section groups together all the sense sequences and verb dividers for a headword or defined run-on phrase.
    /// </summary>
    [Serializable]
    public class Definition
    {
        public SenseSequence[] sseq { get; set; }

        /// <summary>
        /// Verb Divider: The verb divider acts as a functional label in verb entries, introducing the separate sense sequences for transitive and intransitive meanings of the verb.
        /// </summary>
        public string vd { get; set; }
    }
}
