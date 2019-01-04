using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.NPC.IntelligenceControl;
using NetMud.DataStructure.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Framework for live entities
    /// </summary>
    public interface IEntity : IExist, ICanSee, IFileStored, ILiveData, IAmOwned
    {
        /// <summary>
        /// Keywords this entity can be found with in command parsing
        /// </summary>
        string[] Keywords { get; set; }

        /// <summary>
        /// The Id for the data template
        /// </summary>
        long TemplateId { get; set; }

        /// <summary>
        /// The keyword name of the object in the data template
        /// </summary>
        string TemplateName { get; }

        /// <summary>
        /// How this entity communicates with the system
        /// </summary>
        IChannelType ConnectionType { get; }

        /// <summary>
        /// The backing data for this entity in the db
        /// </summary>
        T Template<T>() where T : IKeyedData;

        /// <summary>
        /// Update this to the live cache
        /// </summary>
        void UpsertToLiveWorldCache(bool forceSave = false);

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
        /// Update this entry by the system
        /// </summary>
        /// <returns>success status</returns>
        bool Save();

        /// <summary>
        /// Remove this entry
        /// </summary>
        /// <returns>success status</returns>
        bool Remove();
    }
}
