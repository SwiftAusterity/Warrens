using System;

namespace NetMud.Cartography
{
    /// <summary>
    /// Methods for handling with the coordinate maps
    /// </summary>
    public static class Cartographer
    {
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
