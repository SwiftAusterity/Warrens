using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Framework for live entities
    /// </summary>
    public interface IEntity : IExist, ICanSee, ICanSpeak, IFileStored, ILiveData, IAmOwned, IDescribable
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
        /// Update this to the live cache
        /// </summary>
        void UpsertToLiveWorldCache(bool forceSave = false);

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        bool WriteTo(IEnumerable<string> output);

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

        /// <summary>
        /// Kickoff any internal timers that need to happen
        /// </summary>
        void KickoffProcesses();
    }
}
