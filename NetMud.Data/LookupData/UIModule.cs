using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Behaviors.System;
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
        public string BodyHtml { get; set; }

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
        public bool PlayerMade { get; set; }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Height", Height.ToString());
            returnList.Add("Width", Width.ToString());

            returnList.Add("BodyHtml", BodyHtml);

            return returnList;
        }
    }
}
