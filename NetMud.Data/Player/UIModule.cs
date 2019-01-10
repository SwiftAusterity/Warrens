using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public MarkdownString BodyHtml { get; set; }

        /// <summary>
        /// If made into a popout what is the height of the window
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// If made into a popout what is the width of the window
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Did a player make this or is this staff made?
        /// </summary>
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
