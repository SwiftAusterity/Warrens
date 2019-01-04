using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using System;
using System.Text;

namespace NetMud.Cartography
{
    /// <summary>
    /// General set of methods to render rooms, zones and worlds into ascii maps
    /// </summary>
    public static class Rendering
    {
        /// <summary>
        /// Render the ascii map of room data for the locale based around the center room of the zIndex (negative 1 zIndex is treated as central room of entire set)
        /// </summary>
        /// <param name="locale">The locale to render for</param>
        /// <param name="radius">The radius of rooms to go out to</param>
        /// <param name="zIndex">The zIndex plane to get</param>
        /// <param name="forAdmin">Is this for admin purposes? (makes it have editor links)</param>
        /// <param name="withPathways">Include pathways? (inflated map)</param>
        /// <returns>a single string that is an ascii map</returns>
        public static string RenderRadiusMap(IZone zone, ITile centerTile, short radiusX, short radiusY, IActor protagonist, bool forAdmin = true)
        {
            return RenderRadiusMap(zone, centerTile.Coordinate.X, centerTile.Coordinate.Y, radiusX, radiusY, protagonist, forAdmin);
        }

        /// <summary>
        /// Render an ascii map of stored data rooms around a specific radius
        /// </summary>
        /// <param name="room">the room to render the radius around</param>
        /// <param name="radius">the radius around the room to render</param>
        /// <param name="forAdmin">include edit links for paths and rooms?</param>
        /// <param name="withPathways">include paths at all?</param>
        /// <returns>a single string that is an ascii map</returns>
        public static string RenderRadiusMap(IZone zone, int x, int y, short radiusX, short radiusY, IActor protagonist, bool forAdmin = true)
        {
            //Why?
            if (zone == null)
                return string.Empty;

            int xLowerBound = x + radiusX < 100
                                ? Math.Max(x - radiusX, 0)
                                : Math.Max(0, 100 - radiusX * 2);

            int xUpperBound = x - radiusX >= 0 
                                ? Math.Min(99, x + radiusX)
                                : Math.Min(99, radiusX * 2);

            int yLowerBound = y + radiusY < 100
                               ? Math.Max(y - radiusY, 0)
                               : Math.Max(0, 100 - radiusY * 2);

            int yUpperBound = y - radiusY >= 0
                                ? Math.Min(99, y + radiusY)
                                : Math.Min(99, radiusY * 2);

            //2. Get slice of room from world map
            ITile[,] map = Cartographer.TakeSliceOfMap(new ValueRange<int>(xLowerBound, xUpperBound)
                                                , new ValueRange<int>(yLowerBound, yUpperBound)
                                                , zone.Map.CoordinateTilePlane, radiusX, radiusY);

            //4. Render slice of room
            return RenderMap(zone, map, forAdmin, x, y, protagonist);
        }

        /// <summary>
        /// Renders a map from a single z,y plane
        /// </summary>
        /// <param name="map">The map to render</param>
        /// <param name="forAdmin">is this for admin (with edit links)</param>
        /// <param name="withPathways">include pathway symbols</param>
        /// <param name="centerRoom">the room considered "center"</param>
        /// <returns>the rendered map</returns>
        public static string RenderMap(IZone zone, ITile[,] map, bool forAdmin, int xOrigin, int yOrigin, IActor protagonist)
        {
            StringBuilder sb = new StringBuilder();

            int x, y;
            for (y = map.GetUpperBound(1); y >= 0; y--)
            {
                string rowString = string.Empty;
                for (x = 0; x < map.GetUpperBound(0); x++)
                {
                    ITile tile = map[x, y];

                    if (tile == null)
                        continue;

                    rowString += RenderRoomToAscii(zone, tile, false, tile.Coordinate.X, tile.Coordinate.Y, protagonist, forAdmin);
                }

                sb.AppendLine(rowString);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Renders a map from a single z,y plane
        /// </summary>
        /// <param name="map">The map to render</param>
        /// <param name="forAdmin">is this for admin (with edit links)</param>
        /// <param name="withPathways">include pathway symbols</param>
        /// <param name="centerRoom">the room considered "center"</param>
        /// <returns>the rendered map</returns>
        public static string RenderOneTile(IZone zone, int xOrigin, int yOrigin, IActor protagonist, bool forAdmin = false)
        {
            return RenderRoomToAscii(zone, zone.Map.CoordinateTilePlane[xOrigin, yOrigin], true, xOrigin, yOrigin, protagonist, forAdmin);
        }

        private static string RenderRoomToAscii(IZone zone, ITile destination, bool hasZoneExits, int x, int y, IActor protagonist, bool forAdmin = false)
        {
            if (destination == null)
                return string.Format("<div href='#' contents-name-data='' x-data='{0}' y-data='{1}' class='tile tileTip' data-tiletip-description='ERROR' title='Space' style='color: gray;'><span>.</span></div>", x, y);

            ITemplate intruder = destination.TopContents()?.Template<ITemplate>();
            IPathway pathway = destination.Pathway();
            string character = " ";
            string colorCode = "#FFFFFF";
            string backgroundColorCode = "#000000";
            string border = string.Empty;
            string contents = string.Empty;
            string tileTipDescription = string.Empty;

            if (destination.Type != null)
            {
                character = destination.Type.AsciiCharacter;
                colorCode = destination.Type.HexColorCode;
                backgroundColorCode = destination.Type.BackgroundHexColor;
            }

            if (intruder != null)
            {
                character = intruder.AsciiCharacter;
                colorCode = intruder.HexColorCode;
            }

            if (pathway != null)
            {
                border = string.Format("border: 1px dotted {0};", string.IsNullOrWhiteSpace(pathway.BorderHexColor) ? "#FFFFFF" : pathway.BorderHexColor);
            }

            if(x == 0)
            {
                border += "border-left: 4px solid white;";
            }

            if (x == 100)
            {
                border += "border-right: 4px solid white;";
            }

            if (y == 0)
            {
                border += "border-bottom: 4px solid white;";
            }

            if (y == 100)
            {
                border += "border-top: 4px solid white;";
            }

            if (forAdmin)
                return string.Format("<div content-name-data=\"{6}\" zone-data=\"{7}\" x-data=\"{2}\" y-data=\"{3}\" class=\"editData tile tileTip\" style=\"color: {1}; background-color: {4}; {5}\"><span>{0}</span></div>", character, colorCode, x, y, backgroundColorCode, border, contents, zone.TemplateId);
            else
                return string.Format("<div content-name-data=\"{6}\" zone-data=\"{7}\" x-data=\"{2}\" y-data=\"{3}\" class=\"moveTo tile tileTip\" style=\"color: {1}; background-color: {4}; {5}\"><span>{0}</span></div>", character, colorCode, x, y, backgroundColorCode, border, contents, zone.TemplateId);
        }

    }
}
