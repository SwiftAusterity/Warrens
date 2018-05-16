using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Game
{
    public abstract class LocationEntityPartial : EntityPartial, ILocation
    {
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
            return MoveInto(thing, string.Empty);
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

                if (!obj.TryMoveInto(this))
                    return "Unable to move into that container.";

                Contents.Add(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (MobilesInside.Contains(obj, containerName))
                    return "That is already in the container";

                if (!obj.TryMoveInto(this))
                    return "Unable to move into that container.";

                MobilesInside.Add(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IPathway)))
            {
                var obj = (IPathway)thing;

                if (Pathways.Contains(obj, containerName))
                    return "That is already in the container";

                if (!obj.TryMoveInto(this))
                    return "Unable to move into that container.";

                Pathways.Add(obj, containerName);
                UpsertToLiveWorldCache();

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
            return MoveFrom(thing, string.Empty);
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

                obj.TryMoveInto(null);
                Contents.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (!MobilesInside.Contains(obj, containerName))
                    return "That is not in the container";

                obj.TryMoveInto(null);
                MobilesInside.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IPathway)))
            {
                var obj = (IPathway)thing;

                if (!Pathways.Contains(obj, containerName))
                    return "That is not in the container";

                obj.TryMoveInto(null);
                Pathways.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        /// <summary>
        /// Get the surrounding locations based on a strength radius
        /// </summary>
        /// <param name="strength">number of places to go out</param>
        /// <returns>list of valid surrounding locations</returns>
        public virtual IEnumerable<ILocation> GetSurroundings(int strength)
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

        public virtual int Humidity { get; set; }

        public virtual int Temperature { get; set; }

        public virtual int EffectiveHumidity()
        {
            //TODO: More stuff
            return Humidity;
        }

        public virtual int EffectiveTemperature()
        {
            //TODO: More Stuff
            return Temperature;
        }

        public virtual bool IsOutside()
        {
            return false;
        }

        public virtual Biome GetBiome()
        {
            return Biome.Fabricated;
        }

        Dictionary<INaturalResource, int> IEnvironment.NaturalResources { get; set; }
    }
}
