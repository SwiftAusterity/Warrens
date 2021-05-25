using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("get", false)]
    [CommandKeyword("take", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IEntity), new CacheReferenceType[] { CacheReferenceType.Entity }, false)]
    [CommandParameter(CommandUsage.Target, typeof(IContains), new CacheReferenceType[] { CacheReferenceType.Inventory }, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Get : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Get()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            List<string> sb = new();
            IEntity thing = (IEntity)Subject;
            IContains actor = (IContains)Actor;
            IContains place;

            string toRoomMessage = "$A$ gets $S$.";

            if (Target != null)
            {
                place = (IContains)Target;
                toRoomMessage = "$A$ gets $S$ from $T$."; 
                sb.Add("You get $S$ from $T$.");
            }
            else
            {
                place = (IContains)OriginLocation;
                sb.Add("You get $S$.");
            }

            place.MoveFrom(thing);
            actor.MoveInto(thing);

            ILexicalParagraph toActor = new LexicalParagraph(sb.ToString());

            ILexicalParagraph toOrigin = new LexicalParagraph(toRoomMessage);

            Message messagingObject = new(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, thing, (IEntity)Target, OriginLocation.CurrentRoom, null);

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
                "Valid Syntax: get &lt;object&gt;",
                "take &lt;object&gt;".PadWithString(14, "&nbsp;", true),
                "get &lt;object&gt; &lt;container&gt;".PadWithString(14, "&nbsp;", true),
                "take &lt;object&gt; &lt;container&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Takes an object from a container or if unspecified attempts to take it from the room you are in.");
            }
            set { }
        }
    }
}
