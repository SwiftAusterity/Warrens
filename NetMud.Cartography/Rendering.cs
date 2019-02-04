using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;
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
        public static string RenderRadiusMap(IRoom room, int radius, bool visibleOnly = false)
        {
            return RenderRadiusMap(room.Template<IRoomTemplate>(), radius, visibleOnly, false);
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
        public static Tuple<string, string, string> RenderRadiusMap(ILocaleTemplate locale, int radius, int zIndex, bool forAdmin = true, bool withPathways = true)
        {
            IRoomTemplate centerRoom = locale.CentralRoom(zIndex);

            string over = RenderRadiusMap(centerRoom, radius, false, forAdmin, withPathways, locale, MapRenderMode.Upwards);
            string here = RenderRadiusMap(centerRoom, radius, false, forAdmin, withPathways, locale, MapRenderMode.Normal);
            string under = RenderRadiusMap(centerRoom, radius, false, forAdmin, withPathways, locale, MapRenderMode.Downwards);

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
        public static string RenderRadiusMap(IRoomTemplate room, int radius, bool visibleOnly = false, bool forAdmin = true, bool withPathways = true, ILocaleTemplate locale = null, MapRenderMode renderMode = MapRenderMode.Normal)
        {
            StringBuilder asciiMap = new StringBuilder();

            //Why?
            if (room == null)
            {
                //Don't show "add room" to non admins, if we're not requesting this for a locale and if the locale has actual rooms
                if (!forAdmin || locale == null || locale.Rooms().Count() > 0)
                {
                    return string.Empty;
                }

                return string.Format("<a href='#' class='addData pathway AdminAddInitialRoom' localeId='{0}' title='New Room'>Add Initial Room</a>", locale.Id);
            }

            //1. Get world map
            ILocaleTemplate ourLocale = room.ParentLocation;

            //2. Get slice of room from world map
            long[,,] map = Cartographer.TakeSliceOfMap(new Tuple<int, int>(Math.Max(room.Coordinates.X - radius, 0), room.Coordinates.X + radius)
                                                , new Tuple<int, int>(Math.Max(room.Coordinates.Y - radius, 0), room.Coordinates.Y + radius)
                                                , new Tuple<int, int>(Math.Max(room.Coordinates.Z - 1, 0), room.Coordinates.Z + 1)
                                                , ourLocale.Interior.CoordinatePlane, true);

            //3. Flatten the map
            long[,] flattenedMap = Cartographer.GetSinglePlane(map, room.Coordinates.Z);

            //4. Render slice of room
            return RenderMap(flattenedMap, visibleOnly, forAdmin, withPathways, room, renderMode);
        }

        /// <summary>
        /// Renders a map from a single z,y plane
        /// </summary>
        /// <param name="map">The map to render</param>
        /// <param name="forAdmin">is this for admin (with edit links)</param>
        /// <param name="withPathways">include pathway symbols</param>
        /// <param name="centerRoom">the room considered "center"</param>
        /// <returns>the rendered map</returns>
        public static string RenderMap(long[,] map, bool visibileOnly, bool forAdmin, bool withPathways, IRoomTemplate centerRoom, MapRenderMode renderMode = MapRenderMode.Normal)
        {
            StringBuilder sb = new StringBuilder();

            if (!withPathways)
            {
                int x, y;
                for (y = map.GetUpperBound(1); y >= 0; y--)
                {
                    string rowString = string.Empty;
                    for (x = 0; x < map.GetUpperBound(0); x++)
                    {
                        IRoomTemplate RoomTemplate = TemplateCache.Get<IRoomTemplate>(map[x, y]);

                        if (RoomTemplate != null)
                        {
                            rowString += RenderRoomToAscii(RoomTemplate, RoomTemplate.GetZonePathways().Any(), RoomTemplate.GetLocalePathways().Any(), !forAdmin && RoomTemplate.Id == centerRoom.Id, forAdmin);
                        }
                        else
                        {
                            rowString += "&nbsp;";
                        }
                    }

                    sb.AppendLine(rowString);
                }
            }
            else
            {
                string[,] expandedMap = new string[(map.GetUpperBound(0) + 1) * 3 + 1, (map.GetUpperBound(1) + 1) * 3 + 1];

                int x, y;
                for (y = map.GetUpperBound(1); y >= 0; y--)
                {
                    for (x = 0; x <= map.GetUpperBound(0); x++)
                    {
                        IRoomTemplate RoomTemplate = TemplateCache.Get<IRoomTemplate>(map[x, y]);

                        if (RoomTemplate != null)
                        {
                            expandedMap = RenderRoomAndPathwaysForMapNode(x, y, RoomTemplate, centerRoom, expandedMap, RoomTemplate.Id == centerRoom.Id, forAdmin, renderMode);
                        }
                    }
                }

                for (y = expandedMap.GetUpperBound(1); y >= 0; y--)
                {
                    string rowString = string.Empty;
                    for (x = 0; x <= expandedMap.GetUpperBound(0); x++)
                    {
                        var xString = expandedMap[x, y];
                        if (string.IsNullOrWhiteSpace(xString))
                        {
                            rowString += "&nbsp;";
                        }
                        else
                        {
                            rowString += expandedMap[x, y];
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(rowString.Replace("&nbsp;", "")))
                    {
                        sb.AppendLine(rowString);
                    }
                }
            }

            return sb.ToString();
        }

        private static string[,] RenderRoomAndPathwaysForMapNode(int x, int y, IRoomTemplate RoomTemplate, IRoomTemplate centerRoom, string[,] expandedMap, bool currentRoom, bool forAdmin, MapRenderMode renderMode)
        {
            IEnumerable<IPathwayTemplate> pathways = RoomTemplate.GetPathways();
            int expandedRoomX = x * 3 + 1;
            int expandedRoomY = y * 3 + 1;

            switch (renderMode)
            {
                case MapRenderMode.Normal:
                    IPathwayTemplate ePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.East);
                    IPathwayTemplate nPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.North);
                    IPathwayTemplate nePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.NorthEast);
                    IPathwayTemplate nwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.NorthWest);
                    IPathwayTemplate sPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.South);
                    IPathwayTemplate sePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.SouthEast);
                    IPathwayTemplate swPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.SouthWest);
                    IPathwayTemplate wPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.West);

                    //The room
                    expandedMap[expandedRoomX, expandedRoomY] = RenderRoomToAscii(RoomTemplate, RoomTemplate.GetZonePathways().Any(), RoomTemplate.GetLocalePathways().Any()
                                                                                    , !forAdmin && currentRoom, forAdmin);

                    expandedMap[expandedRoomX - 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nwPath, RoomTemplate.Id, MovementDirectionType.NorthWest, forAdmin);
                    expandedMap[expandedRoomX, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nPath, RoomTemplate.Id, MovementDirectionType.North, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(nePath, RoomTemplate.Id, MovementDirectionType.NorthEast, forAdmin);
                    expandedMap[expandedRoomX - 1, expandedRoomY] = RenderPathwayToAsciiForModals(wPath, RoomTemplate.Id, MovementDirectionType.West, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY] = RenderPathwayToAsciiForModals(ePath, RoomTemplate.Id, MovementDirectionType.East, forAdmin);
                    expandedMap[expandedRoomX - 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(swPath, RoomTemplate.Id, MovementDirectionType.SouthWest, forAdmin);
                    expandedMap[expandedRoomX, expandedRoomY - 1] = RenderPathwayToAsciiForModals(sPath, RoomTemplate.Id, MovementDirectionType.South, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(sePath, RoomTemplate.Id, MovementDirectionType.SouthEast, forAdmin);

                    break;
                case MapRenderMode.Upwards:
                    IPathwayTemplate upPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.Up);
                    IPathwayTemplate upePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpEast);
                    IPathwayTemplate upnPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpNorth);
                    IPathwayTemplate upnePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpNorthEast);
                    IPathwayTemplate upnwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpNorthWest);
                    IPathwayTemplate upsPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpSouth);
                    IPathwayTemplate upsePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpSouthEast);
                    IPathwayTemplate upswPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpSouthWest);
                    IPathwayTemplate upwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.UpWest);

                    expandedMap[expandedRoomX, expandedRoomY] = RenderPathwayToAsciiForModals(upPath, RoomTemplate.Id, MovementDirectionType.Up, forAdmin);
                    expandedMap[expandedRoomX - 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(upnwPath, RoomTemplate.Id, MovementDirectionType.UpNorthWest, forAdmin);
                    expandedMap[expandedRoomX, expandedRoomY + 1] = RenderPathwayToAsciiForModals(upnPath, RoomTemplate.Id, MovementDirectionType.UpNorth, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(upnePath, RoomTemplate.Id, MovementDirectionType.UpNorthEast, forAdmin);
                    expandedMap[expandedRoomX - 1, expandedRoomY] = RenderPathwayToAsciiForModals(upwPath, RoomTemplate.Id, MovementDirectionType.UpWest, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY] = RenderPathwayToAsciiForModals(upePath, RoomTemplate.Id, MovementDirectionType.UpEast, forAdmin);
                    expandedMap[expandedRoomX - 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(upswPath, RoomTemplate.Id, MovementDirectionType.UpSouthWest, forAdmin);
                    expandedMap[expandedRoomX, expandedRoomY - 1] = RenderPathwayToAsciiForModals(upsPath, RoomTemplate.Id, MovementDirectionType.UpSouth, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(upsePath, RoomTemplate.Id, MovementDirectionType.UpSouthEast, forAdmin);

                    break;
                case MapRenderMode.Downwards:
                    IPathwayTemplate downPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.Down);
                    IPathwayTemplate downePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownEast);
                    IPathwayTemplate downnPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownNorth);
                    IPathwayTemplate downnePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownNorthEast);
                    IPathwayTemplate downnwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownNorthWest);
                    IPathwayTemplate downsPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownSouth);
                    IPathwayTemplate downsePath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownSouthEast);
                    IPathwayTemplate downswPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownSouthWest);
                    IPathwayTemplate downwPath = pathways.FirstOrDefault(path => path.DirectionType == MovementDirectionType.DownWest);

                    expandedMap[expandedRoomX, expandedRoomY] = RenderPathwayToAsciiForModals(downPath, RoomTemplate.Id, MovementDirectionType.Down, forAdmin);
                    expandedMap[expandedRoomX - 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(downnwPath, RoomTemplate.Id, MovementDirectionType.DownNorthWest, forAdmin);
                    expandedMap[expandedRoomX, expandedRoomY + 1] = RenderPathwayToAsciiForModals(downnPath, RoomTemplate.Id, MovementDirectionType.DownNorth, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY + 1] = RenderPathwayToAsciiForModals(downnePath, RoomTemplate.Id, MovementDirectionType.DownNorthEast, forAdmin);
                    expandedMap[expandedRoomX - 1, expandedRoomY] = RenderPathwayToAsciiForModals(downwPath, RoomTemplate.Id, MovementDirectionType.DownWest, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY] = RenderPathwayToAsciiForModals(downePath, RoomTemplate.Id, MovementDirectionType.DownEast, forAdmin);
                    expandedMap[expandedRoomX - 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(downswPath, RoomTemplate.Id, MovementDirectionType.DownSouthWest, forAdmin);
                    expandedMap[expandedRoomX, expandedRoomY - 1] = RenderPathwayToAsciiForModals(downsPath, RoomTemplate.Id, MovementDirectionType.DownSouth, forAdmin);
                    expandedMap[expandedRoomX + 1, expandedRoomY - 1] = RenderPathwayToAsciiForModals(downsePath, RoomTemplate.Id, MovementDirectionType.DownSouthEast, forAdmin);

                    break;
            }

            return expandedMap;
        }

        private static string RenderPathwayToAsciiForModals(IPathwayTemplate path, long originId, MovementDirectionType directionType, bool forAdmin = false)
        {
            string returnValue = string.Empty;
            string asciiCharacter = Utilities.TranslateDirectionToAsciiCharacter(directionType);
            long destinationId = -1;
            string destinationName = string.Empty;
            var pathType = PathwayType.None;

            if (path != null && path.Destination != null)
            {
                destinationName = path.Destination.Name;
                destinationId = path.Destination.Id;
                pathType = path.Type;
            }

            if (forAdmin)
            {
                if (path != null)
                {
                    if (pathType == PathwayType.Rooms)
                    {
                        returnValue = string.Format("<a href='#' class='editData pathway AdminEditPathway' pathwayId='{0}' fromRoom='{3}' toRoom='{4}' title='Edit - {5} path to {1}' data-id='{0}'>{2}</a>",
                            path.Id, destinationName, asciiCharacter, originId, destinationId, directionType.ToString());
                    }
                    else
                    {
                        var classString = "zone";

                        if (pathType == PathwayType.Locale)
                        {
                            classString = "locale";
                        }

                        returnValue = string.Format("<a class='pathway {4}' title='{5}: {3} to {1}' data-id='{0}'>{2}</a>",
                            path.Id, destinationName, asciiCharacter, directionType.ToString(), classString, path.Name);
                    }
                }
                else
                {
                    string roomString = string.Format("Add - {0} path and room", directionType.ToString());

                    if (!string.IsNullOrWhiteSpace(destinationName))
                    {
                        roomString = string.Format("Add {0} path to {1}", directionType.ToString(), destinationName);
                    }

                    returnValue = string.Format("<a href='#' class='addData pathway AdminAddPathway' pathwayId='-1' fromRoom='{0}' toRoom='{4}' data-direction='{1}' data-incline='{2}' title='{3}'>+</a>",
                        originId, Utilities.TranslateDirectionToDegrees(directionType).Item1, Utilities.GetBaseInclineGrade(directionType), roomString, destinationId);
                }
            }
            else if (path != null)
            {
                if (pathType == PathwayType.Rooms)
                {
                    returnValue = string.Format("<a href='#' class='pathway nonAdminPathway' title='{3}: {1}{4} to {2}{5}' data-id='{6}'>{0}</a>",
                        asciiCharacter, directionType, destinationName, path.Name, originId, destinationId, path.Id);
                }
                else
                {
                    var classString = "zone";

                    if (pathType == PathwayType.Locale)
                    {
                        classString = "locale";
                    }

                    returnValue = string.Format("<a class='pathway {4}' title='{5}: {3} to {1}' data-id='{0}'>{2}</a>",
                        path.Id, destinationName, asciiCharacter, directionType.ToString(), classString, path.Name);
                }
            }

            return returnValue;
        }

        private static string RenderRoomToAscii(IRoomTemplate destination, bool hasZoneExits, bool hasLocaleExits, bool isCurrentLocation, bool forAdmin = false)
        {
            MovementDirectionType[] upPaths = new MovementDirectionType[] {
                MovementDirectionType.Up,
                MovementDirectionType.UpEast,
                MovementDirectionType.UpNorth,
                MovementDirectionType.UpNorthEast,
                MovementDirectionType.UpNorthWest,
                MovementDirectionType.UpSouth,
                MovementDirectionType.UpSouthEast,
                MovementDirectionType.UpSouthWest,
                MovementDirectionType.UpWest
            };

            MovementDirectionType[] downPaths = new MovementDirectionType[] {
                MovementDirectionType.Down,
                MovementDirectionType.DownEast,
                MovementDirectionType.DownNorth,
                MovementDirectionType.DownNorthEast,
                MovementDirectionType.DownNorthWest,
                MovementDirectionType.DownSouth,
                MovementDirectionType.DownSouthEast,
                MovementDirectionType.DownSouthWest,
                MovementDirectionType.DownWest
            };

            var hasUp = destination.GetPathways().Any(path => upPaths.Contains(path.DirectionType));
            var hasDown = destination.GetPathways().Any(path => downPaths.Contains(path.DirectionType));
            var extraClass = "";

            string character = "O";

            if (forAdmin)
            {
                if (hasZoneExits && hasLocaleExits)
                {
                    character = "$";
                }
                else if (hasZoneExits)
                {
                    character = "Z";
                }
                else if (hasLocaleExits)
                {
                    character = "L";
                }
            }
            else
            {
                if (isCurrentLocation)
                {
                    character = "@";
                    extraClass = "isHere";
                }
                else if (hasUp)
                {
                    if (hasDown)
                    {
                        character = "X";
                    }
                    else
                    {
                        character = "A";
                    }
                }
                else if (hasDown)
                {
                    character = "V";
                }
            }

            if (forAdmin)
            {
                return string.Format("<a href='#' class='editData AdminEditRoom' roomId='{0}' title='Edit - {2}'>{1}</a>", destination.Id, character, destination.Name);
            }
            else
            {
                return string.Format("<a href='#' class='room nonAdminRoom {3}' title='{1}{2}'>{0}</a>", character, destination.Name, destination.Id, extraClass);
            }
        }

    }
}
