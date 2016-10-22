using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetMud.Models
{
    /// <summary>
    /// Partial view model for rendering ascii maps
    /// </summary>
    public class AsciiMapViewModel
    {
        /// <summary>
        /// The render type
        /// RenderRoomForEditWithRadius, RenderWorldMap, RenderZoneMap
        /// </summary>
        public string MapRenderType { get; set; }
        
        /// <summary>
        /// The ID of the thing we're rendering
        /// </summary>
        public long DataID { get; set; }

        /// <summary>
        /// The zindex we're rendering
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Radius we're rendering, only relevant to rooms right now
        /// </summary>
        public int Radius { get; set; }

        public AsciiMapViewModel(string mapRenderType, long dataId, int zIndex, int radius = -1)
        {
            MapRenderType = mapRenderType;
            DataID = dataId;
            ZIndex = zIndex;
            Radius = radius;
        }
    }
}