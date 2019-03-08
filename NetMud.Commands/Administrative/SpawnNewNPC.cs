using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;

namespace NetMud.Commands.System
{
    /// <summary>
    /// Spawns a new NPC into the world. Missing target parameter = container you're standing in
    /// </summary>
    [CommandKeyword("SpawnNewNPC", false, "mspawn", false, true)]
    [CommandPermission(StaffRank.Admin)]
    [CommandParameter(CommandUsage.Subject, typeof(INonPlayerCharacterTemplate), CacheReferenceType.Data, "[0-9]+", false)] //for IDs
    [CommandParameter(CommandUsage.Subject, typeof(INonPlayerCharacterTemplate), CacheReferenceType.Data, "[a-zA-z]+", false)] //for names
    [CommandParameter(CommandUsage.Target, typeof(IContains), CacheReferenceType.Entity, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class SpawnNewNPC : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public SpawnNewNPC()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            INonPlayerCharacterTemplate newObject = (INonPlayerCharacterTemplate)Subject;
            List<string> sb = new List<string>();
            IGlobalPosition spawnTo;

            //No target = spawn to room you're in
            if (Target != null)
            {
                spawnTo = (IGlobalPosition)Target;
            }
            else
            {
                spawnTo = OriginLocation;
            }

            INonPlayerCharacter entityObject = Activator.CreateInstance(newObject.EntityClass, new object[] { newObject, spawnTo }) as INonPlayerCharacter;

            //TODO: keywords is janky, location should have its own identifier name somehow for output purposes - DISPLAY short/long NAME
            sb.Add(string.Format("{0} spawned to {1}", entityObject.TemplateName, spawnTo.CurrentZone.Keywords[0]));

            ILexicalParagraph toActor = new LexicalParagraph(sb.ToString());

            ILexicalParagraph toOrigin = new LexicalParagraph("$S$ appears suddenly.");

            ILexicalParagraph toSubject = new LexicalParagraph("You are ALIVE");

            IMessage messagingObject = new Message(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin },
                ToSubject = new List<ILexicalParagraph> { toSubject }
            };

            messagingObject.ExecuteMessaging(Actor, entityObject, spawnTo.CurrentZone, OriginLocation.CurrentZone, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                string.Format("Valid Syntax: spawnNewNPC &lt;NPC name&gt;"),
                "spawnNewNPC  &lt;NPC name&gt;  &lt;location name to spawn to&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("spawnNewNPC spawns a new NPC from its data template into the room or into a specified location.");
            }
            set { }
        }
    }
}
