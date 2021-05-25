using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.Utility;
using System;
using System.Collections.Generic;

namespace NetMud.Commands.System
{
    /// <summary>
    /// Spawns a new inanimate into the world.  Missing target parameter = container you're standing in
    /// </summary>
    [CommandKeyword("SpawnNewObject", false, "ospawn", false, true)]
    [CommandPermission(StaffRank.Admin)]
    [CommandParameter(CommandUsage.Subject, typeof(IInanimateTemplate), CacheReferenceType.Data, "[0-9]+", false)] //for IDs
    [CommandParameter(CommandUsage.Subject, typeof(IInanimateTemplate), CacheReferenceType.Data, "[a-zA-z]+", false)] //for names
    [CommandParameter(CommandUsage.Target, typeof(IContains), CacheReferenceType.Entity, true)]
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
        internal override bool ExecutionBody()
        {
            IInanimateTemplate newObject = (IInanimateTemplate)Subject;
            IMobile initator = (IMobile)Actor;
            List<string> sb = new();
            IInanimate entityObject;

            //No target = spawn to inventory
            if (Target != null)
            {
                IGlobalPosition spawnTo = (IGlobalPosition)Target;
                entityObject = Activator.CreateInstance(newObject.EntityClass, new object[] { newObject }) as IInanimate;
                sb.Add(string.Format("{0} spawned to {1}.", entityObject.TemplateName, spawnTo.CurrentZone.Keywords[0]));
            }
            else
            {
                entityObject = Activator.CreateInstance(newObject.EntityClass, new object[] { newObject, initator.GetContainerAsLocation() }) as IInanimate;
                sb.Add(string.Format("{0} spawned to your inventory.", entityObject.TemplateName));
            }

            //TODO: keywords is janky, location should have its own identifier name somehow for output purposes - DISPLAY short/long NAME

            ILexicalParagraph toActor = new LexicalParagraph(sb.ToString());

            ILexicalParagraph toOrigin = new LexicalParagraph("$S$ appears in the $T$.");

            ILexicalParagraph toSubject = new LexicalParagraph("You are ALIVE");

            ILexicalParagraph toTarget = new LexicalParagraph("You have been given $S$");

            Message messagingObject = new(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin },
                ToSubject = new List<ILexicalParagraph> { toSubject },
                ToTarget = new List<ILexicalParagraph> { toTarget }
            };

            messagingObject.ExecuteMessaging(Actor, entityObject, OriginLocation.CurrentZone, OriginLocation.CurrentZone, null);

            return true;
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new()
            {
                string.Format("Valid Syntax: spawnNewObject &lt;object name&gt;"),
                "spawnNewObject  &lt;object name&gt;  &lt;location name to spawn to&gt;".PadWithString(14, "&nbsp;", true)
            };

            return sb;
        }


        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override MarkdownString HelpText
        {
            get
            {
                return string.Format("SpawnNewObject spawns a new object from its data template into the room or into a specified inventory.");
            }
            set { }
        }
    }
}
