namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Relational rule From sentence construction
    /// </summary>
    public interface IGrammarRule
    {
        /// <summary>
        /// Rule applies when sentence is in this tense
        /// </summary>
        LexicalTense Tense { get; set; }

        /// <summary>
        /// Rule applies when sentence is in this perspective
        /// </summary>
        NarrativePerspective Perspective { get; set; }

        /// <summary>
        /// When the From word is this or a synonym of this (only native synonyms) this rule applies
        /// </summary>
        IDictata SpecificWord { get; set; }

        /// <summary>
        /// Applies when this type of word is the primary one
        /// </summary>
        LexicalType FromType { get; set; }

        /// <summary>
        /// This rule applies when the word is this role
        /// </summary>
        GrammaticalType FromRole { get; set; }

        /// <summary>
        /// When the origin word has this semantic tag
        /// </summary>
        string FromSemantics { get; set; }

        /// <summary>
        /// Applies when we're trying to figure out where to put this type of word
        /// </summary>
        LexicalType ToType { get; set; }

        /// <summary>
        /// This rule applies when the adjunct word is this role
        /// </summary>
        GrammaticalType ToRole { get; set; }

        /// <summary>
        /// When the modifying word has this semantic tag
        /// </summary>
        string ToSemantics { get; set; }

        /// <summary>
        /// Can be made into a list
        /// </summary>
        bool Listable { get; set; }

        /// <summary>
        /// Where does the To word fit around the From word? (the from word == 0)
        /// </summary>
        int ModificationOrder { get; set; }

        /// <summary>
        /// Does this word require an Article added (like nouns preceeding or verbs anteceding)
        /// </summary>
        bool NeedsArticle { get; set; }

        /// <summary>
        /// The presence of these criteria changes the sentence type
        /// </summary>
        SentenceType AltersSentence { get; set; }
    }
}
