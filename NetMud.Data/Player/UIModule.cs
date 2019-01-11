using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// The definition for a UI Module
    /// </summary>
    [Serializable]
    public class UIModule : LookupDataPartial, IUIModule
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

        /// <summary>
        /// The content to load in
        /// </summary>
        [StringLength(8000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [Display(Name = "Body Content (HTML)", Description = "HTML, css and javascript which drives your module.")]
        [DataType(DataType.MultilineText)]
        public MarkdownString BodyHtml { get; set; }

        /// <summary>
        /// If made into a popout what is the height of the window
        /// </summary>
        [Range(100, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height", Description = "The default height of the window your module will spawn in if put into a popup.")]
        [DataType(DataType.Text)]
        public int Height { get; set; }

        /// <summary>
        /// If made into a popout what is the width of the window
        /// </summary>
        [Range(100, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width", Description = "The default width of the window your module will spawn in if put into a popup.")]
        [DataType(DataType.Text)]
        public int Width { get; set; }

        /// <summary>
        /// Did a player make this or is this staff made?
        /// </summary>
        [Display(Name = "System Default", Description = "If newly registered accounts get this as a defaulted UI Module or not (and which quadrant it goes in).")]
        [DataType(DataType.Text)]
        public int SystemDefault { get; set; }

        public UIModule()
        {
            SystemDefault = -1;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Height", Height.ToString());
            returnList.Add("Width", Width.ToString());
            returnList.Add("SystemDefault", SystemDefault.ToString());

            returnList.Add("BodyHtml", BodyHtml);

            return returnList;
        }
    }
}
