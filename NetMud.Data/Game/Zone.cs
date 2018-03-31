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

        ///// <summary>
        ///// Getall the rooms for the zone
        ///// </summary>
        ///// <returns>the rooms for the zone</returns>
        //public IEnumerable<IRoomData> Rooms()
        //{
        //    return BackingDataCache.GetAll<IRoomData>().Where(room => room.ZoneAffiliation.Equals(this));
        //}

        ///// <summary>
        ///// Get the absolute center room of the zone
        ///// </summary>
        ///// <returns>the central room of the zone</returns>
        //public IRoomData CentralRoom(int zIndex = -1)
        //{
        //    return Cartography.Cartographer.FindCenterOfMap(ZoneMap.CoordinatePlane, zIndex);
        //}

        ///// <summary>
        ///// Get the basic map render for the zone
        ///// </summary>
        ///// <returns>the zone map in ascii</returns>
        //public string RenderMap(int zIndex, bool forAdmin = false)
        //{
        //    return Cartography.Rendering.RenderMap(ZoneMap.GetSinglePlane(zIndex), forAdmin, true, CentralRoom(zIndex));
        //}

        ///// <summary>
        ///// The diameter of the zone
        ///// </summary>
        ///// <returns>the diameter of the zone in room count x,y,z</returns>
        //public Tuple<int, int, int> Diameter()
        //{
        //    return new Tuple<int, int, int>(ZoneMap.CoordinatePlane.GetUpperBound(0) - ZoneMap.CoordinatePlane.GetLowerBound(0)
        //                                    , ZoneMap.CoordinatePlane.GetUpperBound(1) - ZoneMap.CoordinatePlane.GetLowerBound(1)
        //                                    , ZoneMap.CoordinatePlane.GetUpperBound(2) - ZoneMap.CoordinatePlane.GetLowerBound(2));
        //}

        ///// <summary>
        ///// Calculate the theoretical dimensions of the zone in inches
        ///// </summary>
        ///// <returns>height, width, depth in inches</returns>
        //public Tuple<int, int, int> FullDimensions()
        //{
        //    int height = -1, width = -1, depth = -1;

        //    return new Tuple<int, int, int>(height, width, depth);
        //}

        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            yield return String.Empty;
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
                Position = me.Position;
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
