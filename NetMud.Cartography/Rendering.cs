using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Linq;
using System.Text;

namespace NetMud.Cartography
{
    /// <summary>
    /// General set of methods to render rooms, zones and worlds into ascii maps
    /// </summary>
    public static class Rendering
    {
        /// <summary>
        /// Render an ascii map of live rooms around a specific radius (always includes pathways, never includes editing links)
        /// </summary>
        /// <param name="room">the room to render the radius around</param>
        /// <param name="radius">the radius around the room to render</param>
        /// <returns>a single string that is an ascii map</returns>
        public static string RenderRadiusMap(IRoom room, int radius)
        {
            return RenderRadiusMap(room.DataTemplate<IRoomData>(), radius, false);
        }

        /// <summary>
        /// Render the ascii map of room data for the locale based around the center room of the zIndex (negative 1 zIndex is treated as central room of entire set)
        /// </summary>
        /// <param name="locale">The locale to render for</param>
        /// <param name="radius">The radius of rooms to go out to</param>
        /// <param name="zIndex">The zIndex plane to get</param>
        /// <param name="forAdmin">Is this for admin purposes? (makes it have editor links)</param>
        /// <param name="withPathways">Include pathways? (inflated map)</param>
        /// <returns>a single string that is an ascii map</returns>
        public static Tuple<string, string, string> RenderRadiusMap(ILocaleData locale, int radius, int zIndex, bool forAdmin = true, bool withPathways = true)
        {
            var centerRoom = locale.CentralRoom(zIndex);

            var over = RenderRadiusMap(centerRoom, radius, forAdmin, withPathways, locale, MapRenderMode.Upwards);
            var here = RenderRadiusMap(centerRoom, radius, forAdmin, withPathways, locale, MapRenderMode.Normal);
            var under = RenderRadiusMap(centerRoom, radius, forAdmin, withPathways, locale, MapRenderMode.Downwards); 

            return new Tuple<string, string, string>(over, here, under);
        }

        /// <summary>
        /// Render an ascii map of stored data rooms around a specific radius
        /// </summary>
        /// <param name="room">the room to render the radius around</param>
        /// <param name="radius">the radius around the room to render</param>
        /// <param name="forAdmin">include edit links for paths and rooms?</param>
        /// <param name="withPathways">include paths at all?</param>
        /// <returns>a single string that is an ascii map</returns>
        public static string RenderRadiusMap(IRoomData room, int radius, bool forAdmin = true, bool withPathways = true, ILocaleData locale = null, MapRenderMode renderMode = MapRenderMode.Normal)
        {
            var asciiMap = new StringBuilder();

            //Why?
            if (room == null)
            {
                //Don't show "add room" to non admins, if we're not requesting this for a locale and if the locale has actual rooms
                if(!forAdmin || locale == null || locale.Rooms().Count() > 0)
                    return string.Empty;

                return string.Format("<a href='#' class='addData pathway AdminAddInitialRoom' localeId='{0}' title='New Room'>Add Initial Room</a>", locale.Id);
            }

            //1. Get world map
            var ourLocale = room.ParentLocation;

            //2. Get slice of room from world map
            var map = Cartographer.TakeSliceOfMap(new Tuple<int, int>(Math.Max(room.Coordinates.Item1 - radius, 0), room.Coordinates.Item1 + radius)
                                                , new Tuple<int, int>(Math.Max(room.Coordinates.Item2 - radius, 0), room.Coordinates.Item2 + radius)
                                                , new Tuple<int, int>(Math.Max(room.Coordinates.Item3 - 1, 0), room.Coordinates.Item3 + 1)
                                                , ourLocale.Interior.CoordinatePlane, true);

            //3. Flatten the map
            var flattenedMap = Cartographer.GetSinglePlane(map, room.Coordinates.Item3);

            //4. Render slice of room
            return RenderMap(flattenedMap, forAdmin, withPathways, room, renderMode);
        }

