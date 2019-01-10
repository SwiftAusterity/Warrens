using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
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
        /// Current location this entity is in
        /// </summary>
        ILocale CurrentLocale { get; set; }

        /// <summary>
        /// Current location this entity is in
        /// </summary>
        IRoom CurrentRoom { get; set; }

        /// <summary>
        /// Current container this entity is in
        /// </summary>
        IContains CurrentContainer { get; set; }

        /// <summary>
        /// Get the absolute lowest level thing this other thing is in
        /// </summary>
        /// <returns></returns>
        ILocation CurrentLocation();

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
