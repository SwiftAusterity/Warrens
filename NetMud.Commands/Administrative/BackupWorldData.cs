using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System;
using System.Collections.Generic;

using NetMud.Utility;
using NetMud.DataStructure.SupportingClasses;
using NetMud.LiveData;

namespace NutMud.Commands.System
{
    //Really help can be invoked on anything that is helpful, even itself
    [CommandKeyword("BackupWorldData", false)]
    [CommandPermission(StaffRank.Admin)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class BackupWorldData : ICommand, IHelpful
    {
        public IActor Actor { get; set; }
        public object Subject { get; set; }
        public object Target { get; set; }
        public object Supporting { get; set; }
        public ILocation OriginLocation { get; set; }
        public IEnumerable<ILocation> Surroundings { get; set; }

        public BackupWorldData()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        public void Execute()
        {
            var sb = new List<string>();

            var hotBack = new HotBackup("/HotBackup/");

            //Our live data restore failed, reload the entire world from backing data
            hotBack.WriteLiveBackup();

            //TODO: keywords is janky, location should have its own identifier name somehow for output purposes
            sb.Add("You have backed the world up");

            var messagingObject = new MessageCluster(RenderUtility.EncapsulateOutput(sb), string.Empty, string.Empty, string.Empty, string.Empty);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);
        }

        public IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Valid Syntax: backupworldata"));

            return sb;
        }

        /// <summary>
        /// Renders the help text for the help command itself
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(string.Format("BackupWorldData backs up all the live data."));

            return sb;
        }
    }
}
