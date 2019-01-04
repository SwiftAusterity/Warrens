using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Cartography
{

    /// <summary>
    /// General utilities for map rendering and parsing
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Send the entire map to all players in the zone
        /// </summary>
        /// <param name="zone">The zone in question</param>
        public static void SendMapToZone(IZone zone)
        {
            IEnumerable<IPlayer> viewers = LiveCache.GetAll<IPlayer>().Where(pl => pl.CurrentLocation?.CurrentZone == zone);

            foreach (IPlayer viewer in viewers)
                SendMapToPlayer(viewer);
        }

        /// <summary>
        /// Send specific tile updates to all players in the zone
        /// </summary>
        /// <param name="zone">The zone in question</param>
        /// <param name="tiles">the coordinates set to send</param>
        public static void SendMapUpdatesToZone(IZone zone, HashSet<Coordinate> tiles)
        {
            IEnumerable<IPlayer> viewers = LiveCache.GetAll<IPlayer>().Where(pl => pl.CurrentLocation?.CurrentZone == zone);

            foreach (IPlayer viewer in viewers)
                SendMapUpdatesToPlayer(viewer, tiles);
        }

        /// <summary>
        /// Send the entire map to one player
        /// </summary>
        /// <param name="player">the player to send the map to</param>
        public static void SendMapToPlayer(IPlayer player)
        {
            player.Descriptor.SendMap();
        }

        /// <summary>
        /// Send specific tile updates to a specific player
        /// </summary>
        /// <param name="player">the player to send the map to</param>
        /// <param name="tiles">the coordinates set to send</param>
        public static void SendMapUpdatesToPlayer(IPlayer player, HashSet<Coordinate> tiles)
        {
            IZone myZone = player.CurrentLocation.CurrentZone;

            HashSet<Tuple<long, long, string>> mapDeltas = new HashSet<Tuple<long, long, string>>(
                tiles.Select(tile => new Tuple<long, long, string>(tile.X, tile.Y, Rendering.RenderOneTile(myZone, tile.X, tile.Y, player)))
                );

            player.Descriptor.SendMapDeltas(mapDeltas);
        }

        /// <summary>
        /// Is this coordinate out of bounds of the map
        /// </summary>
        /// <param name="boundings">a 3d coordinate x,y,z</param>
        /// <param name="map">the 3d map in question</param>
        /// <returns>whether it is out of bounds of the map</returns>
        public static bool IsOutOfBounds(Coordinate boundings, long[,] map)
        {
            return map.GetUpperBound(0) < boundings.X || map.GetLowerBound(0) > boundings.X
                || map.GetUpperBound(1) < boundings.Y || map.GetLowerBound(1) > boundings.Y;

        }

        /// <summary>
        /// Translates degreesFromNorth into direction words for pathways
        /// </summary>
        /// <param name="degreesFromNorth">the value to translate</param>
        /// <param name="inclineGrade">the value to translate</param>
        /// <param name="reverse">reverse the direction or not</param>
        /// <returns>translated text</returns>
        public static MovementDirectionType TranslateToDirection(int degreesFromNorth, bool reverse = false)
        {
            int trueDegrees = degreesFromNorth;

            if (trueDegrees < 0)
                return MovementDirectionType.None;

            if (reverse)
                trueDegrees = degreesFromNorth < 180 ? degreesFromNorth + 180 : degreesFromNorth - 180;

            if (trueDegrees > 22 && trueDegrees < 67)
                return MovementDirectionType.NorthEast;

            if (trueDegrees > 66 && trueDegrees < 111)
                return MovementDirectionType.East;

            if (trueDegrees > 110 && trueDegrees < 155)
                return MovementDirectionType.SouthEast;

            if (trueDegrees > 154 && trueDegrees < 199)
                return MovementDirectionType.South;

            if (trueDegrees > 198 && trueDegrees < 243)
                return MovementDirectionType.SouthWest;

            if (trueDegrees > 242 && trueDegrees < 287)
                return MovementDirectionType.West;

            if (trueDegrees > 286 && trueDegrees < 331)
                return MovementDirectionType.NorthWest;

            return MovementDirectionType.North;
        }

        /// <summary>
        /// Translates direction words into degreesFromNorth and inclineGrade for pathways, returns the absolute default value
        /// </summary>
        /// <param name="direction">the value to translate</param>
        /// <returns>degrees from north, incline grade</returns>
        public static Coordinate TranslateDirectionToDegrees(MovementDirectionType direction)
        {
            switch (direction)
            {
                case MovementDirectionType.East:
                    return new Coordinate(90, 0);
                case MovementDirectionType.North:
                    return new Coordinate(0, 0);
                case MovementDirectionType.NorthEast:
                    return new Coordinate(45, 0);
                case MovementDirectionType.NorthWest:
                    return new Coordinate(315, 0);
                case MovementDirectionType.South:
                    return new Coordinate(180, 0);
                case MovementDirectionType.SouthEast:
                    return new Coordinate(135, 0);
                case MovementDirectionType.SouthWest:
                    return new Coordinate(225, 0);
                case MovementDirectionType.West:
                    return new Coordinate(270, 0);
            }

            //return none, neutral for anything not counted
            return new Coordinate(-1, 0);
        }

        public static MovementDirectionType ReverseDirection(MovementDirectionType direction)
        {
            switch (direction)
            {
                case MovementDirectionType.East:
                    return MovementDirectionType.West;
                case MovementDirectionType.North:
                    return MovementDirectionType.South;
                case MovementDirectionType.NorthEast:
                    return MovementDirectionType.SouthWest;
                case MovementDirectionType.NorthWest:
                    return MovementDirectionType.SouthEast;
                case MovementDirectionType.South:
                    return MovementDirectionType.North;
                case MovementDirectionType.SouthEast:
                    return MovementDirectionType.NorthWest;
                case MovementDirectionType.SouthWest:
                    return MovementDirectionType.NorthEast;
                case MovementDirectionType.West:
                    return MovementDirectionType.East;
            }

            //return none, neutral for anything not counted
            return MovementDirectionType.None;
        }

        /// <summary>
        /// Translates hard directions to ascii characters. UP inclines are always brackets open to the left, DOWN is always bracket open to the right
        /// </summary>
        /// <param name="direction">the hard direction to turn into a character</param>
        /// <returns>a single ascii character in a string</returns>
        public static string TranslateDirectionToAsciiCharacter(MovementDirectionType direction)
        {
            switch (direction)
            {
                default:
                    return "#";
                case MovementDirectionType.West:
                case MovementDirectionType.East:
                    return "-";
                case MovementDirectionType.South:
                case MovementDirectionType.North:
                    return "|";
                case MovementDirectionType.SouthWest:
                case MovementDirectionType.NorthEast:
                    return "/";
                case MovementDirectionType.SouthEast:
                case MovementDirectionType.NorthWest:
                    return @"\";
            }
        }

        /// <summary>
        /// X, Y, Z
        /// </summary>
        /// <param name="transversalDirection">The direction being faced</param>
        /// <returns>the coordinates for the direction needed to move one unit "forward"</returns>
        public static Coordinate GetDirectionStep(MovementDirectionType transversalDirection)
        {
            switch (transversalDirection)
            {
                default: //We already defaulted to 0,0,0
                    break;
                case MovementDirectionType.East:
                    return new Coordinate(1, 0);
                case MovementDirectionType.North:
                    return new Coordinate(0, 1);
                case MovementDirectionType.NorthEast:
                    return new Coordinate(1, 1);
                case MovementDirectionType.NorthWest:
                    return new Coordinate(-1, 1);
                case MovementDirectionType.South:
                    return new Coordinate(0, -1);
                case MovementDirectionType.SouthEast:
                    return new Coordinate(1, -1);
                case MovementDirectionType.SouthWest:
                    return new Coordinate(-1, -1);
                case MovementDirectionType.West:
                    return new Coordinate(-1, 0);
            }

            return new Coordinate(0, 0);
        }

        /// <summary>
        /// Get something's direction from yourself
        /// </summary>
        /// <param name="here">you</param>
        /// <param name="there">them</param>
        /// <returns>A direction, or None if it's the same coordinate</returns>
        public static MovementDirectionType DirectionFrom(Coordinate here, Coordinate there)
        {
            bool isNorth = false;
            bool isEast = false;
            bool isWest = false;
            bool isSouth = false;

            //East/west
            if (there.X > here.X)
                isEast = true;
            else if (there.X < here.X)
                isWest = true;

            //North/South
            if (there.Y > here.Y)
                isNorth = true;
            else if (there.Y < here.Y)
                isSouth = true;

            if(isNorth)
            {
                if (isEast)
                    return MovementDirectionType.NorthEast;
                else if (isWest)
                    return MovementDirectionType.NorthWest;

                return MovementDirectionType.North;
            }

            if (isSouth)
            {
                if (isEast)
                    return MovementDirectionType.SouthEast;
                else if (isWest)
                    return MovementDirectionType.SouthWest;

                return MovementDirectionType.South;
            }

            if (isEast)
                return MovementDirectionType.East;
            if (isWest)
                return MovementDirectionType.West;

            return MovementDirectionType.None;
        }

        /// <summary>
        /// Rotates the current deltas. Assumes North is the initial direction
        /// </summary>
        /// <param name="currentDeltas">the current coordinate deltas</param>
        /// <param name="direction">where we're rotating to</param>
        /// <returns>the new deltas</returns>
        public static Coordinate RotateCoordinateDeltas(Coordinate currentDeltas, MovementDirectionType direction)
        {
            var x = currentDeltas.X;
            var y = currentDeltas.Y;

            switch(direction)
            {
                case MovementDirectionType.South:
                    return new Coordinate(x, -1 * y);

                case MovementDirectionType.East:
                    return new Coordinate(y, x);

                case MovementDirectionType.West:
                    return new Coordinate(-1 * y, x * -1);

                case MovementDirectionType.NorthEast:
                    return new Coordinate(x + y, y - x.TowardsZero());

                case MovementDirectionType.NorthWest:
                    return new Coordinate(x - y, y + x.TowardsZero());

                case MovementDirectionType.SouthEast:
                    return new Coordinate(x + y.TowardsZero(), -1 * y - x.TowardsZero());

                case MovementDirectionType.SouthWest:
                    return new Coordinate(x - y.TowardsZero(), -1 * y + x.TowardsZero());
            }

            return currentDeltas;
        }
    }
}
