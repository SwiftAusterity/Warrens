using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Cartography
{
    /// <summary>
    /// Methods for handling with the coordinate maps
    /// </summary>
    public static class Cartographer
    {
        /// <summary>
        /// Gets a room in a direction if there is one based on the world map the room belongs to
        /// </summary>
        /// <param name="origin">The room we're starting in</param>
        /// <param name="direction">The direction we're moving in</param>
        /// <returns>null or a RoomData</returns>
        public static IRoomData GetRoomInDirection(IRoomData origin, MovementDirectionType direction)
        {
            //We can't find none directions on a map
            if (origin == null || direction == MovementDirectionType.None)
                return null;

            var steps = Utilities.GetDirectionStep(direction);
            var newX = origin.Coordinates.Item1 + steps.Item1;
            var newY = origin.Coordinates.Item2 + steps.Item2;
            var newZ = origin.Coordinates.Item3 + steps.Item3;

            //out of bounds
           // if (Utilities.IsOutOfBounds(new Tuple<int,int,int>(newX, newY, newZ), worldMap))
           //     return null;

            //if (worldMap[newX, newY, newZ] > -1)
             //   return BackingDataCache.Get<IRoomData>(worldMap[newX, newY, newZ]);

            return null;
        }

        /// <summary>
        /// Render a 3d map down to 2d
        /// </summary>
        /// <param name="zIndex"></param>
        /// <returns>flattened map</returns>
        public static long[,] GetSinglePlane(long[, ,] fullMap, int zIndex)
        {
            if (zIndex > fullMap.GetUpperBound(2) || zIndex < 0)
                throw new InvalidOperationException("Requested zIndex greater than upper Z bound of map.");

            var flatMap = new long[fullMap.GetUpperBound(0) + 1, fullMap.GetUpperBound(1) + 1];

            int x, y;
            for (x = 0; x <= fullMap.GetUpperBound(0); x++)
                for (y = 0; y <= fullMap.GetUpperBound(1); y++)
                    flatMap[x, y] = fullMap[x, y, zIndex];

            return flatMap;
        }

        /// <summary>
        /// Isolates the main world map to just one zone
        /// </summary>
        /// <param name="fullMap">the world map</param>
        /// <param name="zoneId">the zone to isolate</param>
        /// <param name="recenter">recenter the map or not, defaults to not</param>
        /// <returns>the zone's map</returns>
        public static long[, ,] GetZoneMap(long[, ,] fullMap, long zoneId, bool recenter = false)
        {
            var newMap = new long[fullMap.GetUpperBound(0) + 1, fullMap.GetUpperBound(1) + 1, fullMap.GetUpperBound(2) + 1];

            int x, y, z, xLowest = 0, yLowest = 0, zLowest = 0;

            for (x = 0; x <= fullMap.GetUpperBound(0); x++)
                for (y = 0; y <= fullMap.GetUpperBound(1); y++)
                    for (z = 0; z <= fullMap.GetUpperBound(2); z++)
                    {
                        var room = BackingDataCache.Get<IRoomData>(fullMap[x, y, z]);

                        if (room == null)
                            continue;

                        newMap[x, y, z] = fullMap[x, y, z];

                        if (xLowest > x)
                            xLowest = x;

                        if (yLowest > y)
                            yLowest = y;

                        if (zLowest > z)
                            zLowest = z;
                    }

            //Maps were the same size or we didnt want to shrink
            if (!false || (xLowest <= 0 && yLowest <= 0 && zLowest <= 0))
                return newMap;

            return ShrinkMap(newMap, xLowest, yLowest, zLowest
                , new Tuple<int, int>(newMap.GetLowerBound(0), newMap.GetUpperBound(0))
                , new Tuple<int, int>(newMap.GetLowerBound(1), newMap.GetUpperBound(1))
                , new Tuple<int, int>(newMap.GetLowerBound(2), newMap.GetUpperBound(2)));
        }

        /// <summary>
        /// Generate a room map starting in a room backing data with a radius around it
        /// </summary>
        /// <param name="room">the starting room</param>
        /// <param name="radius">the radius of rooms to go out to. -1 means "generate the entire world"</param>
        /// <param name="recenter">find the center node of the array and return an array with that node at absolute center</param>
        /// <returns>a 3d array of rooms</returns>
        public static long[, ,] GenerateMapFromRoom(IRoomData room, int radius, HashSet<IRoomData> roomPool, bool shrink = false)
        {
            if (room == null || radius < 0)
                throw new InvalidOperationException("Invalid inputs.");

            var diameter = radius * 2;
            var center = radius;

            //+1 for center room
            diameter++;
            center++;

            var returnMap = new long[diameter, diameter, diameter];


            //The origin room
            returnMap = AddFullRoomToMap(returnMap, room, diameter, center, center, center, roomPool);

            if (shrink)
                returnMap = ShrinkMap(returnMap);

            return returnMap;
        }

        /// <summary>
        /// Shrinks a map matrix to its exact needed coordinate bounds
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static long[, ,] ShrinkMap(long[, ,] map)
        {
            //We take a "full slice" of the map to shrink it
            return Cartographer.TakeSliceOfMap(new Tuple<int, int>(map.GetLowerBound(0), map.GetUpperBound(0))
                                                    , new Tuple<int, int>(map.GetLowerBound(1), map.GetUpperBound(1))
                                                    , new Tuple<int, int>(map.GetLowerBound(2), map.GetUpperBound(2))
                                                    , map, true);
        }

        /// <summary>
        /// Gives back the original map but with all rooms that fall outside of the indicated bounds removed
        /// </summary>
        /// <param name="xBounds">The upper and lower bounds to grab for X axis</param>
        /// <param name="yBounds">The upper and lower bounds to grab for Y axis</param>
        /// <param name="zBounds">The upper and lower bounds to grab for Z axis</param>
        /// <param name="map">The map to take from</param>
        /// <param name="shrink">Return a new array that is bound to the size of the remaining data</param>
        /// <returns>the new sliced array</returns>
        public static long[, ,] TakeSliceOfMap(Tuple<int, int> xBounds, Tuple<int, int> yBounds, Tuple<int, int> zBounds, long[, ,] map, bool shrink = false)
        {
            var newMap = new long[map.GetUpperBound(0) + 1, map.GetUpperBound(1) + 1, map.GetUpperBound(2) + 1];

            int x, y, z, xLowest = -1, yLowest = -1, zLowest = -1;

            for (x = 0; x <= map.GetUpperBound(0); x++)
                if (x <= xBounds.Item2 && x >= xBounds.Item1)
                    for (y = 0; y <= map.GetUpperBound(1); y++)
                        if (y <= yBounds.Item2 && y >= yBounds.Item1)
                            for (z = 0; z <= map.GetUpperBound(2); z++)
                                if (z <= zBounds.Item2 && z >= zBounds.Item1 && map[x, y, z] > 0)
                                {
                                    newMap[x, y, z] = map[x, y, z];

                                    if (xLowest == -1 || xLowest > x)
                                        xLowest = x;

                                    if (yLowest == -1 || yLowest > y)
                                        yLowest = y;

                                    if (zLowest == -1 || zLowest > z)
                                        zLowest = z;
                                }

            //Maps were the same size or we didnt want to shrink
            if (!shrink || (xLowest <= 0 && yLowest <= 0 && zLowest <= 0))
                return newMap;

            return ShrinkMap(newMap, xLowest, yLowest, zLowest, xBounds, yBounds, zBounds);
        }

        /// <summary>
        /// Finds the central room of a given map
        /// </summary>
        /// <param name="map">the map x,y,z</param>
        /// <param name="zIndex">If > -1 we're looking for the x,y center of the single plane as opposed to the actual x,y,z center of the whole map</param>
        /// <returns>the central room</returns>
        public static IRoomData FindCenterOfMap(long[, ,] map, int zIndex = -1)
        {
            var zCenter = zIndex;
            long roomId = -1;

            //If we want a specific z index thats fine, otherwise we find the middle Z
            if (zIndex == -1)
                zCenter = (map.GetUpperBound(2) - map.GetLowerBound(2)) / 2 + map.GetLowerBound(2);

            var xCenter = (map.GetUpperBound(0) - map.GetLowerBound(0)) / 2 + map.GetLowerBound(0);
            var yCenter = (map.GetUpperBound(1) - map.GetLowerBound(1)) / 2 + map.GetLowerBound(1);

            roomId = map[xCenter, yCenter, zCenter];

            if (roomId < 0)
            {
                for (var variance = 1;
                variance <= xCenter - map.GetLowerBound(0) && variance <= map.GetUpperBound(0) - xCenter
                && variance <= yCenter - map.GetLowerBound(1) && variance <= map.GetUpperBound(1) - yCenter
                ; variance++)
                {
                    //Check around it
                    if (map[xCenter - variance, yCenter, zCenter] >= 0)
                        roomId = map[xCenter - variance, yCenter, zCenter];
                    else if (map[xCenter + variance, yCenter, zCenter] >= 0)
                        roomId = map[xCenter + variance, yCenter, zCenter];
                    else if (map[xCenter, yCenter - variance, zCenter] >= 0)
                        roomId = map[xCenter, yCenter - variance, zCenter];
                    else if (map[xCenter, yCenter + variance, zCenter] >= 0)
                        roomId = map[xCenter, yCenter + variance, zCenter];
                    else if (map[xCenter - variance, yCenter - variance, zCenter] >= 0)
                        roomId = map[xCenter - variance, yCenter - variance, zCenter];
                    else if (map[xCenter - variance, yCenter + variance, zCenter] >= 0)
                        roomId = map[xCenter - variance, yCenter + variance, zCenter];
                    else if (map[xCenter + variance, yCenter - variance, zCenter] >= 0)
                        roomId = map[xCenter + variance, yCenter - variance, zCenter];
                    else if (map[xCenter + variance, yCenter + variance, zCenter] >= 0)
                        roomId = map[xCenter + variance, yCenter + variance, zCenter];

                    if (roomId >= 0)
                        break;
                }
            }

            //Well, no valid rooms on this Z so try another Z unless all we got was this one Z
            if(roomId < 0 && zIndex == -1)
            {
                IRoomData returnRoom = null;

                for (var variance = 1;
                variance < zCenter - map.GetLowerBound(2) && variance < map.GetUpperBound(2) - zCenter
                ; variance++)
                {
                    returnRoom = FindCenterOfMap(map, zCenter - variance);

                    if (returnRoom != null)
                        break;

                    returnRoom = FindCenterOfMap(map, zCenter + variance);

                    if (returnRoom != null)
                        break;
                }

                return returnRoom;
            }

            return BackingDataCache.Get<IRoomData>(roomId);
        }

        //It's just easier to pass the ints we already calculated along instead of doing the math every single time, this cascades each direction fully because it calls itself for existant rooms
        private static long[, ,] AddFullRoomToMap(long[, ,] dataMap, IRoomData origin, int diameter, int centerX, int centerY, int centerZ, HashSet<IRoomData> roomPool)
        {
            if (roomPool != null && roomPool.Count > 0 && roomPool.Contains(origin))
                roomPool.Remove(origin);

            //Render the room itself
            dataMap[centerX - 1, centerY - 1, centerZ - 1] = origin.ID;
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.North, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.NorthEast, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.NorthWest, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.East, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.West, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.South, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.SouthEast, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.SouthWest, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.Up, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.UpNorth, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.UpNorthEast, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.UpNorthWest, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.UpEast, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.UpWest, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.UpSouth, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.UpSouthEast, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.UpSouthWest, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.Down, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.DownNorth, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.DownNorthEast, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.DownNorthWest, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.DownEast, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.DownWest, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.DownSouth, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.DownSouthEast, origin, diameter, centerX, centerY, centerZ, roomPool);
            dataMap = AddDirectionToMap(dataMap, MovementDirectionType.DownSouthWest, origin, diameter, centerX, centerY, centerZ, roomPool);

            return dataMap;
        }

        //We have to render our pathway out, an empty space for the potential pathway back and the destination room
        private static long[, ,] AddDirectionToMap(long[, ,] dataMap, MovementDirectionType transversalDirection, IRoomData origin, int diameter, int centerX, int centerY, int centerZ, HashSet<IRoomData> roomPool)
        {
            var pathways = origin.GetPathways(true);
            var directionalSteps = Utilities.GetDirectionStep(transversalDirection);

            var xStepped = centerX + directionalSteps.Item1;
            var yStepped = centerY + directionalSteps.Item2;
            var zStepped = centerZ + directionalSteps.Item3;

            //If we're not over diameter budget and there is nothing there already (we might have already rendered the path and room) then render it
            //When the next room tries to render backwards it'll run into the existant path it came from and stop the chain here
            if (xStepped < diameter && xStepped > 0
                && yStepped > 0 && yStepped < diameter
                && zStepped > 0 && zStepped < diameter
                && dataMap[xStepped - 1, yStepped - 1, zStepped - 1] <= 0)
            {
                var thisPath = pathways.FirstOrDefault(path =>
                                                        (path.DirectionType == transversalDirection && path.FromLocationID.Equals(origin.ID.ToString()))
                                                        || (path.DirectionType == Utilities.ReverseDirection(transversalDirection) && path.ToLocationID.Equals(origin.ID.ToString()))
                                                        );
                if (thisPath != null)
                {
                    var locId = long.Parse(thisPath.ToLocationID);
                    if (thisPath.ToLocationID.Equals(origin.ID.ToString()))
                        locId = long.Parse(thisPath.FromLocationID);

                    var passdownOrigin = BackingDataCache.Get<IRoomData>(locId);

                    if (passdownOrigin != null)
                    {
                        dataMap[xStepped - 1, yStepped - 1, zStepped - 1] = passdownOrigin.ID;
                        dataMap = AddFullRoomToMap(dataMap, passdownOrigin, diameter, xStepped, yStepped, zStepped, roomPool);
                    }
                }
            }

            return dataMap;
        }

        private static long[, ,] ShrinkMap(long[, ,] fullMap, int xLowest, int yLowest, int zLowest, Tuple<int, int> xBounds, Tuple<int, int> yBounds, Tuple<int, int> zBounds)
        {
            //Maps were the same size
            if (xLowest <= 0 && yLowest <= 0 && zLowest <= 0)
                return fullMap;

            var shrunkMap = new long[fullMap.GetUpperBound(0) - xLowest, fullMap.GetUpperBound(1) - yLowest, fullMap.GetUpperBound(2) - zLowest];

            int x, y, z;
            for (x = 0; x <= shrunkMap.GetUpperBound(0); x++)
                if (x <= xBounds.Item2 && x >= xBounds.Item1)
                    for (y = 0; y <= shrunkMap.GetUpperBound(1); y++)
                        if (y <= yBounds.Item2 && y >= yBounds.Item1)
                            for (z = 0; z <= shrunkMap.GetUpperBound(2); z++)
                                shrunkMap[x, y, z] = fullMap[x + xLowest, y + yLowest, z + zLowest];

            return shrunkMap;
        }
    }
}
