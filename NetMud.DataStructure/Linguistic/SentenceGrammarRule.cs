﻿using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Rules for sentence construction
    /// </summary>
    public class SentenceGrammarRule
    {
        /// <summary>
        /// Affects this fragment of the sentence
        /// </summary>
        [Display(Name = "Type", Description = "Affects this fragment of the sentence.")]
        [UIHint("EnumDropDownList")]
        public GrammaticalType Fragment { get; set; }

        /// <summary>
        /// Where does the To word fit around the From word? (the from word == 0)
        /// </summary>
        [Display(Name = "Placement Order", Description = " Where in the sentence does this fit? (starts with 0).")]
        [DataType(DataType.Text)]
        public int ModificationOrder { get; set; }

        /// <summary>
        /// For this sentence type
        /// </summary>
        [Display(Name = "Grammatical Role", Description = "For this sentence type specifically.")]
        [UIHint("EnumDropDownList")]
        public SentenceType Type { get; set; }
    }
}
