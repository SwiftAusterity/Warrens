using NetMud.DataStructure.Architectural;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Where pathways lead to
    /// </summary>
    public interface IPathwayDestination
    {
        /// <summary>
        /// The keywords/name for the pathway
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Where this leads to
        /// </summary>
        IZoneTemplate Destination { get; set; }

        /// <summary>
        /// What tile this goes to
        /// </summary>
        Coordinate Coordinates { get; set; }
    }
}
