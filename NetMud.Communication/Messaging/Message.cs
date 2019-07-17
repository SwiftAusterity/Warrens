using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Communication.Messaging
{
    /// <summary>
    /// Used by the system to produce output for commands and events
    /// </summary>
    [Serializable]
    public class Message : IMessage
    {
        /// <summary>
        /// Message to send to the acting entity
        /// </summary>
        public IEnumerable<string> ToActor { get; set; }

        /// <summary>
        /// Message to send to the subject of the command
        /// </summary>
        public IEnumerable<string> ToSubject { get; set; }

        /// <summary>
        /// Message to send to the target of the command
        /// </summary>
        public IEnumerable<string> ToTarget { get; set; }

        /// <summary>
        /// Message to send to the origin location of the command/event
        /// </summary>
        public IEnumerable<string> ToOrigin { get; set; }

        /// <summary>
        /// Message to send to the destination location of the command/event
        /// </summary>
        public IEnumerable<string> ToDestination { get; set; }

        /// <summary>
        /// New up an empty cluster
        /// </summary>
        public Message()
        {
            ToActor = Enumerable.Empty<string>();
            ToSubject = Enumerable.Empty<string>();
            ToTarget = Enumerable.Empty<string>();
            ToOrigin = Enumerable.Empty<string>();
            ToDestination = Enumerable.Empty<string>();
        }

        /// <summary>
        /// New up a clister with just toactor for system messages
        /// </summary>
        public Message(string toActor)
        {
            ToActor = new List<string> { toActor };
            ToSubject = Enumerable.Empty<string>();
            ToTarget = Enumerable.Empty<string>();
            ToOrigin = Enumerable.Empty<string>();
            ToDestination = Enumerable.Empty<string>();
        }

        /// <summary>
        /// New up a clister with just toactor for system messages
        /// </summary>
        public Message(IEnumerable<string> toActor)
        {
            ToActor = toActor;
            ToSubject = Enumerable.Empty<string>();
            ToTarget = Enumerable.Empty<string>();
            ToOrigin = Enumerable.Empty<string>();
            ToDestination = Enumerable.Empty<string>();
        }

        /// <summary>
        /// New up a full cluster
        /// </summary>
        /// <param name="actor">Message to send to the acting entity</param>
        /// <param name="subject">Message to send to the subject of the command</param>
        /// <param name="target">Message to send to the target of the command</param>
        /// <param name="origin">Message to send to the origin location of the command/event</param>
        /// <param name="destination">Message to send to the destination location of the command/event</param>
        public Message(IEnumerable<string> actor, IEnumerable<string> subject, IEnumerable<string> target,
            IEnumerable<string> origin, IEnumerable<string> destination)
        {
            ToActor = actor;
            ToSubject = subject;
            ToTarget = target;
            ToOrigin = origin;
            ToDestination = destination;
        }

        /// <summary>
        /// Executes the messaging, sending messages using WriteTo on all relevant entities
        /// </summary>
        /// <param name="Actor">The acting entity</param>
        /// <param name="Subject">The command's subject entity</param>
        /// <param name="Target">The command's target entity</param>
        /// <param name="OriginLocation">The location the acting entity acted in</param>
        /// <param name="DestinationLocation">The location the command is targetting</param>
        public void ExecuteMessaging(IEntity Actor, IEntity Subject, IEntity Target, IGlobalPosition OriginLocation, IGlobalPosition DestinationLocation, ulong radius, bool coallate = false)
        {
            Dictionary<MessagingTargetType, IEntity[]> entities = new Dictionary<MessagingTargetType, IEntity[]>
            {
                { MessagingTargetType.Actor, new IEntity[] { Actor } },
                { MessagingTargetType.Subject, new IEntity[] { Subject } },
                { MessagingTargetType.Target, new IEntity[] { Target } }
            };

            if (Actor != null && ToActor.Any())
            {
                Actor.WriteTo(TranslateOutput(ToActor, entities), coallate);
            }

            if (Subject != null && ToSubject.Any())
            {
                Subject.WriteTo(TranslateOutput(ToSubject, entities), coallate);
            }

            if (Target != null && ToTarget.Any())
            {
                Target.WriteTo(TranslateOutput(ToTarget, entities), coallate);
            }

            //TODO: origin and destination are areas of effect on their surrounding areas
            if (OriginLocation != null && ToOrigin.Any())
            {
                IEnumerable<IEntity> validContents = OriginLocation.GetContents(radius).Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target));

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach (IEntity dude in validContents)
                {
                    dude.WriteTo(TranslateOutput(ToOrigin, entities), coallate);
                }
            }

            if (DestinationLocation != null && ToDestination.Any())
            {
                IEnumerable<IEntity> validContents = DestinationLocation.GetContents(radius).Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target));

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach (IEntity dude in validContents)
                {
                    dude.WriteTo(TranslateOutput(ToDestination, entities), coallate);
                }
            }
        }

        //TODO: Sentence combinatory logic for lexica output

        /// <summary>
        /// Translates output text with color codes and entity variables
        /// </summary>
        /// <param name="output">the output text to translate</param>
        /// <param name="entities">relevant entities for the variables transform</param>
        /// <returns>translated output</returns>
        private IEnumerable<string> TranslateOutput(IEnumerable<string> output, Dictionary<MessagingTargetType, IEntity[]> entities)
        {
            return MessagingUtility.TranslateEntityVariables(output.ToArray(), entities);
        }
    }

}
