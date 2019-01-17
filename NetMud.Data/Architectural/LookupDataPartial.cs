using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Architectural
{
    /// <summary>
    /// Partial for anything that is considered lookup data (non-entity backing data)
    /// </summary>
    public abstract class LookupDataPartial : TemplatePartial, ILookupData
    {
        /// <summary>
        /// Extra text for the help command to display
        /// </summary>
        [StringDataIntegrity("Help text empty.", warning: true)]
        [MarkdownStringLengthValidator(ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType("Markdown")]
        [Display(Name = "Help Text", Description = "The descriptive text shown in the public help pages and when HELP is used in game.")]
        [Required]
        [MarkdownBinder]
        public MarkdownString HelpText { get; set; }

        /// <summary>
        /// Make a new one of these
        /// </summary>
        public LookupDataPartial()
        {
            //empty instance for getting the dataTableName
        }

        /// <summary>
        /// Render out the display for the help command
        /// </summary>
        /// <returns>Help text</returns>
        public virtual IEnumerable<string> RenderHelpBody()
        {
            List<string> sb = new List<string>
            {
                 HelpText
            };

            return sb;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Help", HelpText);

            return returnList;
        }

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        public override CacheType CachingType => CacheType.LookupData;
        #endregion

        public override object Clone()
        {
            throw new NotImplementedException("Not much point cloning generics.");
        }
    }
}
