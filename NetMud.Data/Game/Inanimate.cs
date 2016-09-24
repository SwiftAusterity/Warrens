using NetMud.Data.Reference;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// "Object" class
    /// </summary>
    [Serializable]
    public class Inanimate : EntityPartial, IInanimate
    {
        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Last known location this was loaded to
        /// </summary>
        public long LastKnownLocation { get; set; }

        /// <summary>
        /// Last known location type this was loaded to
        /// </summary>
        public string LastKnownLocationType { get; set; }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public new IInanimateData DataTemplate
        {
            get
            {
                return BackingDataCache.Get<IInanimateData>(DataTemplateId);
            }
            internal set
            {
                DataTemplateId = value.ID;
            }
        }

        [JsonConstructor]
        public Inanimate(DimensionalModel model)
        {
            Model = model;

            //IDatas need parameterless constructors
            Contents = new EntityContainer<IInanimate>();
            Pathways = new EntityContainer<IPathway>();
            MobilesInside = new EntityContainer<IMobile>();
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Inanimate()
        {
            //IDatas need parameterless constructors
            Contents = new EntityContainer<IInanimate>();
            Pathways = new EntityContainer<IPathway>();
            MobilesInside = new EntityContainer<IMobile>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Inanimate(IInanimateData backingStore)
        {
            Contents = new EntityContainer<IInanimate>(backingStore.InanimateContainers);
            Pathways = new EntityContainer<IPathway>();
            MobilesInside = new EntityContainer<IMobile>(backingStore.MobileContainers);

            DataTemplate = backingStore;
            SpawnNewInWorld();
        }

        /// <summary>
        /// News up an entity with its backing data and where to spawn it into
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        /// <param name="spawnTo">where to spawn this into</param>
        public Inanimate(IInanimateData backingStore, IContains spawnTo)
        {
            Contents = new EntityContainer<IInanimate>(backingStore.InanimateContainers);
            Pathways = new EntityContainer<IPathway>();
            MobilesInside = new EntityContainer<IMobile>(backingStore.MobileContainers);

            DataTemplate = backingStore;
            SpawnNewInWorld(spawnTo);
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
        }

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Contents { get; set; }

        /// <summary>
        /// Pathways leading out of this
        /// </summary>
        public IEntityContainer<IPathway> Pathways { get; set; }

        /// <summary>
        /// Any mobiles (players, npcs) contained in this
        /// </summary>
        public IEntityContainer<IMobile> MobilesInside { get; set; }

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
                contents.AddRange(MobilesInside.EntitiesContained().Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Contents.EntitiesContained().Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IPathway)))
                contents.AddRange(Pathways.EntitiesContained().Select(ent => (T)ent));

            return contents;
        }

        /// <summary>
        /// Get all of the entities matching a type inside this in a named container
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        /// <param name="containerName">the name of the container</param>
        public IEnumerable<T> GetContents<T>(string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
                contents.AddRange(MobilesInside.EntitiesContained(containerName).Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Contents.EntitiesContained(containerName).Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IPathway)))
                contents.AddRange(Pathways.EntitiesContained(containerName).Select(ent => (T)ent));

            return contents;
        }

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing)
        {
            return MoveInto<T>(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (Contents.Contains(obj, containerName))
                    return "That is already in the container";

                Contents.Add(obj, containerName);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (MobilesInside.Contains(obj, containerName))
                    return "That is already in the container";

                MobilesInside.Add(obj, containerName);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IPathway)))
            {
                var obj = (IPathway)thing;

                if (Pathways.Contains(obj, containerName))
                    return "That is already in the container";

                Pathways.Add(obj, containerName);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move to container.";
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing)
        {
            return MoveFrom<T>(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity out of this' named container
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (!Contents.Contains(obj, containerName))
                    return "That is not in the container";

                Contents.Remove(obj, containerName);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }


            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (!MobilesInside.Contains(obj, containerName))
                    return "That is not in the container";

                MobilesInside.Remove(obj, containerName);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IPathway)))
            {
                var obj = (IPathway)thing;

                if (!Pathways.Contains(obj, containerName))
                    return "That is not in the container";

                Pathways.Remove(obj, containerName);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        #region spawning
        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException("Objects can't spawn to nothing");
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //We can't even try this until we know if the data is there
            if (DataTemplate == null)
                throw new InvalidOperationException("Missing backing data store on object spawn event.");

            var bS = (IInanimateData)DataTemplate;

            BirthMark = LiveCache.GetUniqueIdentifier(bS);
            Keywords = new string[] { bS.Name.ToLower() };
            Birthdate = DateTime.Now;

            if (spawnTo == null)
            {
                throw new NotImplementedException("Objects can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveInto<IInanimate>(this);

            Contents = new EntityContainer<IInanimate>();

            LiveCache.Add(this);
        }
        #endregion

        #region rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();
            var backingStore = (IInanimateData)DataTemplate;

            sb.Add(string.Format("There is a {0} here", backingStore.Name));

            return sb;
        }

        /// <summary>
        /// Get the surrounding locations based on a strength radius
        /// </summary>
        /// <param name="strength">number of places to go out</param>
        /// <returns>list of valid surrounding locations</returns>
        public IEnumerable<ILocation> GetSurroundings(int strength)
        {
            var radiusLocations = new List<ILocation>();

            //If we don't have any paths out what can we even do
            if (Pathways.Count() == 0)
                return radiusLocations;

            var currentRadius = 0;
            var currentPathsSet = Pathways.EntitiesContained();
            while (currentRadius <= strength && currentPathsSet.Count() > 0)
            {
                var currentLocsSet = currentPathsSet.Select(path => path.ToLocation);

                if (currentLocsSet.Count() == 0)
                    break;

                radiusLocations.AddRange(currentLocsSet);
                currentPathsSet = currentLocsSet.SelectMany(ro => ro.Pathways.EntitiesContained());

                currentRadius++;
            }

            return radiusLocations;
        }
        #endregion
    }
}
