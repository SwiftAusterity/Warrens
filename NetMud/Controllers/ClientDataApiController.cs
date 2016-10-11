using NetMud.Communication.Messaging;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace NetMud.Controllers
{
    public class ClientDataApiController : ApiController
    {
        public string GetEntityModelView(long modelId, short yaw, short pitch, short roll)
        {
            var model = BackingDataCache.Get<IDimensionalModelData>(modelId);

            if (model == null)
                return String.Empty;

            return model.ViewFlattenedModel(pitch, yaw, roll);
        }

        public string[] GetDimensionalData(long id)
        {
            var model = BackingDataCache.Get<IDimensionalModelData>(id);

            if (model == null)
                return new string[0];

            return model.ModelPlanes.Select(plane => plane.TagName).Distinct().ToArray();
        }

        [HttpGet]
        public string RenderRoomForEdit(long id, int radius)
        {
            var centerRoom = BackingDataCache.Get<IRoomData>(id);

            if (centerRoom == null || radius < 0)
                return "Invalid inputs.";

            var sb = new StringBuilder();

            var pathways = centerRoom.GetPathways();

            var nw = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.NorthWest);
            var n = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.North);
            var ne = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.NorthEast);

            var northString = String.Empty;
            northString += RenderPathwayToAscii(nw, centerRoom.ID, MovementDirectionType.NorthWest);
            northString += RenderPathwayToAscii(n, centerRoom.ID, MovementDirectionType.North);
            northString += RenderPathwayToAscii(ne, centerRoom.ID, MovementDirectionType.NorthEast);

            sb.AppendLine(northString);

            var w = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.West);
            var e = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.East);

            var middleString = String.Empty;
            middleString += RenderPathwayToAscii(w, centerRoom.ID, MovementDirectionType.West);
            middleString += RenderRoomToAscii(centerRoom, true);
            middleString += RenderPathwayToAscii(e, centerRoom.ID, MovementDirectionType.East);

            sb.AppendLine(middleString);

            var sw = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.SouthWest);
            var s = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.South);
            var se = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.SouthEast);

            var southString = String.Empty;
            southString += RenderPathwayToAscii(sw, centerRoom.ID, MovementDirectionType.SouthWest);
            southString += RenderPathwayToAscii(s, centerRoom.ID, MovementDirectionType.South);
            southString += RenderPathwayToAscii(se, centerRoom.ID, MovementDirectionType.SouthEast);

            sb.AppendLine(southString);

            var extraString = String.Empty;
            foreach (var path in pathways.Where(path => path.DirectionType == MovementDirectionType.None
                                                        && !string.IsNullOrWhiteSpace(path.ToLocationID)
                                                        && path.ToLocationType.Equals("Room")))
                extraString += "&nbsp;" + RenderPathwayToAscii(path, centerRoom.ID, MovementDirectionType.None);

            //One last for adding non-directional ones
            extraString += "&nbsp;" + RenderPathwayToAscii(null, centerRoom.ID, MovementDirectionType.None);

            if (extraString.Length > 0)
            {
                sb.AppendLine("&nbsp;");
                sb.AppendLine(extraString);
            }

            return sb.ToString();
        }

        private string RenderPathwayToAscii(IPathwayData path, long originId, MovementDirectionType directionType)
        {
            var returnValue = String.Empty;
            var asciiCharacter = MessagingUtility.TranslateDirectionToAsciiCharacter(directionType);

            if (path != null)
            {
                var destination = BackingDataCache.Get<IRoomData>(long.Parse(path.ToLocationID));

                returnValue = String.Format("<a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editData pathway' title='Edit - {1}' data-id='{0}'>{2}</a>",
                    path.ID, destination.Name, asciiCharacter);
            }
            else
                returnValue = String.Format("<a href='/GameAdmin/Pathway/Add/{0}' class='addData pathway' target='_blank' data-direction='{1}' title='Add - {2} path and room'>+</a>",
                    originId, MessagingUtility.TranslateDirectionToDegrees(directionType), directionType.ToString());

            return returnValue;
        }

        private string RenderRoomToAscii(IRoomData destination, bool centered)
        {
            var character = centered ? "0" : "*";

            return String.Format("<a href='/GameAdmin/Room/Edit/{0}' class='editData room' target='_blank' title='Edit - {2}'>{1}</a>", destination.ID, character, destination.Name);
        }
    }
}
