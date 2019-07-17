using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetMud.Commands.System
{
    [CommandQueueSkip]
    [CommandKeyword("commands", false, false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Commands : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Commands()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        internal override bool ExecutionBody()
        {
            //NPCs dont need to use this
            if (!Actor.GetType().GetInterfaces().Contains(typeof(IPlayer)))
            {
                return false;
            }

            List<string> returnStrings = new List<string>();
            StringBuilder sb = new StringBuilder();

            Assembly commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));

            IEnumerable<global::System.Type> loadedCommands = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICommand)));

            loadedCommands = loadedCommands.Where(comm => comm.GetCustomAttributes<CommandPermissionAttribute>().Any(att => att.MinimumRank <= Actor.Template<IPlayerTemplate>().GamePermissionsRank));

            returnStrings.Add("Commands:");

            List<string> commandNames = new List<string>();
            foreach (global::System.Type command in loadedCommands)
            {
                foreach(CommandKeywordAttribute commandName in command.GetCustomAttributes<CommandKeywordAttribute>().Where(key => key.DisplayInHelpAndCommands))
                {
                    if (!commandNames.Contains(commandName.Keyword))
                    {
                        commandNames.Add(commandName.Keyword);
                    }
                }

                if (!commandNames.Contains(command.Name) && command.GetCustomAttribute<CommandSuppressName>() == null)
                {
                    commandNames.Add(command.Name);
                }
            }

            sb.AppendLine(string.Join(", ", commandNames.Select(cmd => cmd.ToLower()).Distinct()));

            if(sb.Length > 0)
            {
                sb.Length -= 2;
            }

            returnStrings.Add(sb.ToString());

            Message messagingObject = new Message(string.Join(" ", returnStrings));

            messagingObject.ExecuteMessaging(Actor, null, null, null, null, 0);

            return true;
        }

        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: commands"
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
                return string.Format("Commands lists possible commands for you to use in-game.");
            }
            set { }
        }
    }
}
