using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Tile;

namespace NetMud.Cartography
{
    /// <summary>
    /// Methods for handling with the coordinate maps
    /// </summary>
    public static class Cartographer
    {
        /// <summary>
        /// Gives back the original map but with all rooms that fall outside of the indicated bounds removed
        /// </summary>
        /// <param name="xBounds">The upper and lower bounds to grab for X axis</param>
        /// <param name="yBounds">The upper and lower bounds to grab for Y axis</param>
        /// <param name="zBounds">The upper and lower bounds to grab for Z axis</param>
        /// <param name="map">The map to take from</param>
        /// <param name="shrink">Return a new array that is bound to the size of the remaining data</param>
        /// <returns>the new sliced array</returns>
        public static ITile[,] TakeSliceOfMap(ValueRange<int> xBounds, ValueRange<int> yBounds, ITile[,] map, short radiusX, short radiusY)
        {
            ITile[,] newMap = new ITile[radiusX * 2 + 1, radiusY * 2 + 1];

            int xI = 0, yI = 0;
            for (int y = yBounds.Low; y <= yBounds.High; y++, yI++)
            {
                xI = 0;

                for (int x = xBounds.Low; x <= xBounds.High; x++, xI++)
                {
                    newMap[xI, yI] = map[x, y];
                }
            }

            return newMap;
        }

        /// <summary>
        /// Shrinks a map matrix to its exact needed coordinate bounds
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static ITile[,] ShrinkMap(ITile[,] map)
        {
            //We take a "full slice" of the map to shrink it
            return TakeSliceOfMap(new ValueRange<int>(map.GetLowerBound(0), map.GetUpperBound(0))
                                                    , new ValueRange<int>(map.GetLowerBound(1), map.GetUpperBound(1))
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
        public static ITile[,] TakeSliceOfMap(ValueRange<int> xBounds, ValueRange<int> yBounds, ITile[,] map, bool shrink)
        {
            ITile[,] newMap = new ITile[map.GetUpperBound(0) + 1, map.GetUpperBound(1) + 1];

            int x, y, xLowest = -1, yLowest = -1;

            for (x = 0; x <= map.GetUpperBound(0); x++)
                if (x <= xBounds.High && x >= xBounds.Low)
                    for (y = 0; y <= map.GetUpperBound(1); y++)
                        if (y <= yBounds.High && y >= yBounds.Low && map[x, y] != null)
                        {
                            newMap[x, y] = map[x, y];

                            if (xLowest == -1 || xLowest > x)
                                xLowest = x;

                            if (yLowest == -1 || yLowest > y)
                                yLowest = y;
                        }

            //Maps were the same size or we didnt want to shrink
            if (!shrink || (xLowest <= 0 && yLowest <= 0))
                return newMap;

            return ShrinkMap(newMap, xLowest, yLowest, xBounds, yBounds);
        }

        /// <summary>
        /// Finds the central room of a given map
        /// </summary>
        /// <param name="map">the map x,y,z</param>
        /// <param name="zIndex">If > -1 we're looking for the x,y center of the single plane as opposed to the actual x,y,z center of the whole map</param>
        /// <returns>the central room</returns>
        public static ITile FindCenterOfMap(ITile[,] map)
        {
            int xCenter = (map.GetUpperBound(0) - map.GetLowerBound(0)) / 2 + map.GetLowerBound(0);
            int yCenter = (map.GetUpperBound(1) - map.GetLowerBound(1)) / 2 + map.GetLowerBound(1);

            ITile tile = map[xCenter, yCenter];

            if (tile == null)
            {
                for (int variance = 1;
                variance <= xCenter - map.GetLowerBound(0) && variance <= map.GetUpperBound(0) - xCenter
                && variance <= yCenter - map.GetLowerBound(1) && variance <= map.GetUpperBound(1) - yCenter
                ; variance++)
                {
                    //Check around it
                    if (map[xCenter - variance, yCenter] != null)
                        tile = map[xCenter - variance, yCenter];
                    else if (map[xCenter + variance, yCenter] != null)
                        tile = map[xCenter + variance, yCenter];
                    else if (map[xCenter, yCenter - variance] != null)
                        tile = map[xCenter, yCenter - variance];
                    else if (map[xCenter, yCenter + variance] != null)
                        tile = map[xCenter, yCenter + variance];
                    else if (map[xCenter - variance, yCenter - variance] != null)
                        tile = map[xCenter - variance, yCenter - variance];
                    else if (map[xCenter - variance, yCenter + variance] != null)
                        tile = map[xCenter - variance, yCenter + variance];
                    else if (map[xCenter + variance, yCenter - variance] != null)
                        tile = map[xCenter + variance, yCenter - variance];
                    else if (map[xCenter + variance, yCenter + variance] != null)
                        tile = map[xCenter + variance, yCenter + variance];

                    if (tile != null)
                        break;
                }
            }

            return tile;
        }

        private static ITile[,] ShrinkMap(ITile[,] fullMap, long xLowest, long yLowest, ValueRange<int> xBounds, ValueRange<int> yBounds)
        {
            //Maps were the same size
            if (xLowest <= 0 && yLowest <= 0)
                return fullMap;

            ITile[,] shrunkMap = new ITile[fullMap.GetUpperBound(0) + 1 - xLowest, fullMap.GetUpperBound(1) + 1 - yLowest];

            int x, y;
            for (x = 0; x <= shrunkMap.GetUpperBound(0); x++)
                if (x <= xBounds.High && x >= xBounds.Low)
                    for (y = 0; y <= shrunkMap.GetUpperBound(1); y++)
                        if (y <= yBounds.High && y >= yBounds.Low)
                            shrunkMap[x, y] = fullMap[x + xLowest, y + yLowest];

            return shrunkMap;
        }
    }
}
