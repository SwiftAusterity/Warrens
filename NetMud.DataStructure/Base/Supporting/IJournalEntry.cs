using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Blog/PatchNotes/etc
    /// </summary>
    public interface IJournalEntry : ILookupData
    {
        /// <summary>
        /// The body of the post
        /// </summary>
        MarkdownString Body { get; set; }

        /// <summary>
        /// When this should be published
        /// </summary>
        DateTime PublishDate { get; set; }

        /// <summary>
        /// free-form tags used to associate entries to pages and filters
        /// </summary>
        string[] Tags { get; set; }

        /// <summary>
        /// Is this available to even unlogged in people. If true overrides MinimumReadLevel
        /// </summary>
        bool Public { get; set; }

        /// <summary>
        /// Is this visibile to only a select level of accounts?
        /// </summary>
        StaffRank MinimumReadLevel { get; set; }

        /// <summary>
        /// When this should be considered "archived"
        /// </summary>
        DateTime ExpireDate { get; set; }

        /// <summary>
        /// Force expiration
        /// </summary>
        bool Expired { get; set; }

        /// <summary>
        /// Check if something is, in fact, expired
        /// </summary>
        /// <returns>Whether it's expired or not</returns>
        bool IsExpired();

        /// <summary>
        /// Check if something is considered published (something can be published AND expired)
        /// </summary>
        /// <returns>Whether it's published or not</returns>
        bool IsPublished();
    }
}
