using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataStructure.SupportingClasses
{
    public class MessageCluster
    {
        public string ToActor { get; set; }
        public string ToSubject { get; set; }
        public string ToTarget { get; set; }
        public string ToOrigin { get; set; }
        public string ToDestination { get; set; }
        public Dictionary<int, Tuple<MessagingType, string>> ToSurrounding { get; set; }

        public MessageCluster()
        {
            ToSurrounding = new Dictionary<int, Tuple<MessagingType, string>>();
        }

        public MessageCluster(string actor, string subject, string target, string origin, string destination)
        {
            ToActor = actor;
            ToSubject = subject;
            ToTarget = target;
            ToOrigin = origin;
            ToDestination = destination;

            ToSurrounding = new Dictionary<int, Tuple<MessagingType, string>>();
        }

        public void ExecuteMessaging(IEntity Actor, IEntity Subject, IEntity Target, IEntity OriginLocation, IEntity DestinationLocation)
        {
            var entities = new Dictionary<MessagingTargetType, IEntity[]>();

            entities.Add(MessagingTargetType.Actor, new IEntity[] { Actor });
            entities.Add(MessagingTargetType.Subject, new IEntity[] { Subject });
            entities.Add(MessagingTargetType.Target, new IEntity[] { Target });
            entities.Add(MessagingTargetType.OriginLocation, new IEntity[] { OriginLocation });
            entities.Add(MessagingTargetType.DestinationLocation, new IEntity[] { DestinationLocation });

            if (Actor != null && !String.IsNullOrWhiteSpace(ToActor))
                Actor.WriteTo(TranslateOutput(ToActor, entities));

            if (Subject != null && !String.IsNullOrWhiteSpace(ToSubject))
                Subject.WriteTo(TranslateOutput(ToSubject, entities));

            if (Target != null && !String.IsNullOrWhiteSpace(ToTarget))
                Target.WriteTo(TranslateOutput(ToTarget, entities));

            if (OriginLocation != null && !String.IsNullOrWhiteSpace(ToOrigin))
            {
                var oLoc = (IContains)OriginLocation;

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach(var dude in oLoc.GetContents<IEntity>().Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target)))
                    dude.WriteTo(TranslateOutput(ToOrigin, entities));
            }

            if (DestinationLocation != null && !String.IsNullOrWhiteSpace(ToDestination))
            {
                var oLoc = (IContains)DestinationLocation;

                //Message dudes in the location, including non-person entities since they might have triggers
                foreach (var dude in oLoc.GetContents<IEntity>().Where(dud => !dud.Equals(Actor) && !dud.Equals(Subject) && !dud.Equals(Target)))
                    dude.WriteTo(TranslateOutput(ToDestination, entities));
            }
        }

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
