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
        /// Render an ascii map of stored data rooms around a specific radius
        /// </summary>
        /// <param name="room">the room to render the radius around</param>
        /// <param name="radius">the radius around the room to render</param>
        /// <param name="forAdmin">include edit links for paths and rooms?</param>
        /// <param name="withPathways">include paths at all?</param>
        /// <returns>a single string that is an ascii map</returns>
        public static string RenderRadiusMap(IRoomData room, int radius, bool forAdmin = true, bool withPathways = true)
        {
            var asciiMap = new StringBuilder();

            //1. Get world map
            var ourLocale = room.Affiliation;

            //2. Get slice of room from world map
            var map = Cartographer.TakeSliceOfMap(new Tuple<int, int>(Math.Max(room.Coordinates.Item1 - radius, 0), room.Coordinates.Item1 + radius)
                                                , new Tuple<int, int>(Math.Max(room.Coordinates.Item2 - radius, 0), room.Coordinates.Item2 + radius)
                                                , new Tuple<int, int>(Math.Max(room.Coordinates.Item3 - 1, 0), room.Coordinates.Item3 + 1)
                                                , ourLocale.Interior.CoordinatePlane, true);

            //3. Flatten the map
            var flattenedMap = Cartographer.GetSinglePlane(map, room.Coordinates.Item3);

            //4. Render slice of room
            return RenderMap(flattenedMap, forAdmin, withPathways, room);
        }

        /// <summary>
        /// Renders a map from a single z,y plane
        /// </summary>
        /// <param name="map">The map to render</param>
        /// <param name="forAdmin">is this for admin (with edit links)</param>
        /// <param name="withPathways">include pathway symbols</param>
        /// <param name="centerRoom">the room considered "center"</param>
        /// <returns>the rendered map</returns>
        public static string RenderMap(long[,] map, bool forAdmin, bool withPathways, IRoomData centerRoom)
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
                        {
                            var pathways = roomData.GetPathways();
                            var ePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.East);
                            var nPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.North);
                            var nePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.NorthEast);
                            var nwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.NorthWest);
                            var sPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.South);
                            var sePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.SouthEast);
                            var swPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.SouthWest);
                            var wPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.West);

                            var expandedRoomX = x * 3 + 1;
                            var expandedRoomY = y * 3 + 1;

                            //The room
                            expandedMap[expandedRoomX, expandedRoomY] = RenderRoomToAscii(roomData, centerRoom.Equals(roomData), forAdmin);

                            expandedMap[expandedRoomX - 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nwPath, roomData.ID, MovementDirectionType.NorthWest
                                                                                                , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.NorthWest), forAdmin);
                            expandedMap[expandedRoomX, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nPath, roomData.ID, MovementDirectionType.North
                                                                                                , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.North), forAdmin);
                            expandedMap[expandedRoomX + 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nePath, roomData.ID, MovementDirectionType.NorthEast
                                                                                                , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.NorthEast), forAdmin);
                            expandedMap[expandedRoomX - 1, expandedRoomY] = RenderPathwayToAsciiForModals(wPath, roomData.ID, MovementDirectionType.West
                                                                                                , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.West), forAdmin);
                            expandedMap[expandedRoomX + 1, expandedRoomY] = RenderPathwayToAsciiForModals(ePath, roomData.ID, MovementDirectionType.East
                                                                                                , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.East), forAdmin);
                            expandedMap[expandedRoomX - 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(swPath, roomData.ID, MovementDirectionType.SouthWest
                                                                                                , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.SouthWest), forAdmin);
                            expandedMap[expandedRoomX, expandedRoomY - 1] = RenderPathwayToAsciiForModals(sPath, roomData.ID, MovementDirectionType.South
                                                                                                , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.South), forAdmin);
                            expandedMap[expandedRoomX + 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(sePath, roomData.ID, MovementDirectionType.SouthEast
                                                                                                , Cartographer.GetRoomInDirection(roomData, MovementDirectionType.SouthEast), forAdmin);
                        }
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

        private static string RenderPathwayToAsciiForModals(IPathwayData path, long originId, MovementDirectionType directionType, IRoomData destination, bool forAdmin = false)
        {
            var returnValue = String.Empty;
            var asciiCharacter = Utilities.TranslateDirectionToAsciiCharacter(directionType);

            if (!forAdmin)
                return "&nbsp;";

            if (path != null)
                destination = BackingDataCache.Get<IRoomData>(int.Parse(path.ToLocationID));

            long destinationId = -1;
            var destinationName = String.Empty;
            if (destination != null)
            {
                destinationName = destination.Name;
                destinationId = destination.ID;
            }

            if (path != null)
            {
                returnValue = String.Format("<a href='#' class='editData pathway AdminEditPathway' pathwayId='{0}' fromRoom='{3}' toRoom='{4}' title='Edit - {5} path to {1}' data-id='{0}'>{2}</a>",
                    path.ID, destinationName, asciiCharacter, originId, destinationId, directionType.ToString());
            }
            else
            {
                var roomString = String.Format("Add - {0} path and room", directionType.ToString());

                if (!string.IsNullOrWhiteSpace(destinationName))
                    roomString = String.Format("Add {0} path to {1}", directionType.ToString(), destinationName);

                returnValue = String.Format("<a href='#' class='addData pathway AdminAddPathway' pathwayId='-1' fromRoom='{0}' toRoom='{3}' data-direction='{1}' title='{2}'>+</a>",
                    originId, Utilities.TranslateDirectionToDegrees(directionType), roomString, destinationId);
            }

            return returnValue;
        }

        private static string RenderRoomToAscii(IRoomData destination, bool centered, bool forAdmin = false)
        {
            var character = centered ? "0" : "*";

            if (forAdmin)
                return String.Format("<a href='#' class='editData AdminEditRoom' roomId='{0}' title='Edit - {2}'>{1}</a>", destination.ID, character, destination.Name);
            else
                return character;
        }

    }
}
