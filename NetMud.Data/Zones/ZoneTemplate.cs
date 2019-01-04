using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Players;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Zones
{

    public class ZoneTemplate : EntityTemplatePartial, IZoneTemplate
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Zone); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } }

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override string[] Keywords
        {
            get
            {
                if (_keywords == null || _keywords.Length == 0)
                    _keywords = new string[] { Name.ToLower() };

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// What hemisphere this zone is in
        /// </summary>
        [Display(Name = "Hemisphere", Description = "The hemisphere of the world this zone is in.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public HemispherePlacement Hemisphere { get; set; }

        /// <summary>
        /// Temperature variance for generating locales
        /// </summary>
        [Display(Name = "Temperature Coefficient", Description = "Determines how chaotic the weather systems are over this zone.")]
        [DataType(DataType.Text)]
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// Barometric variance for generating locales
        /// </summary>
        [Display(Name = "Barometric Coefficient", Description = "Determines how chaotic the weather systems are over this zone.")]
        [DataType(DataType.Text)]
        public int PressureCoefficient { get; set; }

        /// <summary>
        /// The font the zone map displays with
        /// </summary>
        [Display(Name = "Font", Description = "The font the zone map displays with.")]
        [DataType(DataType.Text)]
        public string Font { get; set; }

        /// <summary>
        /// The background color for the display
        /// </summary>
        [Display(Name = "Background Color", Description = "The hex code of the color of the background of the zone.")]
        [DataType(DataType.Text)]
        [UIHint("ColorPicker")]
        [Required]
        public string BackgroundHexColor { get; set; }

        [JsonProperty("BaseTileType")]
        private TemplateCacheKey _baseTileType { get; set; }

        /// <summary>
        /// The default tile the map is filled with
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Base Tile", Description = "The default tile the map fills with.")]
        [UIHint("TileTemplateList")]
        [TileTemplateDataBinder]
        [Required]
        public ITileTemplate BaseTileType
        {
            get
            {
                return TemplateCache.Get<ITileTemplate>(_baseTileType);
            }
            set
            {
                if (value != null)
                    _baseTileType = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// What the natural biome is for generating locales
        /// </summary>
        [Display(Name = "Biome", Description = "The biome of the zone.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public Biome BaseBiome { get; set; }

        /// <summary>
        /// The entrance coordinates if someone ends up in this zone without entrance coordinates
        /// </summary>
        [Display(Name = "Base Coordinates", Description = "The default X,Y coordinates characters enter the map into.")]
        [UIHint("Coordinate")]
        [Required]
        public Coordinate BaseCoordinates { get; set; }


        [JsonProperty("World")]
        private TemplateCacheKey _world { get; set; }

        /// <summary>
        /// What world does this belong to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "World", Description = "The World/Dimension this belongs to.")]
        [UIHint("GaiaTemplateList")]
        [GaiaTemplateDataBinder]
        [Required]
        public IGaiaTemplate World
        {
            get
            {
                if (_world == null)
                    return null;

                return TemplateCache.Get<IGaiaTemplate>(_world);
            }
            set
            {
                if (value != null)
                    _world = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Cooperative")]
        private HashSet<string> _cooperative { get; set; }

        /// <summary>
        /// Other players who are allowed to modify this zone map
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<IAccount> Cooperative
        {
            get
            {
                if (_cooperative == null)
                    _cooperative = new HashSet<string>();

                return new HashSet<IAccount>(_cooperative.Select(coop => Account.GetByHandle(coop)));
            }
            set
            {
                _cooperative = new HashSet<string>(value.Select(coop => coop.GlobalIdentityHandle));
            }
        }

        /// <summary>
        /// Paths out of this zone
        /// </summary>
        public HashSet<IPathway> Pathways { get; set; }

        /// <summary>
        /// The room plane
        /// </summary>
        [UIHint("Map")]
        [CoordinateTileMapDataBinder]
        public IZoneTemplateMap Map { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public ZoneTemplate()
        {
            Map = new ZoneTemplateMap();
        }

        /// <summary>
        /// Get one of the tiles in the tile map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ITileTemplate GetTile(Coordinate coordinates)
        {
            if (Map == null)
            {
                Map = new ZoneTemplateMap();
            }

            return TemplateCache.Get<ITileTemplate>(Map.CoordinateTilePlane[coordinates.X, coordinates.Y]);
        }

        /// <summary>
        /// Get one of the pathways in the tile map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public IPathway GetPathway(Coordinate coordinates)
        {
            return Pathways.FirstOrDefault(path => path.OriginCoordinates.X == coordinates.X && path.OriginCoordinates.Y == coordinates.Y);
        }


        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IZone GetLiveInstance()
        {
            return LiveCache.Get<IZone>(Id);
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Temperature", TemperatureCoefficient.ToString());
            returnList.Add("Pressure", PressureCoefficient.ToString());
            returnList.Add("Biome", BaseBiome.ToString());
            returnList.Add("Hemisphere", Hemisphere.ToString());

            returnList.Add("Font", Font);
            returnList.Add("BackgroundHexColor", BackgroundHexColor);
            returnList.Add("Base Tile Type", BaseTileType.Name);

            returnList.Add("Base Coordinates", string.Format("({0},{1})", BaseCoordinates.X, BaseCoordinates.Y));

            return returnList;
        }

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public override bool PersistToCache()
        {
            try
            {
                TemplateCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            HashSet<IDecayEvent> decayEvents = new HashSet<IDecayEvent>();
            foreach (IDecayEvent decayEvent in DecayEvents)
                decayEvents.Add((IDecayEvent)decayEvent.Clone());

            HashSet<IInteraction> interactions = new HashSet<IInteraction>();
            foreach (IInteraction interaction in Interactions)
                interactions.Add((IInteraction)interaction.Clone());

            HashSet<IPathway> pathways = new HashSet<IPathway>();
            foreach (IPathway pathway in Pathways)
                pathways.Add((IPathway)pathway.Clone());

            return new ZoneTemplate
            {
                Name = Name,
                AsciiCharacter = AsciiCharacter,
                Description = Description,
                HexColorCode = HexColorCode,
                Qualities = Qualities,
                Interactions = Interactions,
                DecayEvents = decayEvents,
                BackgroundHexColor = BackgroundHexColor,
                BaseBiome = BaseBiome,
                BaseTileType = BaseTileType,
                BaseCoordinates = BaseCoordinates,
                Font = Font,
                Hemisphere = Hemisphere,
                PressureCoefficient = PressureCoefficient,
                TemperatureCoefficient = TemperatureCoefficient,
                World = World,
                Map = Map,
                Pathways = pathways
            };
        }
    }
}
