using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Locale;

namespace NetMud.DataStructure.Room
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface IRoomTemplate : IRoomFramework, ILocationData
    {
        /// <summary>
        /// Gets the remaining distance to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) between here and there</returns>
        int GetDistanceDestination(ILocationData destination);

        /// <summary>
        /// What locale does this belong to
        /// </summary>
        ILocaleTemplate ParentLocation { get; set; }
    }
}
