using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.System
{
    public interface IMessage
    {
        /// <summary>
        /// Message to send to the acting entity
        /// </summary>
        IEnumerable<string> ToActor { get; set; }

        /// <summary>
        /// Message to send to the subject of the command
        /// </summary>
        IEnumerable<string> ToSubject { get; set; }

        /// <summary>
        /// Message to send to the target of the command
        /// </summary>
        IEnumerable<string> ToTarget { get; set; }

        /// <summary>
        /// Message to send to the origin location of the command/event
        /// </summary>
        IEnumerable<string> ToOrigin { get; set; }

        /// <summary>
        /// Message to send to the destination location of the command/event
        /// </summary>
        IEnumerable<string> ToDestination { get; set; }

        /// <summary>
        /// Executes the messaging, sending messages using WriteTo on all relevant entities
        /// </summary>
        /// <param name="Actor">The acting entity</param>
        /// <param name="Subject">The command's subject entity</param>
        /// <param name="Target">The command's target entity</param>
        /// <param name="OriginLocation">The location the acting entity acted in</param>
        /// <param name="DestinationLocation">The location the command is targetting</param>
        void ExecuteMessaging(IEntity Actor, IEntity Subject, IEntity Target, IEntity OriginLocation, IEntity DestinationLocation);
    }
}
