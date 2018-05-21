using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

using NetMud.Utility;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.Data.EntityBackingData;
using NetMud.Data.Game;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.Data.System;

namespace NutMud.Commands.System
{
    /// <summary>
    /// Spawns a new inanimate into the world.  Missing target parameter = container you're standing in
    /// </summary>
    [CommandKeyword("SpawnNewObject", false)]
    [CommandPermission(StaffRank.Admin)]
    [CommandParameter(CommandUsage.Subject, typeof(InanimateData), new CacheReferenceType[] { CacheReferenceType.Data }, "[0-9]+", false)] //for IDs
    [CommandParameter(CommandUsage.Subject, typeof(InanimateData), new CacheReferenceType[] { CacheReferenceType.Data }, "[a-zA-z]+", false)] //for names
    [CommandParameter(CommandUsage.Target, typeof(IContains), new CacheReferenceType[] { CacheReferenceType.Entity }, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class SpawnNewObject : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public SpawnNewObject()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var newObject = (IInanimateData)Subject;
            var sb = new List<string>();
            IGlobalPosition spawnTo;

            //No target = spawn to room you're in
            if (Target != null)
                spawnTo = (IGlobalPosition)Target;
            else
                spawnTo = OriginLocation;

            var entityObject = new Inanimate(newObject, spawnTo);

            //TODO: keywords is janky, location should have its own identifier name somehow for output purposes - DISPLAY short/long NAME
            sb.Add(string.Format("{0} spawned to {1}", entityObject.DataTemplateName, spawnTo.CurrentLocation.Keywords[0]));

            var toActor = new Message(MessagingType.Visible, new Occurrence() { Strength = 1 })
            {
                Override = sb
            };

            var toOrigin = new Message(MessagingType.Visible, new Occurrence() { Strength = 30 })
            {
                Override = new string[] { "$S$ appears in the $T$." }
            };

            var toSubject = new Message(MessagingType.Visible, new Occurrence() { Strength = 30 })
            {
                Override = new string[] { "You are ALIVE" }
            };

            var toTarget = new Message(MessagingType.Visible, new Occurrence() { Strength = 30 })
            {
                Override = new string[] { "You have been given $S$" }
            };

            var messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin },
                ToSubject = new List<IMessage> { toSubject },
                ToTarget = new List<IMessage> { toTarget }
            };

            messagingObject.ExecuteMessaging(Actor, entityObject, spawnTo.CurrentLocation, OriginLocation.CurrentLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                string.Format("Valid Syntax: spawnNewObject &lt;object name&gt;"),
                "spawnNewObject  &lt;object name&gt;  &lt;location name to spawn to&gt;".PadWithString(14, "&nbsp;", true)
            };

            return sb;
        }


        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("SpawnNewObject spawns a new object from its data template into the room or into a specified inventory.");
            }
            set { }
        }
    }
}
