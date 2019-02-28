namespace NetMud.Models
{
    /// <summary>
    /// Partial view model for rendering ascii maps
    /// </summary>
    public class LiveAsciiMapViewModel
    {
        /// <summary>
        /// The render type
        /// RenderRoomForEditWithRadius, RenderWorldMap, RenderZoneMap
        /// </summary>
        public string MapRenderType { get; set; }

        /// <summary>
        /// The Birthmark of the thing we're rendering
        /// </summary>
        public string BirthMark { get; set; }

        /// <summary>
        /// The zindex we're rendering
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Radius we're rendering, only relevant to rooms right now
        /// </summary>
        public int Radius { get; set; }

        public LiveAsciiMapViewModel(string mapRenderType, string birthMark, int zIndex, int radius = -1)
        {
            MapRenderType = mapRenderType;
            BirthMark = birthMark;
            ZIndex = zIndex;
            Radius = radius;
        }
    }
}