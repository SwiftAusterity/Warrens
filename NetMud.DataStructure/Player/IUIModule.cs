using NetMud.DataStructure.Architectural;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// The definition for a UI Module
    /// </summary>
    public interface IUIModule : ILookupData
    {
        /// <summary>
        /// The content to load in
        /// </summary>
        MarkdownString BodyHtml { get; set; }

        /// <summary>
        /// If made into a popout what is the height of the window
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// If made into a popout what is the width of the window
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Did a player make this or is this staff made?
        /// </summary>
        int SystemDefault { get; set; }
    }
}