        /// <summary>
        /// Renders a map from a single z,y plane
        /// </summary>
        /// <param name="map">The map to render</param>
        /// <param name="forAdmin">is this for admin (with edit links)</param>
        /// <param name="withPathways">include pathway symbols</param>
        /// <param name="centerRoom">the room considered "center"</param>
        /// <returns>the rendered map</returns>
        public static string RenderMap(long[,] map, bool forAdmin, bool withPathways, IRoomData centerRoom, MapRenderMode renderMode = MapRenderMode.Normal)
        {
            var sb = new StringBuilder();

            if (!withPathways)
            {
                int x, y;
                for (y = map.GetUpperBound(1); y >= 0; y--)
                {
                    var rowString = String.Empty;
                    for (x = 0; x < map.GetUpperBound(0); x++)
                    {
                        var roomData = BackingDataCache.Get<IRoomData>(map[x, y]);

                        if (roomData != null)
                            rowString += RenderRoomToAscii(roomData, centerRoom.Equals(roomData), forAdmin);
                        else
                            rowString += "&nbsp;";
                    }

                    sb.AppendLine(rowString);
                }
            }
            else
            {
                var expandedMap = new string[(map.GetUpperBound(0) + 1) * 3 + 1, (map.GetUpperBound(1) + 1) * 3 + 1];

                int x, y;
                for (y = map.GetUpperBound(1); y >= 0; y--)
                {
                    for (x = 0; x <= map.GetUpperBound(0); x++)
                    {
                        var roomData = BackingDataCache.Get<IRoomData>(map[x, y]);

                        if (roomData != null)
                            expandedMap = RenderRoomAndPathwaysForMapNode(x, y, roomData, centerRoom, expandedMap, forAdmin, renderMode);
                    }
                }

                for (y = expandedMap.GetUpperBound(1); y >= 0; y--)
                {
                    var rowString = String.Empty;
                    for (x = 0; x <= expandedMap.GetUpperBound(0); x++)
                        rowString += expandedMap[x, y];

                    sb.AppendLine(rowString);
                }
            }

            return sb.ToString();
        }

