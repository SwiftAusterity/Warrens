﻿using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.Rendering
{
    [CommandKeyword("inventory", false, new string[] { "inv", "i" })]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Inventory : CommandPartial
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
            List<string> sb = new List<string>();
            IMobile chr = (IMobile)Actor;
            List<IMessage> toActor = new List<IMessage>
            {
                new Message()
                {
                    Body = new string[] { "You look through your belongings." }
                }
            };

            foreach (IInanimate thing in chr.Inventory.EntitiesContained())
            {
                string itemString = string.Format("{0} {1}", thing.Template<IInanimateTemplate>().AsciiCharacter, thing.GetDescribableName(Actor));
                toActor.Add(new Message()
                {
                    Body = new string[] { itemString }
                });
            }

            MessageCluster messagingObject = new MessageCluster(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentZone, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: inventory",
                "inv".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Inventory lists out all inanimates currently on your person. It is an undetectable action unless a viewer has high perception.");
            }
            set { }
        }

    }
}
