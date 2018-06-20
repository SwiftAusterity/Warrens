using NetMud.DataStructure.Base.PlayerConfiguration;
using System;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// The definition for a UI Module
    /// </summary>
    [Serializable]
    public class UIModule : LookupDataPartial, IUIModule
    {
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
    }
}
