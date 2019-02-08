using NetMud.Cartography;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Zone;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Locale
{
    /// <summary>
    /// Backing data for locales
    /// </summary>
    public class LocaleTemplate : EntityTemplatePartial, ILocaleTemplate
    {
        [JsonIgnore]
        [ScriptIgnore]
        public override Type EntityClass
        {
            get { return typeof(Locale); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

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
                {
                    _keywords = new string[] { Name.ToLower() };
                }

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// Is this zone discoverable?
        /// </summary>
        [Display(Name = "Always Discovered", Description = "Is this locale automatically known to players?")]
        [UIHint("Boolean")]
        public bool AlwaysDiscovered { get; set; }

        /// <summary>
        /// When this locale dies off
        /// </summary>
        public DateTime RollingExpiration { get; set; }

        /// <summary>
        /// The interior map of the locale
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IMap Interior { get; set; }

        [JsonProperty("ParentLocation")]
        private TemplateCacheKey _parentLocation { get; set; }

        /// <summary>
        /// The zone this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Locales must have a zone affiliation.")]
        public IZoneTemplate ParentLocation
        {
            get
            {
                return TemplateCache.Get<IZoneTemplate>(_parentLocation);
            }
            set
            {
                if (value != null)
                {
                    _parentLocation = new TemplateCacheKey(value);
                }
            }
        }

        public LocaleTemplate()
        {
            Descriptives = new HashSet<ISensoryEvent>();
        }

        /// <summary>
        ///List of rooms
        /// </summary>
        public IEnumerable<IRoomTemplate> Rooms()
        {
            return TemplateCache.GetAll<IRoomTemplate>().Where(room => room.ParentLocation.Equals(this));
        }

        /// <summary>
        /// Regenerate the internal map for the locale; try not to do this often
        /// </summary>
        public void RemapInterior()
        {
            IEnumerable<IRoomTemplate> remainingRooms = Rooms();
            long[,,] returnMap = Cartographer.GenerateMapFromRoom(CentralRoom(), new HashSet<IRoomTemplate>(remainingRooms), true);

            Interior = new Map(returnMap, false);
        }

        /// <summary>
        /// Get the central room for a Z plane
        /// </summary>
        /// <param name="zIndex">The Z plane to get the central room of</param>
        /// <returns>The room that is in the center of the Z plane</returns>
        public IRoomTemplate CentralRoom(int zIndex = -1)
        {
            IEnumerable<IRoomTemplate> roomsPlane = Rooms().Where(room => zIndex == -1 || (room.Coordinates != null && room.Coordinates.Z == zIndex));

            //TODO 
            return roomsPlane.FirstOrDefault();
        }

        /// <summary>
        /// Mean diameter of this locale by room dimensions
        /// </summary>
        /// <returns>H,W,D</returns>
        public Dimensions Diameter()
        {
            return new Dimensions(0, 0, 0);
        }

        /// <summary>
        /// Dimensional size at max for h,w,d
        /// </summary>
        /// <returns>H,W,D</returns>
        public Dimensions FullDimensions()
        {
            return new Dimensions(0, 0, 0);
        }

        /// <summary>
        /// Get the dimensions of this model, passthru to FullDimensions
        /// </summary>
        /// <returns></returns>
        public override Dimensions GetModelDimensions()
        {
            return FullDimensions();
        }

        /// <summary>
        /// Render the interior map
        /// </summary>
        /// <param name="zIndex">Z Plane this is getting a map for</param>
        /// <param name="forAdmin">ignore visibility</param>
        /// <returns>The flattened map</returns>
        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            return Rendering.RenderRadiusMap(this, 10, zIndex, forAdmin).Item2;
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public ILocale GetLiveInstance()
        {
            return LiveCache.Get<ILocale>(Id);
        }
    }
}