        private static string[,] RenderRoomAndPathwaysForMapNode(int x, int y, IRoomData roomData, IRoomData centerRoom, string[,] expandedMap, bool forAdmin, MapRenderMode renderMode)
        {
            var pathways = roomData.GetPathways();
            var expandedRoomX = x * 3 + 1;
            var expandedRoomY = y * 3 + 1;

            switch (renderMode)
            {
                case MapRenderMode.Normal:
                    var ePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.East);
                    var nPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.North);
                    var nePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.NorthEast);
                    var nwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.NorthWest);
                    var sPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.South);
                    var sePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.SouthEast);
                    var swPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.SouthWest);
                    var wPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.West);

                    //The room
                    expandedMap[expandedRoomX, expandedRoomY] = RenderRoomToAscii(roomData, centerRoom.Equals(roomData), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nwPath, roomData.Id, MovementDirectionType.NorthWest
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.NorthWest), forAdmin);

                    expandedMap[expandedRoomX, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nPath, roomData.Id, MovementDirectionType.North
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.North), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nePath, roomData.Id, MovementDirectionType.NorthEast
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.NorthEast), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY] = RenderPathwayToAsciiForModals(wPath, roomData.Id, MovementDirectionType.West
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.West), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY] = RenderPathwayToAsciiForModals(ePath, roomData.Id, MovementDirectionType.East
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.East), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(swPath, roomData.Id, MovementDirectionType.SouthWest
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.SouthWest), forAdmin);

                    expandedMap[expandedRoomX, expandedRoomY - 1] = RenderPathwayToAsciiForModals(sPath, roomData.Id, MovementDirectionType.South
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.South), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(sePath, roomData.Id, MovementDirectionType.SouthEast
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.SouthEast), forAdmin);

                    break;
                case MapRenderMode.Upwards:
                    var upPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.Up);
                    var upePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpEast);
                    var upnPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpNorth);
                    var upnePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpNorthEast);
                    var upnwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpNorthWest);
                    var upsPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpSouth);
                    var upsePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpSouthEast);
                    var upswPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpSouthWest);
                    var upwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpWest);

                    //The room
                    expandedMap[expandedRoomX, expandedRoomY] = RenderPathwayToAsciiForModals(upPath, roomData.Id, MovementDirectionType.Up
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.Up), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(upnwPath, roomData.Id, MovementDirectionType.UpNorthWest
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.UpNorthWest), forAdmin);

                    expandedMap[expandedRoomX, expandedRoomY + 1] = RenderPathwayToAsciiForModals(upnPath, roomData.Id, MovementDirectionType.UpNorth
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.UpNorth), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(upnePath, roomData.Id, MovementDirectionType.UpNorthEast
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.UpNorthEast), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY] = RenderPathwayToAsciiForModals(upwPath, roomData.Id, MovementDirectionType.UpWest
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.UpWest), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY] = RenderPathwayToAsciiForModals(upePath, roomData.Id, MovementDirectionType.UpEast
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.UpEast), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(upswPath, roomData.Id, MovementDirectionType.UpSouthWest
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.UpSouthWest), forAdmin);

                    expandedMap[expandedRoomX, expandedRoomY - 1] = RenderPathwayToAsciiForModals(upsPath, roomData.Id, MovementDirectionType.UpSouth
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.UpSouth), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(upsePath, roomData.Id, MovementDirectionType.UpSouthEast
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.UpSouthEast), forAdmin);

                    break;
                case MapRenderMode.Downwards:
                    var downPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.Down);
                    var downePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownEast);
                    var downnPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownNorth);
                    var downnePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownNorthEast);
                    var downnwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownNorthWest);
                    var downsPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownSouth);
                    var downsePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownSouthEast);
                    var downswPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownSouthWest);
                    var downwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownWest);

                    //The room
                    expandedMap[expandedRoomX, expandedRoomY] = RenderPathwayToAsciiForModals(downPath, roomData.Id, MovementDirectionType.Down
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.Down), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(downnwPath, roomData.Id, MovementDirectionType.DownNorthWest
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.DownNorthWest), forAdmin);

                    expandedMap[expandedRoomX, expandedRoomY + 1] = RenderPathwayToAsciiForModals(downnPath, roomData.Id, MovementDirectionType.DownNorth
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.DownNorth), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(downnePath, roomData.Id, MovementDirectionType.DownNorthEast
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.DownNorthEast), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY] = RenderPathwayToAsciiForModals(downwPath, roomData.Id, MovementDirectionType.DownWest
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.DownWest), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY] = RenderPathwayToAsciiForModals(downePath, roomData.Id, MovementDirectionType.DownEast
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.DownEast), forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(downswPath, roomData.Id, MovementDirectionType.DownSouthWest
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.DownSouthWest), forAdmin);

                    expandedMap[expandedRoomX, expandedRoomY - 1] = RenderPathwayToAsciiForModals(downsPath, roomData.Id, MovementDirectionType.DownSouth
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.DownSouth), forAdmin);

                    expandedMap[expandedRoomX + 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(downsePath, roomData.Id, MovementDirectionType.DownSouthEast
                                                                                        , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.DownSouthEast), forAdmin);

                    break;
            }

            return expandedMap;
        }

        private static string RenderPathwayToAsciiForModals(IPathwayData path, long originId, MovementDirectionType directionType, IRoomData destination, bool forAdmin = false)
        {
            var returnValue = String.Empty;
            var asciiCharacter = Utilities.TranslateDirectionToAsciiCharacter(directionType);

            if (!forAdmin)
                return "&nbsp;";

            if (path != null)
                destination = (IRoomData)path.Destination;

            long destinationId = -1;
            var destinationName = String.Empty;
            if (destination != null)
            {
                destinationName = destination.Name;
                destinationId = destination.Id;
            }

            if (path != null)
            {
                returnValue = String.Format("<a href='#' class='editData pathway AdminEditPathway' pathwayId='{0}' fromRoom='{3}' toRoom='{4}' title='Edit - {5} path to {1}' data-id='{0}'>{2}</a>",
                    path.Id, destinationName, asciiCharacter, originId, destinationId, directionType.ToString());
            }
            else
            {
                var roomString = String.Format("Add - {0} path and room", directionType.ToString());

                if (!string.IsNullOrWhiteSpace(destinationName))
                    roomString = String.Format("Add {0} path to {1}", directionType.ToString(), destinationName);

                returnValue = String.Format("<a href='#' class='addData pathway AdminAddPathway' pathwayId='-1' fromRoom='{0}' toRoom='{3}' data-direction='{1}' title='{2}'>+</a>",
                    originId, Utilities.TranslateDirectionToDegrees(directionType).Item1, roomString, destinationId);
            }

            return returnValue;
        }

        private static string RenderRoomToAscii(IRoomData destination, bool centered, bool forAdmin = false)
        {
            var character = centered ? "0" : "*";

            if (forAdmin)
                return String.Format("<a href='#' class='editData AdminEditRoom' roomId='{0}' title='Edit - {2}'>{1}</a>", destination.Id, character, destination.Name);
            else
                return character;
        }

    }
}
