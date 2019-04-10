using NetMud.DataStructure.Architectural.ActorBase;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Locations are special containers for entities
    /// </summary>
    public interface ILocation : IContains
    {
        /// <summary>
        /// current maximum section
        /// </summary>
        ulong MaxSection { get; set; }

        /// <summary>
        /// Mobiles (NPC, Players) in the room
        /// </summary>
        IEntityContainer<IMobile> MobilesInside { get; set; }
    }
}
