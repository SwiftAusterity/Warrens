namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Word/phrase types
    /// </summary>
    public enum SentenceType : short
    {
        Partial = 0,
        Statement = 1,
        Question = 2,
        Exclamation = 3,
        ExclamitoryQuestion = 4
    }
}
