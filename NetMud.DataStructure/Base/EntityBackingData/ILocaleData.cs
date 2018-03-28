using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backingdata for permenant locales
    /// </summary>
    public interface ILocaleData : IEntityBackingData, ISingleton
    {
        /// <summary>
        /// The rooms contained within the locale should it need to regenerate from nothing
        /// </summary>
        HashSet<IRoomData> Rooms { get; set; }

        /// <summary>
        /// Does this even need to be discovered?
        /// </summary>
        bool AlwaysVisible { get; set; }

        /// <summary>
        /// The map of the rooms inside
        /// </summary>
        IMap Interior { get; set; }

        /// <summary>
        /// Get the absolute center room of the locale
        /// </summary>
        /// <returns>the central room of the locale</returns>
        IRoomData CentralRoom(int zIndex = -1);

        /// <summary>
        /// Get the basic map render for the zone
        /// </summary>
        /// <returns>the zone map in ascii</returns>
        string RenderMap(int zIndex, bool forAdmin = false);

        /// <summary>
        /// The diameter of the locale
        /// </summary>
        /// <returns>the diameter of the locale in room count</returns>
        Tuple<int, int, int> Diameter();

        /// <summary>
        /// Calculate the theoretical dimensions of the locale in inches
        /// </summary>
        /// <returns>the dimensions of the locale in inches</returns>
        Tuple<int, int, int> FullDimensions();
    }
}
