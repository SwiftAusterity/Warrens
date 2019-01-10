using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Room
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface IRoomFramework : IDescribable
    {
        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }

        /// <summary>
        /// What the room's primary material is (is it filled with water, air, etc)
        /// </summary>
        IMaterial Medium { get; set; }

        /// <summary>
        /// Current coordinates of the room on its world map
        /// </summary>
        Coordinate Coordinates { get; set; }
    }
}
