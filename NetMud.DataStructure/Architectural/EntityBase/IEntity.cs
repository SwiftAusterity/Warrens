using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.NPC.IntelligenceControl;
using NetMud.DataStructure.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Framework for live entities
    /// </summary>
    public interface IEntity : IExist, ICanSee, ICanSmell, ICanHear, ICanSense, ICanSpeak, ICanTaste, ICanTouch, IFileStored, ILiveData, IAmOwned, IDescribable
    {
        /// <summary>
        /// Returns whether or not this is a player object
        /// </summary>
        /// <returns>if it is a player object</returns>
        bool IsPlayer();

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
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        Dimensions GetModelDimensions();

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        float GetModelVolume();

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
        bool WriteTo(IEnumerable<string> output, bool delayed = false);

        /// <summary>
        /// Buffer of output to send to clients via WriteTo
        /// </summary>
        IList<IEnumerable<string>> OutputBuffer { get; set; }

        /// <summary>
        /// Buffer of command string input sent from the client
        /// </summary>
        IList<string> InputBuffer { get; set; }

        /// <summary>
        /// What is currently being executed
        /// </summary>
        string CurrentAction { get; set; }

        /// <summary>
        /// Stops whatever is being executed and clears the input buffer
        /// </summary>
        void StopInput();

        /// <summary>
        /// Stops whatever is being executed, does not clear the input buffer
        /// </summary>
        void HaltInput();

        /// <summary>
        /// Clears the input buffer
        /// </summary>
        void FlushInput();

        /// <summary>
        /// Returns whats in the input buffer
        /// </summary>
        /// <returns>Any strings still in the input buffer</returns>
        IEnumerable<string> PeekInput();
        /// Update this entry by the system
        /// </summary>
        /// <returns>success status</returns>
        bool Save();

        /// <summary>
        /// Remove this entry
        /// </summary>
        /// <returns>success status</returns>
        bool Remove();

        /// <summary>
        /// Kickoff any internal timers that need to happen
        /// </summary>
        void KickoffProcesses();
    }
}
