namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Word/phrase types
    /// </summary>
    public enum LexicalType : short
    {
        None = -1,
        Noun = 0,
        Pronoun = 1,
        Verb = 2,
        Adjective = 3,
        Adverb = 4,
        Conjunction = 5,
        Interjection = 6,
        ProperNoun = 7,
        Article = 8
    }
}
