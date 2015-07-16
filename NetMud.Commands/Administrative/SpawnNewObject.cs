using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

using NetMud.Utility;

namespace NutMud.Commands.System
{
    //Really help can be invoked on anything that is helpful, even itself
    [CommandKeyword("SpawnNewObject")]
    [CommandPermission(StaffRank.Admin)]
    [CommandParameter(CommandUsage.Subject, typeof(IObject), new CacheReferenceType[] { CacheReferenceType.Data }, true )]
    [CommandParameter(CommandUsage.Target, typeof(ILocation), new CacheReferenceType[] { CacheReferenceType.Entity }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class SpawnNewObject : ICommand, IHelpful
    {
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
            var newObject = (IObject)Subject;
            var sb = new List<string>();
            var spawnTo = OriginLocation;

            //No target = spawn to room you're in
            if (Target != null)
                spawnTo = (ILocation)Target;

            newObject.SpawnNewInWorld(spawnTo);

            //TODO: keywords is janky, location should have its own identifier name somehow for output purposes
            sb.Add(String.Format("{0} spawned to {1}", newObject.Name, spawnTo.Keywords[0]));

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
