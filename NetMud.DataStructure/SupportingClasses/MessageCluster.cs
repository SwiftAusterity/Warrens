using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataStructure.SupportingClasses
{
    /// <summary>
    /// Used by the system to produce output for commands and events
    /// </summary>
    public class MessageCluster
    {
        /// <summary>
        /// Message to send to the acting entity
        /// </summary>
        public string ToActor { get; set; }

        /// <summary>
        /// Message to send to the subject of the command
        /// </summary>
        public string ToSubject { get; set; }

        /// <summary>
        /// Message to send to the target of the command
        /// </summary>
        public string ToTarget { get; set; }

        /// <summary>
        /// Message to send to the origin location of the command/event
        /// </summary>
        public string ToOrigin { get; set; }

        /// <summary>
        /// Message to send to the destination location of the command/event
        /// </summary>
        public string ToDestination { get; set; }

        /// <summary>
        /// Message to send to the surrounding locations of the command/event
        /// </summary>
        public Dictionary<int, Tuple<MessagingType, string>> ToSurrounding { get; set; }

        /// <summary>
        /// New up an empty cluster
        /// </summary>
        public MessageCluster()
        {
            ToSurrounding = new Dictionary<int, Tuple<MessagingType, string>>();
        }

        /// <summary>
        /// New up a full cluster
        /// </summary>
        /// <param name="actor">Message to send to the acting entity</param>
        /// <param name="subject">Message to send to the subject of the command</param>
        /// <param name="target">Message to send to the target of the command</param>
        /// <param name="origin">Message to send to the origin location of the command/event</param>
        /// <param name="destination">Message to send to the destination location of the command/event</param>
        public MessageCluster(string actor, string subject, string target, string origin, string destination)
        {
            ToActor = actor;
            ToSubject = subject;
            ToTarget = target;
            ToOrigin = origin;
            ToDestination = destination;

            ToSurrounding = new Dictionary<int, Tuple<MessagingType, string>>();
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
            var entities = new Dictionary<MessagingTargetType, IEntity[]>();

            entities.Add(MessagingTargetType.Actor, new IEntity[] { Actor });
            entities.Add(MessagingTargetType.Subject, new IEntity[] { Subject });
            entities.Add(MessagingTargetType.Target, new IEntity[] { Target });
            entities.Add(MessagingTargetType.OriginLocation, new IEntity[] { OriginLocation });
            entities.Add(MessagingTargetType.DestinationLocation, new IEntity[] { DestinationLocation });

            if (Actor != null && !string.IsNullOrWhiteSpace(ToActor))
                Actor.WriteTo(TranslateOutput(ToActor, entities));

            if (Subject != null && !string.IsNullOrWhiteSpace(ToSubject))
                Subject.WriteTo(TranslateOutput(ToSubject, entities));

            if (Target != null && !string.IsNullOrWhiteSpace(ToTarget))
                Target.WriteTo(TranslateOutput(ToTarget, entities));

            if (OriginLocation != null && !string.IsNullOrWhiteSpace(ToOrigin))
            {
                var oLoc = (IContains)OriginLocation;
                var validContents = oLoc.GetContents<IEntity>().Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target));

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach (var dude in validContents)
                    dude.WriteTo(TranslateOutput(ToOrigin, entities));
            }

            if (DestinationLocation != null && !string.IsNullOrWhiteSpace(ToDestination))
            {
                var oLoc = (IContains)DestinationLocation;

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach (var dude in oLoc.GetContents<IEntity>().Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target)))
                    dude.WriteTo(TranslateOutput(ToDestination, entities));
            }
        }

        /// <summary>
        /// Translates output text with color codes and entity variables
        /// </summary>
        /// <param name="output">the output text to translate</param>
        /// <param name="entities">relevant entities for the variables transform</param>
        /// <returns>translated output</returns>
        private IEnumerable<string> TranslateOutput(string output, Dictionary<MessagingTargetType, IEntity[]> entities)
        {
            var outputStrings = new List<string>();

            var entityTranslated = MessagingUtility.TranslateEntityVariables(output, entities);
            var colorTranslated = MessagingUtility.TranslateColorVariables(entityTranslated);

            outputStrings.Add(colorTranslated);

            return outputStrings;
        }
    }

}
