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
        /// Only when the word ends with
        /// </summary>
        string WhenEndsWith { get; set; }

        /// <summary>
        /// Only when the word begins with
        /// </summary>
        string WhenBeginsWith { get; set; }

        /// <summary>
        /// Only applies when the context is possessive
        /// </summary>
        bool WhenPossessive { get; set; }

        /// <summary>
        /// Only applies when the context is plural
        /// </summary>
        bool WhenPlural { get; set; }

        /// <summary>
        /// Add this prefix
        /// </summary>
        string AddPrefix { get; set; }

        /// <summary>
        /// Add this suffix
        /// </summary>
        string AddSuffix { get; set; }

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
        /// Does this word require an Article added (like nouns preceeding or verbs anteceding, or non-english honorifics/possessive conjugations)
        /// </summary>
        bool NeedsArticle { get; set; }

        /// <summary>
        /// The presence of these criteria changes the sentence type
        /// </summary>
        SentenceType AltersSentence { get; set; }

        /// <summary>
        /// Rate this rule on how specific it is so we can run the more specific rules first
        /// </summary>
        /// <returns>Specificity rating, higher = more specific</returns>
        int RuleSpecificity();
    }
}
