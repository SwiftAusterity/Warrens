using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : EntityPartial, IZone
    {
        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        public int BaseElevation { get; set; }

        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>
        public int PressureCoefficient { get; set; }

        /// <summary>
        /// Is this zone ownership malleable
        /// </summary>
        public bool Claimable { get; set; }

        public HashSet<ILocale> Locales { get; set; }
        public IEntityContainer<IMobile> MobilesInside { get; set; }
        public IEntityContainer<IInanimate> Contents { get; set; }
        public IEntityContainer<IPathway> Pathways { get; set; }
        public int Humidity { get; set; }
        public int Temperature { get; set; }
        public Dictionary<INaturalResource, int> NaturalResources { get; set; }
        public bool AlwaysVisible { get; set; }

        public string DiscoveryName => throw new NotImplementedException();

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {

            BaseElevation = 0;
            TemperatureCoefficient = 0;
            PressureCoefficient = 0;
            Claimable = false;
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
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            throw new NotImplementedException();
        }

        public ILocale GenerateAdventure(string name = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ILocation> GetSurroundings(int strength)
        {
            throw new NotImplementedException();
        }

        public string MoveInto<T>(T thing)
        {
            throw new NotImplementedException();
        }

        public string MoveInto<T>(T thing, string containerName)
        {
            throw new NotImplementedException();
        }

        public string MoveFrom<T>(T thing)
        {
            throw new NotImplementedException();
        }

        public string MoveFrom<T>(T thing, string containerName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetContents<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetContents<T>(string containerName)
        {
            throw new NotImplementedException();
        }

        public int EffectiveHumidity()
        {
            throw new NotImplementedException();
        }

        public int EffectiveTemperature()
        {
            throw new NotImplementedException();
        }

        public bool IsOutside()
        {
            throw new NotImplementedException();
        }

        public Biome GetBiome()
        {
            throw new NotImplementedException();
        }

        public bool IsDiscovered(IEntity discoverer)
        {
            throw new NotImplementedException();
        }

        public void GetFromWorldOrSpawn()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IZone> ZoneExits()
        {
            throw new NotImplementedException();
        }
    }
}
