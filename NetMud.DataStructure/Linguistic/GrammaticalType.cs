namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// The type of grammatical parts of a sentence
    /// </summary>
    public enum GrammaticalType : short
    {
        None = -1,
        Subject = 0,
        DirectObject = 1,
        IndirectObject = 2,
        Verb = 3,
        Descriptive = 4
    }
}
