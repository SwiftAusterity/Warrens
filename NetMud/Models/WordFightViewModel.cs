using NetMud.DataStructure.Linguistic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models
{
    public class WordFightViewModel
    {
        public IDictata WordOne { get; set; }
        public IDictata WordTwo { get; set; }

        /// <summary>
        /// Strength rating of word in relation to synonyms
        /// </summary>
        [Display(Name = "Severity", Description = "Strength (more of) rating.")]
        [UIHint("TrinaryBoolean")]
        public int Severity { get; set; }

        /// <summary>
        /// Synonym rating for elegance
        /// </summary>
        [Display(Name = "Elegance", Description = "Crudeness rating.")]
        [UIHint("TrinaryBoolean")]
        public int Elegance { get; set; }

        /// <summary>
        /// Finesse synonym rating; execution of form
        /// </summary>
        [Display(Name = "Quality", Description = "Finesse; quality of execution of form or function.")]
        [UIHint("TrinaryBoolean")]
        public int Quality { get; set; }
    }
}
