using NetMud.Data.DataIntegrity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Blog/PatchNotes/etc
    /// </summary>
    [Serializable]
    public class JournalEntry : LookupDataPartial, IJournalEntry
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
        [StringDataIntegrity("Body must be between 20 and 2000 characters", 20, 2000)]
        public MarkdownString Body { get; set; }

        /// <summary>
        /// When this should be published
        /// </summary>
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// free-form tags used to associate entries to pages and filters
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// Is this available to even unlogged in people. If true overrides MinimumReadLevel
        /// </summary>
        public bool Public { get; set; }

        /// <summary>
        /// Is this visibile to only a select level of accounts?
        /// </summary>
        public StaffRank MinimumReadLevel { get; set; }

        /// <summary>
        /// When this should be considered "archived"
        /// </summary>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// Force expiration
        /// </summary>
        public bool Expired { get; set; }

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
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Subject", Name);
            returnList.Add("Body", Body);
            returnList.Add("Publish Date", PublishDate.ToString());
            returnList.Add("Expire Date", ExpireDate.ToString());
            returnList.Add("Force Expired", Expired.ToString());
            returnList.Add("Public", Public.ToString());
            returnList.Add("Minimum Read Level", MinimumReadLevel.ToString());

            foreach (var tag in Tags)
                returnList.Add("Tag", tag);

            return returnList;
        }
    }
}
