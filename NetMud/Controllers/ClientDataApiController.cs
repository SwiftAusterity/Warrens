using NetMud.Communication.Messaging;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Physics;
using NetMud.Utility;
using System;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace NetMud.Controllers
{
    public class ClientDataApiController : ApiController
    {
        public string GetEntityModelView(long modelId)
        {
            var model = BackingDataCache.Get<IDimensionalModelData>(modelId);

            if (model == null)
                return String.Empty;

            return Render.FlattenModel(model);
        }

        public string[] GetDimensionalData(long id)
        {
            var model = BackingDataCache.Get<IDimensionalModelData>(id);

            if (model == null)
                return new string[0];

            return new string[] { model.ModelPlane.TagName };
        }

        [HttpGet]
        public string RenderRoomForEditWithRadius(long id, int radius)
        {
            var centerRoom = BackingDataCache.Get<IRoomData>(id);

            if (centerRoom == null || radius < 0)
                return "Invalid inputs.";

            var sb = new StringBuilder();

            //triple radius (one for pathway, one for return pathway, one for rooms) in each direction plus the center room
            var diameter = radius * 6 + 1;

            //Useful to have, we dont want math all over the code just to find the center every time
            var center = radius * 3 + 1;

            var asciiMap = new string[diameter, diameter];

            //The origin room
            asciiMap = RenderFullRoomToAscii(asciiMap, centerRoom, diameter, center, center, true);

            int x, y;
            for (y = diameter - 1; y >= 0; y--)
            {
                var rowString = String.Empty;
                for (x = 0; x < diameter; x++)
                    rowString += asciiMap[x, y];

                sb.AppendLine(rowString);
            }

            //We add an entire extra 2 lines to do non-compass direction rooms
            var extraString = String.Empty;
            foreach (var path in centerRoom.GetPathways().Where(path => path.DirectionType == MovementDirectionType.None
                                                        && !string.IsNullOrWhiteSpace(path.ToLocationID)
                                                        && path.ToLocationType.Equals("Room")))
                extraString += "&nbsp;" + RenderPathwayToAscii(path, centerRoom.ID, MovementDirectionType.None);

            //One last for allowance of adding non-directional ones
            extraString += "&nbsp;" + RenderPathwayToAscii(null, centerRoom.ID, MovementDirectionType.None);

            if (extraString.Length > 0)
            {
                sb.AppendLine("&nbsp;");
                sb.AppendLine(extraString);
            }

            return sb.ToString();
        }

        //It's just easier to pass the ints we already calculated along instead of doing the math every single time, this cascades each direction fully because it calls itself for existant rooms
        private string[,] RenderFullRoomToAscii(string[,] asciiMap, IRoomData origin, int diameter, int centerX, int centerY, bool center = false)
        {
            //Render the room itself
            asciiMap[centerX - 1, centerY - 1] = RenderRoomToAscii(origin, center);
            asciiMap = RenderDirection(asciiMap, MovementDirectionType.North, origin, diameter, centerX, centerY);
            asciiMap = RenderDirection(asciiMap, MovementDirectionType.NorthEast, origin, diameter, centerX, centerY);
            asciiMap = RenderDirection(asciiMap, MovementDirectionType.NorthWest, origin, diameter, centerX, centerY);
            asciiMap = RenderDirection(asciiMap, MovementDirectionType.East, origin, diameter, centerX, centerY);
            asciiMap = RenderDirection(asciiMap, MovementDirectionType.West, origin, diameter, centerX, centerY);
            asciiMap = RenderDirection(asciiMap, MovementDirectionType.South, origin, diameter, centerX, centerY);
            asciiMap = RenderDirection(asciiMap, MovementDirectionType.SouthEast, origin, diameter, centerX, centerY);
            asciiMap = RenderDirection(asciiMap, MovementDirectionType.SouthWest, origin, diameter, centerX, centerY);

            return asciiMap;
        }

        //We have to render our pathway out, an empty space for the potential pathway back and the destination room
        private string[,] RenderDirection(string[,] asciiMap, MovementDirectionType transversalDirection, IRoomData origin, int diameter, int centerX, int centerY)
        {
            var pathways = origin.GetPathways();
            var directionalSteps = GetDirectionStep(transversalDirection);

            var xStepped = centerX + directionalSteps.Item1;
            var yStepped = centerY + directionalSteps.Item2;

            //If we're not over diameter budget and there is nothing there already (we might have already rendered the path and room) then render it
            //When the next room tries to render backwards it'll run into the existant path it came from and stop the chain here
            if (xStepped <= diameter && yStepped <= diameter && xStepped > 0 && yStepped > 0 
                && String.IsNullOrWhiteSpace(asciiMap[xStepped - 1, yStepped - 1]))
            {
                var thisPath = pathways.FirstOrDefault(path => path.DirectionType == transversalDirection);
                asciiMap[xStepped - 1, yStepped - 1] = RenderPathwayToAscii(thisPath, origin.ID, transversalDirection);

                //We triple step here because the first step was the pathway but the second step is blank for the return pathway. the third step is the actual room
                var tripleXStep = xStepped + directionalSteps.Item1 * 2;
                var tripleYStep = yStepped + directionalSteps.Item2 * 2;

                if (thisPath != null && thisPath.ToLocationType.Equals("Room", StringComparison.InvariantCultureIgnoreCase)
                    && tripleXStep <= diameter && tripleYStep <= diameter && tripleXStep > 0 && tripleYStep > 0
                    && String.IsNullOrWhiteSpace(asciiMap[tripleXStep - 1, tripleYStep - 1]))
                {
                    var passdownOrigin = BackingDataCache.Get<IRoomData>(long.Parse(thisPath.ToLocationID));

                    if (passdownOrigin != null)
                        asciiMap = RenderFullRoomToAscii(asciiMap, passdownOrigin, diameter, tripleXStep, tripleYStep);
                }
            }

            return asciiMap;
        }

        //X, Y
        private Tuple<int, int> GetDirectionStep(MovementDirectionType transversalDirection)
        {
            switch (transversalDirection)
            {
                default: //We already defaulted to 0,0
                    break;
                case MovementDirectionType.East:
                    return new Tuple<int, int>(1, 0);
                case MovementDirectionType.North:
                    return new Tuple<int, int>(0, 1);
                case MovementDirectionType.NorthEast:
                    return new Tuple<int, int>(1, 1);
                case MovementDirectionType.NorthWest:
                    return new Tuple<int, int>(-1, 1);
                case MovementDirectionType.South:
                    return new Tuple<int, int>(0, -1);
                case MovementDirectionType.SouthEast:
                    return new Tuple<int, int>(1, -1);
                case MovementDirectionType.SouthWest:
                    return new Tuple<int, int>(-1, -1);
                case MovementDirectionType.West:
                    return new Tuple<int, int>(-1, 0);
            }
            return new Tuple<int, int>(0, 0);
        }

        private string RenderPathwayToAscii(IPathwayData path, long originId, MovementDirectionType directionType)
        {
            var returnValue = String.Empty;
            var asciiCharacter = RenderUtility.TranslateDirectionToAsciiCharacter(directionType);

            if (path != null)
            {
                var destination = BackingDataCache.Get<IRoomData>(long.Parse(path.ToLocationID));

                returnValue = String.Format("<a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editData pathway' title='Edit - {1}' data-id='{0}'>{2}</a>",
                    path.ID, destination.Name, asciiCharacter);
            }
            else
                returnValue = String.Format("<a href='/GameAdmin/Pathway/Add/{0}' class='addData pathway' target='_blank' data-direction='{1}' title='Add - {2} path and room'>+</a>",
                    originId, RenderUtility.TranslateDirectionToDegrees(directionType), directionType.ToString());

            return returnValue;
        }

        private string RenderRoomToAscii(IRoomData destination, bool centered)
        {
            var character = centered ? "0" : "*";

            return String.Format("<a href='/GameAdmin/Room/Edit/{0}' class='editData room' target='_blank' title='Edit - {2}'>{1}</a>", destination.ID, character, destination.Name);
        }
    }
}
