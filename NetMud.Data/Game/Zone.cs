using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using System.Linq;
using NetMud.Data.EntityBackingData;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : LocationEntityPartial, IZone
    {
        /// <summary>
        /// Is this zone discoverable?
        /// </summary>
        public bool AlwaysVisible { get; set; }

        /// <summary>
        /// The name used in the tag for discovery checking
        /// </summary>
        public string DiscoveryName
        {
            get
            {
                return "Zone_" + DataTemplate<IZoneData>().Name;
            }
        }

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
        /// Is this zone ownership malleable
        /// </summary>
        public bool Claimable { get; set; }

        /// <summary>
        /// Locales inside this zone
        /// </summary>
        public HashSet<ILocale> Locales { get; set; }

        /// <summary>
        /// Natural resource counts used to populate locales and allow for gathering
        /// </summary>
        public Dictionary<INaturalResource, int> NaturalResources { get; set; }

        public override IGlobalPosition Position
        {
            get
            {
                    return new GlobalPosition { CurrentLocation = this, CurrentZone = this };
            }
            set
            {
                _currentLocationBirthmark = BirthMark;
                UpsertToLiveWorldCache();
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

        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();

            sb.Add(string.Format("%O%{0}%O%", DataTemplate<IZoneData>().Name));
            sb.Add(string.Empty.PadLeft(DataTemplate<IZoneData>().Name.Length, '-'));

            return sb;
        }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            //TODO
            return new Tuple<int, int, int>(1, 1, 1);
        }

        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition { CurrentLocation = this, CurrentZone = this });
        }

        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            var dataTemplate = DataTemplate<IZoneData>();

            BirthMark = LiveCache.GetUniqueIdentifier(dataTemplate);
            Keywords = new string[] { dataTemplate.Name.ToLower() };
            Birthdate = DateTime.Now;
            Position = spawnTo;
        }

        public ILocale GenerateAdventure(string name = "")
        {
            //TODO
            throw new NotImplementedException();
        }

        public override IEnumerable<ILocation> GetSurroundings(int strength)
        {
            //Zone is always 1
            return base.GetSurroundings(1);
        }
        
        public bool IsDiscovered(IEntity discoverer)
        {
            //TODO

            //discoverer.HasAccomplishment(DiscoveryName);

            //For now
            return true;
        }

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
                Position = new GlobalPosition { CurrentLocation = this, CurrentZone = this };
            }
        }

        public IEnumerable<IZone> ZoneExits(IEntity viewer)
        {
            return Pathways.EntitiesContained()
                    .Where(path => path.ToLocation.GetType() == typeof(IZone) && ((IZone)path.ToLocation).IsDiscovered(viewer))
                    .Select(path => (IZone)path.ToLocation);
        }
    }
}
