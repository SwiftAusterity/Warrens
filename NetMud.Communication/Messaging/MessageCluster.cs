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
    public class MessageCluster : IMessageCluster
    {
        /// <summary>
        /// Message to send to the acting entity
        /// </summary>
        public IEnumerable<IMessage> ToActor { get; set; }

        /// <summary>
        /// Message to send to the subject of the command
        /// </summary>
        public IEnumerable<IMessage> ToSubject { get; set; }

        /// <summary>
        /// Message to send to the target of the command
        /// </summary>
        public IEnumerable<IMessage> ToTarget { get; set; }

        /// <summary>
        /// Message to send to the origin location of the command/event
        /// </summary>
        public IEnumerable<IMessage> ToOrigin { get; set; }

        /// <summary>
        /// Message to send to the destination location of the command/event
        /// </summary>
        public IEnumerable<IMessage> ToDestination { get; set; }

        /// <summary>
        /// New up an empty cluster
        /// </summary>
        public MessageCluster()
        {
            ToActor = Enumerable.Empty<IMessage>();
            ToSubject = Enumerable.Empty<IMessage>();
            ToTarget = Enumerable.Empty<IMessage>();
            ToOrigin = Enumerable.Empty<IMessage>();
            ToDestination = Enumerable.Empty<IMessage>();
        }

        /// <summary>
        /// New up a clister with just toactor for system messages
        /// </summary>
        public MessageCluster(IMessage toActor)
        {
            ToActor = new List<IMessage> { toActor };
            ToSubject = Enumerable.Empty<IMessage>();
            ToTarget = Enumerable.Empty<IMessage>();
            ToOrigin = Enumerable.Empty<IMessage>();
            ToDestination = Enumerable.Empty<IMessage>();
        }

        /// <summary>
        /// New up a clister with just toactor for system messages
        /// </summary>
        public MessageCluster(List<IMessage> toActor)
        {
            ToActor = toActor;
            ToSubject = Enumerable.Empty<IMessage>();
            ToTarget = Enumerable.Empty<IMessage>();
            ToOrigin = Enumerable.Empty<IMessage>();
            ToDestination = Enumerable.Empty<IMessage>();
        }

        /// <summary>
        /// New up a full cluster
        /// </summary>
        /// <param name="actor">Message to send to the acting entity</param>
        /// <param name="subject">Message to send to the subject of the command</param>
        /// <param name="target">Message to send to the target of the command</param>
        /// <param name="origin">Message to send to the origin location of the command/event</param>
        /// <param name="destination">Message to send to the destination location of the command/event</param>
        public MessageCluster(IEnumerable<IMessage> actor, IEnumerable<IMessage> subject, IEnumerable<IMessage> target, IEnumerable<IMessage> origin, IEnumerable<IMessage> destination)
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
                if (ToActor.SelectMany(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                {
                    Actor.WriteTo(TranslateOutput(ToActor.SelectMany(msg => msg.Override), entities));
                }
                else
                {
                    var language = Actor.IsPlayer() ? ((IPlayer)Actor).Template<IPlayerTemplate>().Account.Config.UILanguage : null;
                    Actor.WriteTo(TranslateOutput(ToActor.Select(msg => msg.Occurrence?.Event?.Describe(language, NarrativeNormalization.Normal, 1, LexicalTense.Present, NarrativePerspective.FirstPerson, false)), entities));
                }
            }

            if (Subject != null && ToSubject.Any())
            {
                if (ToSubject.SelectMany(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                {
                    Subject.WriteTo(TranslateOutput(ToSubject.SelectMany(msg => msg.Override), entities));
                }
                else
                {
                    var language = Subject.IsPlayer() ? ((IPlayer)Subject).Template<IPlayerTemplate>().Account.Config.UILanguage : null;
                    Subject.WriteTo(TranslateOutput(ToSubject.Select(msg => msg.Occurrence?.Event?.Describe(language, NarrativeNormalization.Normal, 1, LexicalTense.Present, NarrativePerspective.SecondPerson, false)), entities));
                }
            }

            if (Target != null && ToTarget.Any())
            {
                var language = Target.IsPlayer() ? ((IPlayer)Target).Template<IPlayerTemplate>().Account.Config.UILanguage : null;
                if (ToTarget.SelectMany(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                {
                    Target.WriteTo(TranslateOutput(ToTarget.SelectMany(msg => msg.Override), entities));
                }
                else
                {
                    Target.WriteTo(TranslateOutput(ToTarget.Select(msg => msg.Occurrence?.Event?.Describe(language, NarrativeNormalization.Normal, 1, LexicalTense.Present, NarrativePerspective.SecondPerson, false)), entities));
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
                    if (ToOrigin.SelectMany(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                    {
                        dude.WriteTo(TranslateOutput(ToOrigin.SelectMany(msg => msg.Override), entities));
                    }
                    else
                    {
                        var language = dude.IsPlayer() ? ((IPlayer)dude).Template<IPlayerTemplate>().Account.Config.UILanguage : null;
                        dude.WriteTo(TranslateOutput(ToOrigin.Select(msg => msg.Occurrence?.Event?.Describe(language, NarrativeNormalization.Normal, 1, LexicalTense.Present, NarrativePerspective.ThirdPerson, false)), entities));
                    }
                }
            }

            if (DestinationLocation != null && ToDestination.Any())
            {
                IContains oLoc = (IContains)DestinationLocation;

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach (IEntity dude in oLoc.GetContents<IEntity>().Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target)))
                {
                    if (ToDestination.SelectMany(msg => msg.Override).Any(str => !string.IsNullOrEmpty(str)))
                    {
                        dude.WriteTo(TranslateOutput(ToDestination.SelectMany(msg => msg.Override), entities));
                    }
                    else
                    {
                        var language = dude.IsPlayer() ? ((IPlayer)dude).Template<IPlayerTemplate>().Account.Config.UILanguage : null;
                        dude.WriteTo(TranslateOutput(ToDestination.Select(msg => msg.Occurrence?.Event?.Describe(language, NarrativeNormalization.Normal, 1, LexicalTense.Present, NarrativePerspective.ThirdPerson, false)), entities));
                    }
                }
            }
        }

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
