using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    public interface IEntity : ILookable, IComparable<IEntity>, IEquatable<IEntity>
    {
        /// <summary>
        /// Indelible guid that helps the system figure out where stuff is, generated when the object is spawned into the world
        /// </summary>
        string BirthMark { get; }
        DateTime Birthdate { get; }
        string[] Keywords { get; set; }

        IData DataTemplate { get; }

        IContains CurrentLocation { get; set; }

        void SpawnNewInWorld();
        void SpawnNewInWorld(IContains spawnTo);

        void UpsertToLiveWorldCache();

        bool TriggerAIAction(IEnumerable<string> input, AITriggerType trigger = AITriggerType.Seen);

        Func<IEnumerable<string>, bool> WriteTo { get; set; }
    }

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
