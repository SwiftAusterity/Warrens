using NetMud.Data.Game;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;

namespace NetMud.Data.EntityBackingData
{
    public class ZoneData : EntityBackingDataPartial, IZoneData
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(Zone); }
        }

        /// <summary>
        /// Base elevation used in generating locales
        /// </summary>
        public int BaseElevation { get; set; }

        /// <summary>
        /// Temperature variance for generating locales
        /// </summary>
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// Barometric variance for generating locales
        /// </summary>
        public int PressureCoefficient { get; set; }

        /// <summary>
        /// What the natural biome is for generating locales
        /// </summary>
        public Biome BaseBiome { get; set; }

        /// <summary>
        /// List of templates the zone can use for Adventure generation
        /// </summary>
        public HashSet<IAdventureTemplate> Templates { get; set; }

        /// <summary>
        /// List of perm locales
        /// </summary>
        public HashSet<ILocaleData> Locales { get; set; }

        /// <summary>
        /// Exits to other zones and their discoverability
        /// </summary>
        public Tuple<IZoneData, bool> ZoneExits { get; set; }

        /// <summary>
        /// What resources spawn here
        /// </summary>
        public Dictionary<INaturalResource, int> NaturalResourceSpawn { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public ZoneData()
        {
            Templates = new HashSet<IAdventureTemplate>();
            Locales = new HashSet<ILocaleData>();
            NaturalResourceSpawn = new Dictionary<INaturalResource, int>();            
        }

        /// <summary>
        /// Get the total rough dimensions of the zone
        /// </summary>
        /// <returns></returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            //TODO
            return new Tuple<int, int, int>(1, 1, 1);
        }
    }
}
