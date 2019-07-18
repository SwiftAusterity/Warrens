using NetMud.Data.Architectural;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyValidation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace NetMud.Data.Administrative
{
    /// <summary>
    /// Blog/PatchNotes/etc
    /// </summary>
    [ValidateInput(false)]
    [Serializable]
    public class JournalEntry : TemplatePartial, IJournalEntry
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } }

        /// <summary>
        /// The body of the post
        /// </summary>
        [AllowHtml]
        [StringDataIntegrity("Body must be between 20 and 2000 characters", 20, 2000)]
        [MarkdownStringLengthValidator(ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [Display(Name = "Body", Description = "The body content of the entry.")]
        [DataType("Markdown")]
        [Required]
        [MarkdownBinder]
        public MarkdownString Body { get; set; }

        /// <summary>
        /// When this should be published
        /// </summary>
        [Display(Name = "Publish On", Description = "The date this will be considered active and available to see.")]
        [DataType("Date")]
        [Required]
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// free-form tags used to associate entries to pages and filters
        /// </summary>
        [Display(Name = "Tags", Description = "Filtering tags such as Blog, Patch Notes, Update, etc.")]
        [UIHint("TagContainer")]
        public HashSet<string> Tags { get; set; }

        /// <summary>
        /// Is this available to even unlogged in people. If true overrides MinimumReadLevel
        /// </summary>
        [Display(Name = "Is Public?", Description = "Can this be seen by people who are not logged in. Overrides Minimum Read Level if true.")]
        [UIHint("Boolean")]
        public bool Public { get; set; }

        /// <summary>
        /// Is this visibile to only a select level of accounts?
        /// </summary>
        [Display(Name = "Minimum Read Level", Description = "Sets the minimum rank someone's account must be to see this.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public StaffRank MinimumReadLevel { get; set; }

        /// <summary>
        /// When this should be considered "archived"
        /// </summary>
        [Display(Name = "Expires On", Description = "The date this will be considered expired.")]
        [DataType("Date")]
        [Required]
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// Force expiration
        /// </summary>
        [Display(Name = "Force Expiry", Description = "If set to true will be considered expired no matter what the date is.")]
        [UIHint("Boolean")]
        public bool Expired { get; set; }

        public JournalEntry()
        {
            PublishDate = DateTime.Now.AddDays(1);
            ExpireDate = DateTime.Now.AddDays(30);
            Public = true;
            Expired = false;
            Body = string.Empty;
            MinimumReadLevel = StaffRank.Player;
            Name = string.Empty;
            Tags = new HashSet<string>();
        }

        /// <summary>
        /// Check if something is, in fact, expired
        /// </summary>
        /// <returns>Whether it's expired or not</returns>
        public bool IsExpired()
        {
            return Expired || ExpireDate < DateTime.Now;
        }

        /// <summary>
        /// Check if something is considered published (something can be published AND expired)
        /// </summary>
        /// <returns>Whether it's published or not</returns>
        public bool IsPublished()
        {
            return DateTime.Now > PublishDate && !IsExpired();
        }

        /// <summary>
        /// Check if this thing has a tag
        /// </summary>
        /// <param name="tagName">the tag we're looking for</param>
        /// <returns>if it has the tag</returns>
        public bool HasTag(string tagName)
        {
            return Tags.Any(tag => tagName.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Subject", Name);
            returnList.Add("Body", Body);
            returnList.Add("Publish Date", PublishDate.ToString());
            returnList.Add("Expire Date", ExpireDate.ToString());
            returnList.Add("Force Expired", Expired.ToString());
            returnList.Add("Public", Public.ToString());
            returnList.Add("Minimum Read Level", MinimumReadLevel.ToString());

            foreach (string tag in Tags)
            {
                returnList.Add("Tag", tag);
            }

            return returnList;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new JournalEntry
            {
                Name = Name,
                Body = Body,
                PublishDate = PublishDate,
                ExpireDate = ExpireDate,
                Expired = Expired,
                Public = Public,
                MinimumReadLevel = MinimumReadLevel,
                Tags = Tags
            };
        }
    }
}
