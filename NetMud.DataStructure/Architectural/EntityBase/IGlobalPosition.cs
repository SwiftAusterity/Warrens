using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using System;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Coords + world designator
    /// </summary>
    public interface IGlobalPosition : ICloneable
    {
        /// <summary>
        /// Current location this entity is in
        /// </summary>
        IZone CurrentZone { get; set; }

        /// <summary>
        /// Current container this entity is in
        /// </summary>
        IContains CurrentContainer { get; set; }

        /// <summary>
        /// What tile someone is on in the zone
        /// </summary>
        Coordinate CurrentCoordinates { get; set; }

        /// <summary>
        /// make a copy of this
        /// </summary>
        /// <returns>a clone of this</returns>
        IGlobalPosition Clone(Coordinate coordinates);

        /// <summary>
        /// The tile of the current location
        /// </summary>
        /// <returns>The room, might be null</returns>
        ITile GetTile();

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        string MoveFrom(IEntity thing);

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of entity to move</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        string MoveInto(IEntity thing);
    }
}
