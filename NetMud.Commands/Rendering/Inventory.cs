using NetMud.Commands.Attributes;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

namespace NetMud.Commands.Rendering
{
    [CommandKeyword("inventory", false)]
    [CommandKeyword("inv", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Inventory : CommandPartial, IHelpful
    {        
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Inventory()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>();
            var chr = (IMobile)Actor;

            sb.Add("You look through your belongings.");

            foreach (var thing in chr.Inventory.EntitiesContained())
                sb.AddRange(thing.RenderToLook(chr));

            var messagingObject = new MessageCluster(sb, new string[] { }, new string[] { }, new string[] { "$A$ sifts through $G$ belongings." }, new string[] { });

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add("Valid Syntax: inventory");
            sb.Add("inv".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// Renders the help text
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Inventory lists out all inanimates currently on your person. It is an undetectable action unless a viewer has high perception."));

            return sb;
        }
    }
}
