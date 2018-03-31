using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using NetMud.DataStructure.Base.System;
using NetMud.Data.Game;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : LocationEntityPartial, IZone
    {
        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        public int BaseElevation { get; set; }

        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>
        public int PressureCoefficient { get; set; }
        
        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {
            ID = -1;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;
            Name = "NotImpl";
            WorldName = "Zero";

            BaseElevation = 0;
            TemperatureCoefficient = 0;
            PressureCoefficient = 0;
            Claimable = false;
        }
        
        /// <summary>
        /// Getall the rooms for the zone
        /// </summary>
        /// <returns>the rooms for the zone</returns>
        public IEnumerable<IRoomData> Rooms()
        {
            return BackingDataCache.GetAll<IRoomData>().Where(room => room.ZoneAffiliation.Equals(this));
        }
        
        /// <summary>
        /// Get the basic map render for the zone
        /// </summary>
        /// <returns>the zone map in ascii</returns>
        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            return Cartography.Rendering.RenderMap(ZoneMap.GetSinglePlane(zIndex), forAdmin, true, CentralRoom(zIndex));
        }

        /// <summary>
        /// The diameter of the zone
        /// </summary>
        /// <returns>the diameter of the zone in room count x,y,z</returns>
        public Tuple<int, int, int> Diameter()
        {
            return new Tuple<int, int, int>(ZoneMap.CoordinatePlane.GetUpperBound(0) - ZoneMap.CoordinatePlane.GetLowerBound(0)
                                            , ZoneMap.CoordinatePlane.GetUpperBound(1) - ZoneMap.CoordinatePlane.GetLowerBound(1)
                                            , ZoneMap.CoordinatePlane.GetUpperBound(2) - ZoneMap.CoordinatePlane.GetLowerBound(2));
        }

        /// <summary>
        /// Calculate the theoretical dimensions of the zone in inches
        /// </summary>
        /// <returns>height, width, depth in inches</returns>
        public Tuple<int, int, int> FullDimensions()
        {
            int height = -1, width = -1, depth = -1;

            return new Tuple<int, int, int>(height, width, depth);
        }

        public IEnumerable<string> RenderToLook(IEntity actor)
        {
            yield return String.Empty;
        }
    }
}
