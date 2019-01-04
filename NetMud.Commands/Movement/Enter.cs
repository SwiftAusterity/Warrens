using NetMud.Cartography;
using NetMud.Commands.Attributes;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Zone;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    /// <summary>
    /// Usage of pathways
    /// </summary>
    [CommandKeyword("enter", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IPathwayDestination), CacheReferenceType.Pathway, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Enter : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Enter()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            List<string> sb = new List<string>();
            var x = Actor.CurrentLocation.CurrentCoordinates.X;
            var y = Actor.CurrentLocation.CurrentCoordinates.Y;

            Coordinate originCoords = new Coordinate(x, y);

            IPathwayDestination destination = (IPathwayDestination)Subject;
            if (destination == null)
            {
                RenderError("That is an invalid destination.");
                return;
            }

            IGlobalPosition newPosition = destination.Destination.GetLiveInstance().CurrentLocation.Clone(destination.Coordinates);

            DataStructure.Tile.ITile destinationTile = newPosition.CurrentZone.Map.CoordinateTilePlane[destination.Coordinates.X, destination.Coordinates.Y];

            if (newPosition.CurrentCoordinates.X < 0 || newPosition.CurrentCoordinates.Y < 0 
                || newPosition.CurrentCoordinates.X > 99 || newPosition.CurrentCoordinates.Y > 99 
                || destinationTile == null || destinationTile.Type == null || !destinationTile.Type.Pathable)
            {
                RenderError("That is an invalid location to travel to.");
                return;
            }

            ITemplate intruder = destinationTile.TopContents()?.Template<ITemplate>();

            //TODO: have some sort of bouncy-slide thing that puts you laterally
            if (intruder != null)
            {
                RenderError("There is something in your way at your destination.");
                return;
            }

            IZone priorZone = Actor.CurrentLocation.CurrentZone;

            Actor.TryMoveTo(newPosition);

            //Send a full map refresh
            if (priorZone != Actor.CurrentLocation.CurrentZone)
                Utilities.SendMapToZone(priorZone);

            Utilities.SendMapToZone(Actor.CurrentLocation.CurrentZone);
            Actor.Save();
            Actor.CurrentLocation.CurrentZone.Save();
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                string.Format("Valid Syntax: enter &lt;pathway&gt;"),
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
                return string.Format("Used to enter/activate pathways.");
            }
            set { }
        }
    }
}
