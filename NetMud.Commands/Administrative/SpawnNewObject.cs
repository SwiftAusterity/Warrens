using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System;
using System.Collections.Generic;

using NetMud.Utility;
using NetMud.DataStructure.Base.EntityBackingData;

namespace NutMud.Commands.System
{
    //Really help can be invoked on anything that is helpful, even itself
    [CommandKeyword("SpawnNewObject", false)]
    [CommandPermission(StaffRank.Admin)]
    [CommandParameter(CommandUsage.Subject, typeof(NetMud.Data.EntityBackingData.ObjectData), new CacheReferenceType[] { CacheReferenceType.Data }, "[0-9]+", false)] //for IDs
    [CommandParameter(CommandUsage.Subject, typeof(NetMud.Data.EntityBackingData.ObjectData), new CacheReferenceType[] { CacheReferenceType.Data }, "[a-zA-z]+", false)] //for names
    [CommandParameter(CommandUsage.Target, typeof(IContains), new CacheReferenceType[] { CacheReferenceType.Entity }, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class SpawnNewObject : ICommand, IHelpful
    {
        public IActor Actor { get; set; }
        public object Subject { get; set; }
        public object Target { get; set; }
        public object Supporting { get; set; }
        public ILocation OriginLocation { get; set; }
        public IEnumerable<ILocation> Surroundings { get; set; }

        public SpawnNewObject()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        public IEnumerable<string> Execute()
        {
            var newObject = (IObjectData)Subject;
            var sb = new List<string>();
            IContains spawnTo;

            //No target = spawn to room you're in
            if (Target != null)
                spawnTo = (IContains)Target;
            else
                spawnTo = OriginLocation;

            var entityObject = new NetMud.Data.Game.Object(newObject, spawnTo);

            //TODO: keywords is janky, location should have its own identifier name somehow for output purposes
            sb.Add(String.Format("{0} spawned to {1}", entityObject.DataTemplate.Name, spawnTo.Keywords[0]));

            return sb;
        }

        public IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add(String.Format("Valid Syntax: spawnNewObject &lt;object name&gt;"));
            sb.Add("spawnNewObject  &lt;object name&gt;  &lt;location name to spawn to&gt;".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// Renders the help text for the help command itself
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(String.Format("SpawnNewObject spawns a new object from its data template into the room or into a specified inventory."));

            return sb;
        }
    }
}
