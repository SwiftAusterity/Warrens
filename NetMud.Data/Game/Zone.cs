using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using System.Linq;
using NetMud.Data.EntityBackingData;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : LocationEntityPartial, IZone
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        public override string DataTemplateName
        {
            get
            {
                if (DataTemplate<IZoneData>() == null)
                    return String.Empty;

                return DataTemplate<IZoneData>().Name;
            }
        }

        /// <summary>
        /// The name used in the tag for discovery checking
        /// </summary>
        public string DiscoveryName
        {
            get
            {
                return "Zone_" + DataTemplateName;
            }
        }

        /// <summary>
        /// Is this zone ownership malleable
        /// </summary>
        public bool Claimable { get; set; }

        [JsonProperty("Locales")]
        private HashSet<string> _locales { get; set; }

        /// <summary>
        /// Locales in this zone, temporary and perm
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<ILocale> Locales
        {
            get
            {
                if (_locales != null)
                    return new HashSet<ILocale>(LiveCache.GetMany<ILocale>(_locales));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _locales = new HashSet<string>(value.Select(k => k.BirthMark));
            }
        }

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
            Pathways = new EntityContainer<IPathway>();

            Claimable = false;
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Zone(IZoneData zone)
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
            Pathways = new EntityContainer<IPathway>();

            Claimable = false;

            DataTemplateId = zone.ID;

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Generate a new random locale with rooms, put it in the world (and this zone) as temporary
        /// </summary>
        /// <param name="name">The name of the new locale, empty = generate a new one</param>
        /// <returns></returns>
        public ILocale GenerateAdventure(string name = "")
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get all possible surroundings (locales, rooms, etc)
        /// </summary>
        /// <param name="strength">How far to look, IGNORED</param>
        /// <returns>All locations that are visible</returns>
        public override IEnumerable<ILocation> GetSurroundings(int strength)
        {
            //Zone is always 1
            return base.GetSurroundings(1);
        }
        
        /// <summary>
        /// Does this entity know about this thing
        /// </summary>
        /// <param name="discoverer">The onlooker</param>
        /// <returns>If this is known to the discoverer</returns>
        public bool IsDiscovered(IEntity discoverer)
        {
            if (DataTemplate<IZoneData>().AlwaysDiscovered)
                return true;

            //TODO

            //discoverer.HasAccomplishment(DiscoveryName);

            //For now
            return true;
        }

        /// <summary>
        /// List out all the known exits to Zones
        /// </summary>
        /// <param name="viewer">the onlooker</param>
        /// <returns>All zones that have exits from here that are known</returns>
        public IEnumerable<IZone> GetVisibleZoneHorizons(IEntity viewer)
        {
            return Enumerable.Empty<IZone>();

            //return Pathways.EntitiesContained()
            //        .Where(path => path.ToLocation.GetType() == typeof(IZone) && ((IZone)path.ToLocation).IsDiscovered(viewer))
            //        .Select(path => (IZone)path.ToLocation);
        }

        /// <summary>
        /// List out all the known exits to Zones
        /// </summary>
        /// <param name="viewer">the onlooker</param>
        /// <returns>All zones that have exits from here that are known</returns>
        public IEnumerable<ILocale> GetVisibleLocaleHorizons(IEntity viewer)
        {
            return Enumerable.Empty<ILocale>();
        }

        /// <summary>
        /// Render to the look command
        /// </summary>
        /// <param name="actor">Who is looking</param>
        /// <returns>Descriptive text</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>
            {
                string.Format("%O%{0}%O%", DataTemplate<IZoneData>().Name),
                string.Empty.PadLeft(DataTemplate<IZoneData>().Name.Length, '-')
            };

            return sb;
        }

        /// <summary>
        /// Get the h,w,d of this
        /// </summary>
        /// <returns></returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            //TODO
            return new Tuple<int, int, int>(1, 1, 1);
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(this));
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        /// <param name="spawnTo">Where this will go</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            var dataTemplate = DataTemplate<IZoneData>();

            BirthMark = LiveCache.GetUniqueIdentifier(dataTemplate);
            Keywords = new string[] { dataTemplate.Name.ToLower() };
            Birthdate = DateTime.Now;
            CurrentLocation = spawnTo;
        }

        /// <summary>
        /// Get this from the world or make a new one and put it in
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<IZone>(DataTemplateId, typeof(ZoneData));

            //Isn't in the world currently
            if (me == default(IZone))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplateId;
                Contents = me.Contents;
                MobilesInside = me.MobilesInside;
                Pathways = me.Pathways;
                Keywords = me.Keywords;
                CurrentLocation = new GlobalPosition(this);
            }
        }
    }
}
