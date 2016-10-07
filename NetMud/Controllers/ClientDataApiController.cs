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
            var room = BackingDataCache.Get<IRoomData>(id);
            var sb = new StringBuilder();

            if (room == null || radius < 0)
                return "Invalid inputs.";

            var pathways = BackingDataCache.GetAll<IPathwayData>().Where(path => path.FromLocationID.Equals(room.ID.ToString()));

            var nw = pathways.FirstOrDefault(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.NorthWest);
            var n = pathways.FirstOrDefault(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.North);
            var ne = pathways.FirstOrDefault(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.NorthEast);

            var northString = String.Empty;
            if (nw != null && !string.IsNullOrWhiteSpace(nw.ToLocationID) && nw.ToLocationType.Equals("Room"))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(nw.ToLocationID));
                northString += String.Format("<a href='/GameAdmin/Pathway/Add/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", nw.ID, location.Name);
            }
            else
                northString += String.Format("<a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='315'>+</a>", id);

            if (n != null && !string.IsNullOrWhiteSpace(n.ToLocationID) && n.ToLocationType.Equals("Room"))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(n.ToLocationID));
                northString += String.Format(" <a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", n.ID, location.Name);
            }
            else
                northString += String.Format(" <a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='0'>+</a>", id);

            if (ne != null && !string.IsNullOrWhiteSpace(ne.ToLocationID) && ne.ToLocationType.Equals("Room"))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(ne.ToLocationID));
                northString += String.Format(" <a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", ne.ID, location.Name);
            }
            else
                northString += String.Format(" <a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='45'>+</a>", id);

            sb.AppendLine(northString);
            sb.AppendLine("\\ | /");

            var w = pathways.FirstOrDefault(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.West);
            var e = pathways.FirstOrDefault(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.East);

            var middleString = String.Empty;
            if (w != null && !string.IsNullOrWhiteSpace(w.ToLocationID) && w.ToLocationType.Equals("Room"))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(w.ToLocationID));
                middleString += String.Format("<a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", w.ID, location.Name);
            }
            else
                middleString += String.Format("<a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='270'>+</a>", id);

            middleString += String.Format("--<a href='/GameAdmin/Room/Edit/{0}' class='addPathway' target='_blank' data-direction='270'>*</a>--", id);

            if (e != null && !string.IsNullOrWhiteSpace(e.ToLocationID) && e.ToLocationType.Equals("Room"))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(e.ToLocationID));
                middleString += String.Format("<a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", e.ID, location.Name);
            }
            else
                middleString += String.Format("<a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='90'>+</a>", id);

            sb.AppendLine(middleString);
            sb.AppendLine("/ | \\");

            var sw = pathways.FirstOrDefault(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.SouthWest);
            var s = pathways.FirstOrDefault(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.South);
            var se = pathways.FirstOrDefault(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.SouthEast);

            var southString = String.Empty;
            if (sw != null && !string.IsNullOrWhiteSpace(sw.ToLocationID) && sw.ToLocationType.Equals("Room"))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(sw.ToLocationID));
                southString += String.Format("<a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", sw.ID, location.Name);
            }
            else
                southString += String.Format("<a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='225'>+</a>", id);

            if (s != null && !string.IsNullOrWhiteSpace(s.ToLocationID) && s.ToLocationType.Equals("Room"))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(s.ToLocationID));
                southString += String.Format(" <a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", s.ID, location.Name);
            }
            else
                southString += String.Format(" <a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='180'>+</a>", id);

            if (se != null && !string.IsNullOrWhiteSpace(se.ToLocationID) && se.ToLocationType.Equals("Room"))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(se.ToLocationID));
                southString += String.Format(" <a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", se.ID, location.Name);
            }
            else
                southString += String.Format(" <a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='135'>+</a>", id);

            sb.AppendLine(southString);

            var extraString = String.Empty;
            foreach (var path in pathways.Where(path => MessagingUtility.TranslateDegreesToDirection(path.DegreesFromNorth) == MovementDirectionType.None
                                                        && !string.IsNullOrWhiteSpace(path.ToLocationID) 
                                                        && path.ToLocationType.Equals("Room")))
            {
                var location = BackingDataCache.Get<IRoomData>(long.Parse(path.ToLocationID));

                if (location != null)
                    extraString += String.Format(" <a href='/GameAdmin/Pathway/Edit/{0}' target='_blank' class='editPathway' title='{1}' data-id='{0}'>#</a>", path.ID, location.Name);
            }

            //One last for adding non-directional ones
            extraString += String.Format(" <a href='/GameAdmin/Pathway/Add/{0}' class='addPathway' target='_blank' data-direction='-1'>+</a>", id);

            if (extraString.Length > 0)
            {
                sb.AppendLine("&nbsp;");
                sb.AppendLine(extraString);
            }

            return sb.ToString();
        }
    }
}
