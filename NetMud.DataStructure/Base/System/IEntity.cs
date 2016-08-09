using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Framework for live entities
    /// </summary>
    public interface IEntity : ILookable, IComparable<IEntity>, IEquatable<IEntity>
    {
        /// <summary>
        /// Indelible guid that helps the system figure out where stuff is, generated when the object is spawned into the world
        /// </summary>
        string BirthMark { get; }

        /// <summary>
        /// When this was first added to the live world
        /// </summary>
        DateTime Birthdate { get; }

        /// <summary>
        /// Keywords this entity can be found with in command parsing
        /// </summary>
        string[] Keywords { get; set; }

        /// <summary>
        /// The backing data for this entity in the db
        /// </summary>
        IData DataTemplate { get; }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        Tuple<int, int, int> GetModelDimensions();

        /// <summary>
        /// Current location this entity is in
        /// </summary>
        IContains CurrentLocation { get; set; }

        /// <summary>
        /// Spawns a new instance of this entity in the live world
        /// </summary>
        void SpawnNewInWorld();

        /// <summary>
        /// Spawns a new instance of this entity in the live world to a specified container
        /// </summary>
        /// <param name="spawnTo">the container to spawn this into</param>
        void SpawnNewInWorld(IContains spawnTo);

        /// <summary>
        /// Update this to the live cache
        /// </summary>
        void UpsertToLiveWorldCache();

        /// <summary>
        /// Serialize this live entity to a binary stream
        /// </summary>
        /// <returns>binary stream</returns>
        byte[] Serialize();

        /// <summary>
        /// Deserialize a binary stream into this entity
        /// </summary>
        /// <param name="bytes">binary to deserialize</param>
        /// <returns>the entity</returns>
        IEntity DeSerialize(byte[] bytes);

        /// <summary>
        /// For non-player entities - accepts output "shown" to it by the parser as a result of commands and events
        /// </summary>
        /// <param name="input">the output strings</param>
        /// <param name="trigger">the methodology type (heard, seen, etc)</param>
        /// <returns></returns>
        bool TriggerAIAction(IEnumerable<string> input, AITriggerType trigger = AITriggerType.Seen);

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        bool WriteTo(IEnumerable<string> input);

        /// <summary>
        /// How this entity communicates with the system
        /// </summary>
        IChannelType ConnectionType { get; }
    }

    /// <summary>
    /// Trigger types for output sent to this entity
    /// </summary>
    public enum AITriggerType
    {
        SpokenTo,
        Heard,
        Seen,
        Sensed,
        PassiveActAt,
        AggressiveActAt
    }
}
