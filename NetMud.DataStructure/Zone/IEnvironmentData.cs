using NetMud.DataStructure.Tile;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Defines behavior around geological, meterological and biological elements for locations
    /// </summary>
    public interface IEnvironmentData
    {
        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>    
        int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>     
        int PressureCoefficient { get; set; }

        /// <summary>
        /// The biome this is supposed to be
        /// </summary>        
        Biome BaseBiome { get; set; }

        /// <summary>
        /// The font the zone map displays with
        /// </summary>
        string Font { get; set; }

        /// <summary>
        /// The background color for the display
        /// </summary>
        string BackgroundHexColor { get; set; }

        /// <summary>
        /// The default tile the map is filled with
        /// </summary>
        ITileTemplate BaseTileType { get; set; }
    }
}
