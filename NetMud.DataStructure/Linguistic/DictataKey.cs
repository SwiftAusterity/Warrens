using System;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// A key for referencing dictata
    /// </summary>
    [Serializable]
    public class DictataKey
    {
        /// <summary>
        /// the lexeme config cache key unique id
        /// </summary>
        public string LexemeKey { get; set; }

        /// <summary>
        /// The form id within the lexeme for the dictata
        /// </summary>
        public short FormId { get; set; }

        public DictataKey()
        {

        }

        public DictataKey(string lexemeKey, short formId)
        {
            LexemeKey = lexemeKey;
            FormId = formId;
        }
    }
}
