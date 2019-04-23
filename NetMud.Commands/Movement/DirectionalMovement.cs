using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    /// <summary>
    /// Handles mobile movement commands. All cardinal directions plus "enter <door>" type pathways
    /// </summary>
    [CommandSuppressName]
    [CommandKeyword("forward", false, "ahead", true, false)]
    [CommandKeyword("backward", false, "back", true, false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class DirectionalMovement : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public DirectionalMovement()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            string subject = CommandWord.ToString();
            ulong newPosition = 0;

            switch (subject)
            {
                case "forward":
                    if (Actor.CurrentLocation.CurrentSection >= ulong.MaxValue)
                    {
                        RenderError("You're already as close to the door as you can be.");
                        return false;
                    }

                    newPosition = Actor.CurrentLocation.CurrentSection + 1;
                    break;
                case "backward":
                    if (Actor.CurrentLocation.CurrentSection <= 0)
                    {
                        RenderError("You're already as far away from the door as you can be.");
                        return false;
                    }

                    newPosition = Actor.CurrentLocation.CurrentSection - 1;
                    break;
            }

            var newPos = (IGlobalPosition)Actor.CurrentLocation.Clone();
            newPos.CurrentSection = newPosition;

            Actor.TryMoveTo(newPos);

            var msg = new Message(string.Format("You move half the distance {0}.", subject));

            msg.ExecuteMessaging(Actor, null, null, newPos, null, 3);

            return true;
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> dirList = new List<string>() {
                "forward", "backward"
            };

            List<string> sb = new List<string>
            {
                string.Format("Valid Syntax:"),
                dirList.CommaList(RenderUtility.SplitListType.AllComma)
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
                return @"
### Movement

Movement is accomplished by going forward or backward.";
            }
            set { }
        }
    }
}
