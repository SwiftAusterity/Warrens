using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
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
        public IEnumerable<ILexicalParagraph> ToActor { get; set; }

        /// <summary>
        /// Message to send to the subject of the command
        /// </summary>
        public IEnumerable<ILexicalParagraph> ToSubject { get; set; }

        /// <summary>
        /// Message to send to the target of the command
        /// </summary>
        public IEnumerable<ILexicalParagraph> ToTarget { get; set; }

        /// <summary>
        /// Message to send to the origin location of the command/event
        /// </summary>
        public IEnumerable<ILexicalParagraph> ToOrigin { get; set; }

        /// <summary>
        /// Message to send to the destination location of the command/event
        /// </summary>
        public IEnumerable<ILexicalParagraph> ToDestination { get; set; }

        /// <summary>
        /// New up an empty cluster
        /// </summary>
        public Message()
        {
            ToActor = Enumerable.Empty<ILexicalParagraph>();
            ToSubject = Enumerable.Empty<ILexicalParagraph>();
            ToTarget = Enumerable.Empty<ILexicalParagraph>();
            ToOrigin = Enumerable.Empty<ILexicalParagraph>();
            ToDestination = Enumerable.Empty<ILexicalParagraph>();
        }

        /// <summary>
        /// New up a clister with just toactor for system messages
        /// </summary>
        public Message(ILexicalParagraph toActor)
        {
            ToActor = new List<ILexicalParagraph> { toActor };
            ToSubject = Enumerable.Empty<ILexicalParagraph>();
            ToTarget = Enumerable.Empty<ILexicalParagraph>();
            ToOrigin = Enumerable.Empty<ILexicalParagraph>();
            ToDestination = Enumerable.Empty<ILexicalParagraph>();
        }

        /// <summary>
        /// New up a clister with just toactor for system messages
        /// </summary>
        public Message(IEnumerable<ILexicalParagraph> toActor)
        {
            ToActor = toActor;
            ToSubject = Enumerable.Empty<ILexicalParagraph>();
            ToTarget = Enumerable.Empty<ILexicalParagraph>();
            ToOrigin = Enumerable.Empty<ILexicalParagraph>();
            ToDestination = Enumerable.Empty<ILexicalParagraph>();
        }

        /// <summary>
        /// New up a full cluster
        /// </summary>
        /// <param name="actor">Message to send to the acting entity</param>
        /// <param name="subject">Message to send to the subject of the command</param>
        /// <param name="target">Message to send to the target of the command</param>
        /// <param name="origin">Message to send to the origin location of the command/event</param>
        /// <param name="destination">Message to send to the destination location of the command/event</param>
        public Message(IEnumerable<ILexicalParagraph> actor, IEnumerable<ILexicalParagraph> subject, IEnumerable<ILexicalParagraph> target, 
            IEnumerable<ILexicalParagraph> origin, IEnumerable<ILexicalParagraph> destination)
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
        public void ExecuteMessaging(IEntity Actor, IEntity Subject, IEntity Target, IEntity OriginLocation, IEntity DestinationLocation)
        {
            Dictionary<MessagingTargetType, IEntity[]> entities = new Dictionary<MessagingTargetType, IEntity[]>
            {
                { MessagingTargetType.Actor, new IEntity[] { Actor } },
                { MessagingTargetType.Subject, new IEntity[] { Subject } },
                { MessagingTargetType.Target, new IEntity[] { Target } },
                { MessagingTargetType.OriginLocation, new IEntity[] { OriginLocation } },
                { MessagingTargetType.DestinationLocation, new IEntity[] { DestinationLocation } }
            };

            if (Actor != null && ToActor.Any())
            {
                if (ToActor.Select(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                {
                    Actor.WriteTo(TranslateOutput(ToActor.Select(msg => msg.Override), entities));
                }
                else
                {
                    Actor.WriteTo(TranslateOutput(ToActor.Select(msg => msg.Describe()), entities));
                }
            }

            if (Subject != null && ToSubject.Any())
            {
                if (ToSubject.Select(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                {
                    Subject.WriteTo(TranslateOutput(ToSubject.Select(msg => msg.Override), entities));
                }
                else
                {
                    Subject.WriteTo(TranslateOutput(ToSubject.Select(msg => msg.Describe()), entities));
                }
            }

            if (Target != null && ToTarget.Any())
            {
                var language = Target.IsPlayer() ? ((IPlayer)Target).Template<IPlayerTemplate>().Account.Config.UILanguage : null;
                if (ToTarget.Select(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                {
                    Target.WriteTo(TranslateOutput(ToTarget.Select(msg => msg.Override), entities));
                }
                else
                {
                    Target.WriteTo(TranslateOutput(ToTarget.Select(msg => msg.Describe()), entities));
                }
            }

            //TODO: origin and destination are areas of effect on their surrounding areas
            if (OriginLocation != null && ToOrigin.Any())
            {
                IContains oLoc = (IContains)OriginLocation;
                IEnumerable<IEntity> validContents = oLoc.GetContents<IEntity>().Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target));

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach (IEntity dude in validContents)
                {
                    if (ToOrigin.Select(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                    {
                        dude.WriteTo(TranslateOutput(ToOrigin.Select(msg => msg.Override), entities));
                    }
                    else
                    {
                        dude.WriteTo(TranslateOutput(ToOrigin.Select(msg => msg.Describe()), entities));
                    }
                }
            }

            if (DestinationLocation != null && ToDestination.Any())
            {
                IContains oLoc = (IContains)DestinationLocation;

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach (IEntity dude in oLoc.GetContents<IEntity>().Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target)))
                {
                    if (ToDestination.Select(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                    {
                        dude.WriteTo(TranslateOutput(ToDestination.Select(msg => msg.Override), entities));
                    }
                    else
                    {
                        dude.WriteTo(TranslateOutput(ToDestination.Select(msg => msg.Describe()), entities));
                    }
                }
            }
        }

        /// <summary>
        /// Get the string version of all the contained messages
        /// </summary>
        /// <param name="target">The entity type to select the messages of</param>
        /// <returns>Everything unpacked</returns>
        public string Unpack(TargetEntity target, LexicalContext overridingContext = null)
        {
            switch(target)
            {
                case TargetEntity.Destination:
                    return string.Join(" ", ToDestination.Select(msg => msg.Describe(overridingContext)));
                case TargetEntity.Origin:
                    return string.Join(" ", ToOrigin.Select(msg => msg.Describe(overridingContext)));
                case TargetEntity.Subject:
                    return string.Join(" ", ToSubject.Select(msg => msg.Describe(overridingContext)));
                case TargetEntity.Target:
                    return string.Join(" ", ToTarget.Select(msg => msg.Describe(overridingContext)));
            }

            return string.Join(" ", ToActor.Select(msg => msg.Describe(overridingContext)));
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
